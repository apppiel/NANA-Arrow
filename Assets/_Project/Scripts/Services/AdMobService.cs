using GoogleMobileAds.Api;
using NanaArrow.Core;
using UnityEngine;

namespace NanaArrow.Services
{
    /// <summary>
    /// AdMob 전면·보상형 (NO.3 AdMobService 이식, GAME_RULES §7 "AdMob 단독 + Unity Ads 미디에이션"). 첫 씬 로드 전에 스스로 생성해 App.SetAds 로 등록.
    /// 판단(언제 띄우나)은 Core AdsController/AdsManager — 여기는 초기화·로드·표시 프리미티브만.
    ///
    /// 스레드: GMA 플러그인은 광고 이벤트를 메인 스레드 보장 없이 올린다 → 콜백은 volatile 플래그만 세우고 처리는 Update(메인 스레드)에서.
    /// 보상은 OnUserEarnedReward 콜백이 발화했을 때만 true (중간에 닫으면 false).
    /// 에디터: 광고를 띄우지 않는다 — 전면은 즉시 false(스킵), 보상형은 시청 완료(true)로 처리 (#if UNITY_EDITOR 격리, 플레이어 빌드에 없음).
    /// 광고 단위 ID: Development Build 면 Google 테스트 ID. 릴리즈 실제 ID 는 아직 비어 있음 → 테스트 ID 폴백 + 경고. 출시 전 채울 것.
    /// </summary>
    public sealed class AdMobService : MonoBehaviour, IInterstitialAd, IRewardedAd
    {
        public static AdMobService Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject(nameof(AdMobService));
            DontDestroyOnLoad(go);
            go.AddComponent<AdMobService>();
        }

        // Google 공식 테스트 ID (https://developers.google.com/admob/unity/test-ads)
#if UNITY_ANDROID
        private const string TestInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        private const string TestRewardedId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS
        private const string TestInterstitialId = "ca-app-pub-3940256099942544/4411468910";
        private const string TestRewardedId = "ca-app-pub-3940256099942544/1712485313";
#else
        private const string TestInterstitialId = "unused";
        private const string TestRewardedId = "unused";
#endif

        // 실제 광고 단위 ID — 미발급 (팀장 AdMob 콘솔 작업 뒤 별도 커밋). 앱 ID(GoogleMobileAdsSettings, androidlib 매니페스트)도 같이.
        private const string RealInterstitialId = "";
        private const string RealRewardedId = "";

        // 전면광고 close 이벤트가 영영 안 오는 경우의 안전장치. 보상형에는 걸지 않는다 (늦게 오는 보상 콜백을 버리면 유저 손해).
        private const float InterstitialSafetyTimeoutSeconds = 90f;

        private string _interstitialAdUnitId;
        private string _rewardedAdUnitId;

        private volatile InterstitialAd _interstitialAd;
        private volatile RewardedAd _rewardedAd;
        private volatile bool _sdkInitialized;
        private volatile bool _pendingInterstitialReload;
        private volatile bool _pendingRewardedReload;
        private volatile bool _fullScreenClosed;
        private volatile bool _rewardEarned;

        private AwaitableCompletionSource<bool> _fullScreen;
        private bool _fullScreenIsRewarded;
        private float _fullScreenStartedAt;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            App.SetAds(this, this);

