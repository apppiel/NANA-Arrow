using System;
using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>
    /// Game 씬의 광고 연결 지점 (UI_FLOW §8): 판단은 <see cref="AdsManager"/>(순수 C#), 표시는 <see cref="App.Interstitial"/>/<see cref="App.Rewarded"/>.
    /// 클리어 팝업 버튼 → <see cref="AfterLevelCleared"/>, 실패 팝업 → <see cref="RequestContinue"/>/<see cref="RetryFromFailPopup"/>.
    /// 전면 광고가 준비 안 됐으면 기다리지 않고 바로 진행. 광고 표시 중 BGM 일시정지.
    /// 같은 레벨 실패 횟수와 레벨당 이어하기 횟수는 앱 세션 안에서만 센다.
    /// </summary>
    public sealed class AdsController : MonoBehaviour
    {
        private const string TriggerClear = "clear";
        private const string TriggerFailRetry = "fail_retry";
        private const string ResultShown = "shown";
        private const string ResultNotReady = "not_ready";
        private const string ResultSkippedFreeLevel = "skipped_free_level";

        [SerializeField] private AdsConfig adsConfig;
        [SerializeField] private GameController gameController;

        private readonly Dictionary<int, int> _failsByLevel = new Dictionary<int, int>();
        private AdsManager _ads;
        private bool _showing;

        /// <summary>이번 판에서 쓴 이어하기 횟수.</summary>
        public int ContinuesUsed { get; private set; }
        /// <summary>이어하기 버튼을 보여도 되는가 (레벨당 maxContinues).</summary>
        public bool CanOfferContinue => _ads.ContinueRequested(ContinuesUsed);
        /// <summary>보상형 광고 준비 여부 (버튼 활성 조건).</summary>
        public bool IsRewardedReady => !_showing && App.Rewarded != null && App.Rewarded.IsReady;
        /// <summary>광고 표시 중 (버튼 중복 탭 방지).</summary>
        public bool IsShowing => _showing;

        private int Level => gameController.Stats?.Level ?? gameController.CurrentLevel;

        private void Awake()
        {
            _ads = AdsManager.FromConfig(adsConfig);
            gameController.SessionStarted += OnSessionStarted;
            GameEvents.LevelFailed += OnLevelFailed;
        }

        private void OnDestroy()
        {
            gameController.SessionStarted -= OnSessionStarted;
            GameEvents.LevelFailed -= OnLevelFailed;
        }

        private void OnSessionStarted(GameSession session) => ContinuesUsed = 0;

        private void OnLevelFailed(LevelStats stats, int arrowsLeft)
        {
            _failsByLevel.TryGetValue(stats.Level, out var fails);
            _failsByLevel[stats.Level] = fails + 1;
        }

        /// <summary>클리어 팝업의 두 버튼: 전면 판정 → (해당 시) 광고 → <paramref name="then"/>.</summary>
        public void AfterLevelCleared(Action then)
        {
            var stats = gameController.Stats;
            var verdict = _ads.LevelCleared(Level, stats != null && stats.IsReplay);
            ShowInterstitialThen(TriggerClear, verdict, then);
        }

        /// <summary>실패 팝업 `다시하기`: 같은 레벨 실패 횟수로 전면 판정 → 재시작 (실패 횟수에 포함).</summary>
        public void RetryFromFailPopup()
        {
            _failsByLevel.TryGetValue(Level, out var fails);
            var verdict = _ads.RetryPressed(fails);
            ShowInterstitialThen(TriggerFailRetry, verdict, gameController.RestartFromFailPopup);
        }

        /// <summary>실패 팝업이 열릴 때 (continue_offer).</summary>
        public void ReportContinueOffered() => GameEvents.RaiseContinueOffered(gameController.Stats, IsRewardedReady);

        /// <summary>
        /// 실패 팝업 `광고 보고 이어하기`: 보상형 광고 → 시청 완료 시에만 목숨 회복. <paramref name="done"/>(true) 면 팝업을 닫고, false 면 팝업 유지.
        /// </summary>
        public async void RequestContinue(Action<bool> done)
        {
            if (_showing || !CanOfferContinue) { done?.Invoke(false); return; }
            GameEvents.RaiseContinueRequested(gameController.Stats);

            var rewarded = App.Rewarded;
            if (rewarded == null || !rewarded.IsReady)
            {
                GameEvents.RaiseRewardedFinished(RewardedResult.FailedToShow, Level);
                done?.Invoke(false);
                return;
            }

            var earned = false;
            _showing = true;
            AudioManager.Instance?.PauseBgm(true);
            try { earned = await rewarded.ShowAsync(); }
            catch (Exception e) { Debug.LogWarning("[Ads] 보상형 광고 예외: " + e.Message); }
            finally
            {
                _showing = false;
                AudioManager.Instance?.PauseBgm(false);
            }
            if (this == null) return;

            GameEvents.RaiseRewardedFinished(earned ? RewardedResult.Rewarded : RewardedResult.ClosedEarly, Level);
            if (!earned) { done?.Invoke(false); return; }

            ContinuesUsed++;
            var livesAfter = gameController.Continue(_ads.ContinueLives);
            GameEvents.RaiseContinueGranted(gameController.Stats, livesAfter);
            done?.Invoke(true);
        }

        private async void ShowInterstitialThen(string trigger, InterstitialVerdict verdict, Action then)
        {
            var level = Level;
            if (verdict == InterstitialVerdict.SkippedFreeLevel)
                GameEvents.RaiseInterstitialDecided(trigger, ResultSkippedFreeLevel, level);

            var interstitial = App.Interstitial;
            if (verdict != InterstitialVerdict.Due || _showing || interstitial == null || !interstitial.IsReady)
            {
                if (verdict == InterstitialVerdict.Due)
                    GameEvents.RaiseInterstitialDecided(trigger, ResultNotReady, level);
                then?.Invoke();
                return;
            }

            _showing = true;
            AudioManager.Instance?.PauseBgm(true);
            var shown = false;
            try { shown = await interstitial.ShowAsync(); }
            catch (Exception e) { Debug.LogWarning("[Ads] 전면 광고 예외: " + e.Message); }
            finally
            {
                _showing = false;
                AudioManager.Instance?.PauseBgm(false);
            }
            if (this == null) return;

            GameEvents.RaiseInterstitialDecided(trigger, shown ? ResultShown : ResultNotReady, level);
            then?.Invoke();
        }
    }
}
