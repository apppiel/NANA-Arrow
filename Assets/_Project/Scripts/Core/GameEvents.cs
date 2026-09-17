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

        public static void RaiseLevelStarted(LevelStats stats) => LevelStarted?.Invoke(stats);
        public static void RaiseLevelCleared(LevelStats stats, int livesLeft) => LevelCleared?.Invoke(stats, livesLeft);
        public static void RaiseLevelFailed(LevelStats stats, int arrowsLeft) => LevelFailed?.Invoke(stats, arrowsLeft);
        public static void RaiseRetryPressed(LevelStats stats, LevelStartReason source, int arrowsLeft) => RetryPressed?.Invoke(stats, source, arrowsLeft);
        public static void RaiseLevelQuit(LevelStats stats, bool appPause, int arrowsLeft, int livesLeft) => LevelQuit?.Invoke(stats, appPause, arrowsLeft, livesLeft);
    }
}
