using Firebase.Analytics;
using NanaArrow.Core;
using UnityEngine;

namespace NanaArrow.Services
{
    /// <summary>
    /// GameEvents → Firebase Analytics (ANALYTICS.md §1: 이벤트 호출은 여기 한 곳에서만, SafeAnalytics 경유).
    /// 첫 씬 로드 전에 스스로 생성. 에디터·개발 빌드는 기본적으로 보내지 않는다 (<see cref="sendInEditor"/>).
    /// 광고·튜토리얼·메뉴 이벤트는 해당 기능(W-011/W-017)이 GameEvents 에 추가되면 여기서 구독한다.
    /// </summary>
    public sealed class AnalyticsReporter : MonoBehaviour
    {
        [SerializeField, Tooltip("에디터·Development Build 에서도 전송 (기본 끔)")] private bool sendInEditor;

        public static AnalyticsReporter Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject(nameof(AnalyticsReporter));
            DontDestroyOnLoad(go);
            go.AddComponent<AnalyticsReporter>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            GameEvents.LevelStarted += OnLevelStarted;
            GameEvents.LevelCleared += OnLevelCleared;
            GameEvents.LevelFailed += OnLevelFailed;
            GameEvents.RetryPressed += OnRetryPressed;
            GameEvents.LevelQuit += OnLevelQuit;
            GameEvents.TutorialStepShown += (level, key, trigger) => Send(AnalyticsEvents.TutorialStep,
                new Parameter(AnalyticsEvents.ParamLevel, level), new Parameter(AnalyticsEvents.ParamTextKey, key), new Parameter(AnalyticsEvents.ParamTrigger, trigger));
            GameEvents.TutorialDone += (level, key, elapsed) => Send(AnalyticsEvents.TutorialDone,
                new Parameter(AnalyticsEvents.ParamLevel, level), new Parameter(AnalyticsEvents.ParamTextKey, key), new Parameter(AnalyticsEvents.ParamElapsedSec, (long)elapsed));
            GameEvents.LanePreviewFirst += level => Send(AnalyticsEvents.LanePreviewFirst, new Parameter(AnalyticsEvents.ParamLevel, level));
            GameEvents.LevelSelectOpened += highest => Send(AnalyticsEvents.LevelSelectOpen, new Parameter(AnalyticsEvents.ParamHighestLevel, highest));
            GameEvents.ContinueOffered += (s, adReady) => Send(AnalyticsEvents.ContinueOffer, Common(s, new Parameter(AnalyticsEvents.ParamAdReady, adReady ? 1 : 0)));
            GameEvents.ContinueRequested += s => Send(AnalyticsEvents.ContinueRequest, Common(s, new Parameter(AnalyticsEvents.ParamContinuesUsed, s.ContinuesUsed)));
            GameEvents.ContinueGranted += (s, livesAfter) => Send(AnalyticsEvents.ContinueGranted, Common(s, new Parameter(AnalyticsEvents.ParamLivesAfter, livesAfter)));
            GameEvents.InterstitialDecided += (trigger, result, level) => Send(AnalyticsEvents.AdInterstitial,
                new Parameter(AnalyticsEvents.ParamTrigger, trigger), new Parameter(AnalyticsEvents.ParamResult, result), new Parameter(AnalyticsEvents.ParamLevel, level));
            GameEvents.RewardedFinished += (result, level) => Send(AnalyticsEvents.AdRewardedResult,
                new Parameter(AnalyticsEvents.ParamResult, AnalyticsEvents.SnakeCase(result)), new Parameter(AnalyticsEvents.ParamLevel, level));
            GameEvents.RewardCodeIssued += synced =>
            {
                Send(AnalyticsEvents.RewardCodeIssued, new Parameter(AnalyticsEvents.ParamSynced, synced ? 1 : 0));
                SetProperty(AnalyticsEvents.PropRewardIssued, "1");
            };
            GameEvents.RewardCodeCopied += () => Send(AnalyticsEvents.RewardCodeCopy);
            GameEvents.RewardLinkOpened += () => Send(AnalyticsEvents.RewardLinkOpen);
            SettingsStore.SoundChanged += on => Send(AnalyticsEvents.SettingsChange, new Parameter(AnalyticsEvents.ParamSetting, "sound"), new Parameter(AnalyticsEvents.ParamValue, on ? 1 : 0));
            SettingsStore.VibrationChanged += on => Send(AnalyticsEvents.SettingsChange, new Parameter(AnalyticsEvents.ParamSetting, "vibration"), new Parameter(AnalyticsEvents.ParamValue, on ? 1 : 0));
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            GameEvents.LevelStarted -= OnLevelStarted;
            GameEvents.LevelCleared -= OnLevelCleared;
            GameEvents.LevelFailed -= OnLevelFailed;
            GameEvents.RetryPressed -= OnRetryPressed;
            GameEvents.LevelQuit -= OnLevelQuit;
        }

