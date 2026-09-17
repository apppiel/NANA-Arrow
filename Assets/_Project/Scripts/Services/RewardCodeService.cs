using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using NanaArrow.Core;
using UnityEngine;

namespace NanaArrow.Services
{
    /// <summary>
    /// 응모 코드 발급 · 서버 저장 (GAME_RULES §7, NO.3 RewardCodeService 이식). 첫 씬 로드 전에 스스로 생성.
    /// 원칙 (NO.1→NO.3 실기에서 물려받음): ① 어떤 경우에도 유저는 코드를 받는다 (서버 실패 → 로컬 발급)
    /// ② 로컬이 진짜 소스, Firestore 는 홈페이지 검증용 사본 — 저장 순서도 로컬 먼저 ③ 미동기화면 앱 실행마다 조용히 재시도
    /// ④ 모든 Firebase 대기에 타임아웃 (CheckAndFixDependenciesAsync 가 hang 하는 환경 대응).
    /// 저장은 PlayerPrefs (재화가 아니라 증명서; 진짜 검증자는 서버). 발급 여부 플래그만 SaveData(<see cref="PlayerProgress.MarkRewardCodeIssued"/>) 로 — Main 의 버튼 표시용.
    /// 에디터: Firebase 네이티브가 없어 초기화가 늘 실패 → 로컬 발급 경로 (정상).
    /// </summary>
    public sealed class RewardCodeService : MonoBehaviour, IRewardCodeService
    {
        public static RewardCodeService Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject(nameof(RewardCodeService));
            DontDestroyOnLoad(go);
            go.AddComponent<RewardCodeService>();
        }

#if UNITY_EDITOR
        [Header("에디터 전용 (빌드에는 영향 없음)")]
        [SerializeField, Tooltip("Firebase 초기화 실패로 강제 처리 → 로컬 발급 흐름 검증")] private bool editorSimulateInitFail;
        [SerializeField, Tooltip("Firestore 저장 실패로 강제 처리 → 미동기화 안내·재시도 검증")] private bool editorSimulateSaveFail;
#endif

        private const string PrefsKeyCode = "nanaarrow.reward.code";
        private const string PrefsKeySynced = "nanaarrow.reward.synced";
        private const string CollectionRewards = "rewards";
        private const string CollectionCodeIndex = "code_index";

        private const float InitTimeoutSeconds = 12f;
        private const float QueryTimeoutSeconds = 10f;
        private const float SaveTimeoutSeconds = 10f;
        private const int InitRetryCount = 30;

        private static readonly WaitForSecondsRealtime OneSecondWait = new WaitForSecondsRealtime(1f);

