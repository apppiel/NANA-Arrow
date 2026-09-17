using System;

namespace NanaArrow.Core
{
    /// <summary>
    /// 게임플레이 → 바깥(애널리틱스·광고·UI) 이벤트 허브. 게임플레이는 여기로 발행만 하고 누가 듣는지 모른다 (GAME_RULES §7 "광고 판단은 AdsManager 한 곳").
    /// 인자의 LevelStats 는 발행 시점의 스냅샷으로 읽을 것.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>보드 등장 직후 (다시하기 포함).</summary>
        public static event Action<LevelStats> LevelStarted;
        /// <summary>마지막 Exit 직후 (팝업 전). int = 남은 하트.</summary>
        public static event Action<LevelStats, int> LevelCleared;
        /// <summary>하트 0. int = 남은 Arrow 수.</summary>
        public static event Action<LevelStats, int> LevelFailed;
        /// <summary>HUD 다시하기 / 실패 팝업 다시하기. int = 남은 Arrow 수.</summary>
        public static event Action<LevelStats, LevelStartReason, int> RetryPressed;
        /// <summary>메인으로 나가기 / 앱 종료. bool = 앱 종료(pause) 인지, int = 남은 Arrow, int = 남은 하트.</summary>
        public static event Action<LevelStats, bool, int, int> LevelQuit;

        /// <summary>튜토리얼 항목 표시 (level, textKey, trigger 이름).</summary>
        public static event Action<int, string, string> TutorialStepShown;
        /// <summary>튜토리얼 항목 종료 (level, textKey, 경과 초).</summary>
        public static event Action<int, string, float> TutorialDone;
        /// <summary>사용자 첫 길게 누르기 (앱 설치 후 1회).</summary>
        public static event Action<int> LanePreviewFirst;
        /// <summary>레벨 선택 패널 열림 (최고 클리어 레벨).</summary>
        public static event Action<int> LevelSelectOpened;

        /// <summary>실패 팝업에 이어하기 버튼이 보일 때 (stats, 광고 준비 여부).</summary>
        public static event Action<LevelStats, bool> ContinueOffered;
        /// <summary>이어하기 버튼 탭.</summary>
        public static event Action<LevelStats> ContinueRequested;
        /// <summary>보상 지급 (stats, 회복 후 목숨).</summary>
        public static event Action<LevelStats, int> ContinueGranted;
        /// <summary>전면 광고 판정 결과 (trigger "clear"/"fail_retry", result "shown"/"not_ready"/"skipped_free_level", level).</summary>
        public static event Action<string, string, int> InterstitialDecided;
        /// <summary>보상형 광고 종료 (result, level).</summary>
        public static event Action<RewardedResult, int> RewardedFinished;
        /// <summary>응모 코드 발급 (서버 저장 성공 여부).</summary>
        public static event Action<bool> RewardCodeIssued;
        public static event Action RewardCodeCopied;
        public static event Action RewardLinkOpened;

        public static void RaiseLevelStarted(LevelStats stats) => LevelStarted?.Invoke(stats);
        public static void RaiseContinueOffered(LevelStats stats, bool adReady) => ContinueOffered?.Invoke(stats, adReady);
        public static void RaiseContinueRequested(LevelStats stats) => ContinueRequested?.Invoke(stats);
        public static void RaiseContinueGranted(LevelStats stats, int livesAfter) => ContinueGranted?.Invoke(stats, livesAfter);
        public static void RaiseInterstitialDecided(string trigger, string result, int level) => InterstitialDecided?.Invoke(trigger, result, level);
        public static void RaiseRewardedFinished(RewardedResult result, int level) => RewardedFinished?.Invoke(result, level);
        public static void RaiseRewardCodeIssued(bool synced) => RewardCodeIssued?.Invoke(synced);
        public static void RaiseRewardCodeCopied() => RewardCodeCopied?.Invoke();
        public static void RaiseRewardLinkOpened() => RewardLinkOpened?.Invoke();
        public static void RaiseTutorialStepShown(int level, string textKey, string trigger) => TutorialStepShown?.Invoke(level, textKey, trigger);
        public static void RaiseTutorialDone(int level, string textKey, float elapsedSec) => TutorialDone?.Invoke(level, textKey, elapsedSec);
        public static void RaiseLanePreviewFirst(int level) => LanePreviewFirst?.Invoke(level);
        public static void RaiseLevelSelectOpened(int highestLevel) => LevelSelectOpened?.Invoke(highestLevel);
        public static void RaiseLevelCleared(LevelStats stats, int livesLeft) => LevelCleared?.Invoke(stats, livesLeft);
        public static void RaiseLevelFailed(LevelStats stats, int arrowsLeft) => LevelFailed?.Invoke(stats, arrowsLeft);
        public static void RaiseRetryPressed(LevelStats stats, LevelStartReason source, int arrowsLeft) => RetryPressed?.Invoke(stats, source, arrowsLeft);
        public static void RaiseLevelQuit(LevelStats stats, bool appPause, int arrowsLeft, int livesLeft) => LevelQuit?.Invoke(stats, appPause, arrowsLeft, livesLeft);
    }
}