        private bool CanSend => sendInEditor || (!Application.isEditor && !Debug.isDebugBuild);

        /// <summary>전송 지점은 여기 하나.</summary>
        public void Send(string eventName, params Parameter[] parameters)
        {
            if (!CanSend) return;
            SafeAnalytics.LogEvent(eventName, parameters);
        }

        public void SetProperty(string name, string value)
        {
            if (!CanSend) return;
            SafeAnalytics.SetUserProperty(name, value);
        }

        private void OnLevelStarted(LevelStats s)
        {
            Send(AnalyticsEvents.LevelStart, Common(s, new Parameter(AnalyticsEvents.ParamStartReason, AnalyticsEvents.SnakeCase(s.StartReason))));
        }

        private void OnLevelCleared(LevelStats s, int livesLeft)
        {
            Send(AnalyticsEvents.LevelClear, Common(s,
                new Parameter(AnalyticsEvents.ParamDurationSec, (long)s.DurationSec),
                new Parameter(AnalyticsEvents.ParamTaps, s.Taps),
                new Parameter(AnalyticsEvents.ParamBlocks, s.Blocks),
                new Parameter(AnalyticsEvents.ParamLivesLeft, livesLeft),
                new Parameter(AnalyticsEvents.ParamContinuesUsed, s.ContinuesUsed),
                new Parameter(AnalyticsEvents.ParamLanePreviews, s.LanePreviews)));
            SetProperty(AnalyticsEvents.PropHighestLevel, App.Progress.HighestClearedLevel.ToString());
        }

        private void OnLevelFailed(LevelStats s, int arrowsLeft)
        {
            Send(AnalyticsEvents.LevelFail, Common(s,
                new Parameter(AnalyticsEvents.ParamDurationSec, (long)s.DurationSec),
                new Parameter(AnalyticsEvents.ParamTaps, s.Taps),
                new Parameter(AnalyticsEvents.ParamBlocks, s.Blocks),
                new Parameter(AnalyticsEvents.ParamArrowsLeft, arrowsLeft),
                new Parameter(AnalyticsEvents.ParamContinuesUsed, s.ContinuesUsed)));
        }

        private void OnRetryPressed(LevelStats s, LevelStartReason source, int arrowsLeft)
        {
            Send(AnalyticsEvents.Retry, Common(s,
                new Parameter(AnalyticsEvents.ParamRetrySource, source == LevelStartReason.RetryFail ? "fail_popup" : "hud"),
                new Parameter(AnalyticsEvents.ParamTaps, s.Taps),
                new Parameter(AnalyticsEvents.ParamArrowsLeft, arrowsLeft)));
        }

        private void OnLevelQuit(LevelStats s, bool appPause, int arrowsLeft, int livesLeft)
        {
            Send(AnalyticsEvents.LevelQuit, Common(s,
                new Parameter(AnalyticsEvents.ParamDurationSec, (long)s.DurationSec),
                new Parameter(AnalyticsEvents.ParamTaps, s.Taps),
                new Parameter(AnalyticsEvents.ParamArrowsLeft, arrowsLeft),
                new Parameter(AnalyticsEvents.ParamLivesLeft, livesLeft),
                new Parameter(AnalyticsEvents.ParamQuitReason, appPause ? "app_pause" : "menu")));
        }

        /// <summary>ANALYTICS §2 공통 파라미터 + 추가분.</summary>
        private static Parameter[] Common(LevelStats s, params Parameter[] extra)
        {
            var all = new Parameter[6 + extra.Length];
            all[0] = new Parameter(AnalyticsEvents.ParamLevel, s.Level);
            all[1] = new Parameter(AnalyticsEvents.ParamLevelName, AnalyticsEvents.LevelName(s.Level));
            all[2] = new Parameter(AnalyticsEvents.ParamBoard, AnalyticsEvents.Board(s.BoardWidth, s.BoardHeight));
            all[3] = new Parameter(AnalyticsEvents.ParamArrows, s.Arrows);
            all[4] = new Parameter(AnalyticsEvents.ParamIsReplay, s.IsReplay ? 1 : 0);
            all[5] = new Parameter(AnalyticsEvents.ParamAttempt, s.Attempt);
            extra.CopyTo(all, 6);
            return all;
        }
    }
}