            _interstitialAdUnitId = ResolveAdUnitId(TestInterstitialId, RealInterstitialId, "전면");
            _rewardedAdUnitId = ResolveAdUnitId(TestRewardedId, RealRewardedId, "보상형");

#if UNITY_IOS && !UNITY_EDITOR
            MobileAds.SetiOSAppPauseOnBackground(true);
#endif
            MobileAds.Initialize(_ => { _sdkInitialized = true; });
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            App.SetAds(null, null);
        }

        private static string ResolveAdUnitId(string testId, string realId, string label)
        {
            if (Debug.isDebugBuild) return testId;
            if (!string.IsNullOrEmpty(realId)) return realId;
            Debug.LogWarning($"[AdMob] {label} 실제 광고 단위 ID 미설정 — 릴리즈 빌드가 테스트 ID로 동작합니다. 출시 전 AdMobService 상수를 채울 것.");
            return testId;
        }

        private void Update()
        {
            if (_sdkInitialized)
            {
                _sdkInitialized = false;
                _pendingInterstitialReload = true;
                _pendingRewardedReload = true;
                Debug.Log("[AdMob] SDK 초기화 완료 → 전면·보상형 프리로드");
            }
            if (_pendingInterstitialReload)
            {
                _pendingInterstitialReload = false;
                LoadInterstitial();
            }
            if (_pendingRewardedReload)
            {
                _pendingRewardedReload = false;
                LoadRewarded();
            }

            if (_fullScreen == null) return;
            if (_fullScreenClosed)
            {
                CompleteFullScreen(_fullScreenIsRewarded && _rewardEarned);
            }
            else if (!_fullScreenIsRewarded && Time.realtimeSinceStartup - _fullScreenStartedAt > InterstitialSafetyTimeoutSeconds)
            {
                Debug.LogWarning($"[AdMob] 전면광고 close 이벤트 {InterstitialSafetyTimeoutSeconds:F0}초 미수신 — 안전장치로 진행합니다.");
                CompleteFullScreen(false);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus || _fullScreen != null) return;
            if (_interstitialAd == null) _pendingInterstitialReload = true;
            if (_rewardedAd == null) _pendingRewardedReload = true;
        }

        private AwaitableCompletionSource<bool> BeginFullScreen(bool isRewarded)
        {
            _fullScreen = new AwaitableCompletionSource<bool>();
            _fullScreenIsRewarded = isRewarded;
            _fullScreenStartedAt = Time.realtimeSinceStartup;
            _fullScreenClosed = false;
            _rewardEarned = false;
            return _fullScreen;
        }

        private void CompleteFullScreen(bool result)
        {
            var source = _fullScreen;
            _fullScreen = null;
            _fullScreenClosed = false;
            _rewardEarned = false;
            if (_fullScreenIsRewarded) _pendingRewardedReload = true;
            else _pendingInterstitialReload = true;
            source.SetResult(result);
        }

        private void OnFullScreenClosed() { _fullScreenClosed = true; }
        private void OnFullScreenFailed(AdError _) { _fullScreenClosed = true; }

        private static Awaitable<bool> Completed(bool value)
        {
            var s = new AwaitableCompletionSource<bool>();
            s.SetResult(value);
            return s.Awaitable;
        }

        private void LoadInterstitial()
        {
            _interstitialAd?.Destroy();
            _interstitialAd = null;
            InterstitialAd.Load(_interstitialAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[AdMob] 전면광고 로드 실패: {error?.GetMessage()}");
                    return;
                }
                _interstitialAd = ad;
            });
        }

        private void LoadRewarded()
        {
            _rewardedAd?.Destroy();
            _rewardedAd = null;
            RewardedAd.Load(_rewardedAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[AdMob] 보상형 광고 로드 실패: {error?.GetMessage()}");
                    return;
                }
                _rewardedAd = ad;
            });
        }

        bool IInterstitialAd.IsReady
        {
            get
            {
#if UNITY_EDITOR
                return false;
#else
                var ad = _interstitialAd;
                return _fullScreen == null && ad != null && ad.CanShowAd();
#endif
            }
        }

        Awaitable<bool> IInterstitialAd.ShowAsync()
        {
#if UNITY_EDITOR
            Debug.Log("[AdMob] 에디터 — 전면광고 스킵 (즉시 진행)");
            return Completed(false);
#else
            if (_fullScreen != null) return Completed(false);
            var ad = _interstitialAd;
            if (ad == null || !ad.CanShowAd())
            {
                _pendingInterstitialReload = true;
                return Completed(false);
            }
            var source = BeginFullScreen(isRewarded: false);
            ad.OnAdFullScreenContentClosed += OnFullScreenClosed;
            ad.OnAdFullScreenContentFailed += OnFullScreenFailed;
            ad.Show();
            return source.Awaitable;
#endif
        }

        bool IRewardedAd.IsReady
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                var ad = _rewardedAd;
                return _fullScreen == null && ad != null && ad.CanShowAd();
#endif
            }
        }

        Awaitable<bool> IRewardedAd.ShowAsync()
        {
#if UNITY_EDITOR
            Debug.Log("[AdMob] 에디터 — 보상형 광고 스킵, 시청 완료로 처리 (플레이어 빌드에는 이 경로가 없다)");
            return Completed(true);
#else
            if (_fullScreen != null) return Completed(false);
            var ad = _rewardedAd;
            if (ad == null || !ad.CanShowAd())
            {
                _pendingRewardedReload = true;
                return Completed(false);
            }
            var source = BeginFullScreen(isRewarded: true);
            ad.OnAdFullScreenContentClosed += OnFullScreenClosed;
            ad.OnAdFullScreenContentFailed += OnFullScreenFailed;
            ad.Show(_ => { _rewardEarned = true; });
            return source.Awaitable;
#endif
        }
    }
}