        private FirebaseFirestore _db;
        private Task<DependencyStatus> _initTask;
        private bool _initUnavailable; // 초기화 자체가 불가(네이티브 없음 등) → 대기 없이 로컬 발급
        private bool _isProcessing;
        private System.Random _rng;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _rng = new System.Random();
            App.SetRewardCodes(this);
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            App.SetRewardCodes(null);
        }

        private void Start()
        {
            StartCoroutine(InitializeFirebaseCoroutine());
            if (!string.IsNullOrEmpty(GetLocalCode()) && !IsLocalCodeSynced())
                StartCoroutine(StartupBackgroundSync());
        }

        /// <summary>로컬에 저장된 코드. 없거나 형식이 깨졌으면 빈 문자열.</summary>
        public string GetLocalCode()
        {
            var code = PlayerPrefs.GetString(PrefsKeyCode, "");
            return RewardCode.IsWellFormed(code) ? code : "";
        }

        /// <summary>서버 저장 성공 여부. false 면 홈페이지 검증에 아직 못 쓴다.</summary>
        public bool IsLocalCodeSynced() => PlayerPrefs.GetInt(PrefsKeySynced, 0) == 1;

        /// <summary>응모 코드 팝업이 열릴 때. 로컬 코드 유무·동기화 상태에 따라 알아서 갈린다. 중복 호출은 무시.</summary>
        public void IssueCode()
        {
            if (_isProcessing) return;
            _isProcessing = true;

            var localCode = GetLocalCode();
            if (!string.IsNullOrEmpty(localCode))
            {
                if (IsLocalCodeSynced())
                {
                    Notify(localCode, RewardCodeStatus.Reissued);
                    _isProcessing = false;
                }
                else
                {
                    Notify(localCode, RewardCodeStatus.Saving);
                    StartCoroutine(SyncExistingCodeCoroutine(localCode));
                }
                return;
            }

            Notify("", RewardCodeStatus.Checking);
            StartCoroutine(IssueFlowCoroutine());
        }

        private IEnumerator InitializeFirebaseCoroutine()
        {
            var fatal = false;
            for (var attempt = 1; attempt <= InitRetryCount && _initTask == null && !fatal; attempt++)
            {
                try
                {
                    _initTask = FirebaseApp.CheckAndFixDependenciesAsync();
                }
                catch (InvalidOperationException)
                {
                    // 다른 쪽(Analytics)이 아직 초기화 중. 1초 뒤 재시도.
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[RewardCode] Firebase 초기화 호출 예외 (로컬 발급으로 진행): " + e.Message);
                    fatal = true;
                }
                if (_initTask == null && !fatal) yield return OneSecondWait;
            }

            if (_initTask == null)
            {
                _initUnavailable = true;
                Debug.LogWarning("[RewardCode] Firebase 초기화 불가 — 이후 흐름은 로컬 발급으로 대체된다");
                yield break;
            }

            _initTask.ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.Result != DependencyStatus.Available)
                {
                    Debug.LogWarning("[RewardCode] Firebase 초기화 실패: " + (task.IsFaulted ? task.Exception?.Message : task.Result.ToString()));
                    return;
                }
                _db = FirebaseFirestore.DefaultInstance;
            });
        }

        private IEnumerator IssueFlowCoroutine()
        {
            yield return WaitForInitReady(InitTimeoutSeconds);
            if (!IsFirebaseUsable())
            {
                FallbackIssueLocal();
                yield break;
            }

            var deviceId = SystemInfo.deviceUniqueIdentifier;
            var getTask = _db.Collection(CollectionRewards).Document(deviceId).GetSnapshotAsync();
            yield return WaitForTaskOrTimeout(getTask, QueryTimeoutSeconds);

            if (!getTask.IsCompleted || getTask.IsFaulted || getTask.IsCanceled)
            {
                Debug.LogWarning("[RewardCode] rewards 조회 실패: " + (getTask.Exception?.Message ?? "타임아웃"));
                FallbackIssueLocal();
                yield break;
            }

            if (getTask.Result.Exists)
            {
                // 같은 기기가 다시 완주 — 서버 코드 재사용 (기기당 1개)
                var existing = getTask.Result.GetValue<string>("code");
                SaveLocalCode(existing, synced: true);
                Notify(existing, RewardCodeStatus.Reissued);
                _isProcessing = false;
                yield break;
            }

            var newCode = RewardCode.Generate(max => _rng.Next(max));
            SaveLocalCode(newCode, synced: false);
            Notify(newCode, RewardCodeStatus.Saving);
            yield return SaveToServerCoroutine(newCode, deviceId, isForeground: true);
        }

        /// <summary>rewards + code_index 를 WriteBatch 로 원자적 커밋 (한쪽만 저장되면 홈페이지에서 조회 안 되는 유령 코드).</summary>
        private IEnumerator SaveToServerCoroutine(string code, string deviceId, bool isForeground)
        {
            var rewardData = new Dictionary<string, object>
            {
                { "code", code }, { "claimed", false }, { "deviceId", deviceId }, { "createdAt", FieldValue.ServerTimestamp },
            };
            var indexData = new Dictionary<string, object>
            {
                { "deviceId", deviceId }, { "createdAt", FieldValue.ServerTimestamp },
            };
            var batch = _db.StartBatch();
            batch.Set(_db.Collection(CollectionRewards).Document(deviceId), rewardData);
            batch.Set(_db.Collection(CollectionCodeIndex).Document(code), indexData);
            var saveTask = batch.CommitAsync();
            yield return WaitForTaskOrTimeout(saveTask, SaveTimeoutSeconds);

            var ok = saveTask.IsCompleted && !saveTask.IsFaulted && !saveTask.IsCanceled;
#if UNITY_EDITOR
            if (editorSimulateSaveFail) ok = false;
#endif
            if (ok)
            {
                SaveLocalCode(code, synced: true);
                GameEvents.RaiseRewardCodeIssued(true);
                if (isForeground) Notify(code, RewardCodeStatus.Issued);
            }
            else
            {
                Debug.LogWarning("[RewardCode] 저장 실패: " + (saveTask.Exception?.Message ?? "타임아웃"));
                if (isForeground)
                {
                    GameEvents.RaiseRewardCodeIssued(false);
                    Notify(code, RewardCodeStatus.SaveFailed);
                }
            }
            if (isForeground) _isProcessing = false;
        }

        private IEnumerator SyncExistingCodeCoroutine(string code)
        {
            yield return WaitForInitReady(InitTimeoutSeconds);
            if (!IsFirebaseUsable())
            {
                Notify(code, RewardCodeStatus.Offline);
                _isProcessing = false;
                yield break;
            }
            yield return SaveToServerCoroutine(code, SystemInfo.deviceUniqueIdentifier, isForeground: true);
        }

        private IEnumerator StartupBackgroundSync()
        {
            yield return WaitForInitReady(InitTimeoutSeconds);
            if (!IsFirebaseUsable()) yield break;
            var code = GetLocalCode();
            if (string.IsNullOrEmpty(code) || IsLocalCodeSynced()) yield break;
            yield return SaveToServerCoroutine(code, SystemInfo.deviceUniqueIdentifier, isForeground: false);
        }

        private static IEnumerator WaitForTaskOrTimeout(Task task, float timeoutSec)
        {
            if (task == null) yield break;
            var elapsed = 0f;
            while (!task.IsCompleted && elapsed < timeoutSec)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private IEnumerator WaitForInitReady(float timeoutSec)
        {
            var elapsed = 0f;
            while (!_initUnavailable && (_initTask == null || !_initTask.IsCompleted) && elapsed < timeoutSec)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private bool IsFirebaseUsable()
        {
#if UNITY_EDITOR
            if (editorSimulateInitFail) return false;
#endif
            if (_initTask == null || !_initTask.IsCompleted) return false;
            if (_initTask.IsFaulted || _initTask.IsCanceled) return false;
            if (_initTask.Result != DependencyStatus.Available) return false;
            return _db != null;
        }

        private void FallbackIssueLocal()
        {
            var newCode = RewardCode.Generate(max => _rng.Next(max));
            SaveLocalCode(newCode, synced: false);
            GameEvents.RaiseRewardCodeIssued(false);
            Notify(newCode, RewardCodeStatus.Offline);
            _isProcessing = false;
        }

        private void SaveLocalCode(string code, bool synced)
        {
            PlayerPrefs.SetString(PrefsKeyCode, code);
            PlayerPrefs.SetInt(PrefsKeySynced, synced ? 1 : 0);
            PlayerPrefs.Save();
            App.Progress.MarkRewardCodeIssued();
        }

        private void Notify(string code, RewardCodeStatus status) => App.NotifyRewardCode(code, status);

#if UNITY_EDITOR
        /// <summary>에디터 전용 — 로컬 코드를 지워 신규 발급 흐름을 다시 재현 (치트 창 "저장 초기화" 와 함께 쓰면 됨).</summary>
        [ContextMenu("[TEST] 로컬 응모 코드 삭제")]
        private void TestClearLocalRewardCode()
        {
            PlayerPrefs.DeleteKey(PrefsKeyCode);
            PlayerPrefs.DeleteKey(PrefsKeySynced);
            PlayerPrefs.Save();
            Debug.Log("[RewardCode] 로컬 코드 삭제됨.");
        }
#endif
    }
}
