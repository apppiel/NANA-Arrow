using System.Collections.Generic;

namespace NanaArrow.Gameplay
{
    /// <summary>
    /// 탭 한 번을 논리 보드에서 즉시 확정한다 (GAME_RULES §2, §3, §9). 연출은 TapResult 를 보고 뒤따른다.
    /// Frozen 의 남은 탭 수를 들고 있으므로 레벨마다 새로 만든다.
    /// </summary>
    public sealed class TapHandler
    {
        private readonly Board _board;
        private readonly LivesTracker _lives;
        private readonly Dictionary<Arrow, int> _remainingHits = new Dictionary<Arrow, int>();

        public TapHandler(Board board, LivesTracker lives)
        {
            _board = board;
            _lives = lives;
        }

        public TapResult Tap(Arrow arrow)
        {
            if (_lives.IsOutOfLives || _board.GetArrow(arrow.Id) != arrow)
                return TapResult.Ignored();

            if (_board.IsLocked(arrow))
                return TapResult.Locked();

            // 마지막 탭(DefaultHits) 전까지는 얼음 깨기: 경로 무관, 목숨 차감 없음
            var remaining = GetRemainingHits(arrow);
            if (remaining > Arrow.DefaultHits)
            {
                remaining--;
                _remainingHits[arrow] = remaining;
                return TapResult.IceBroken(remaining);
            }

            var fire = FireResolver.Resolve(_board, arrow);
            if (fire.IsBlocked)
                return TapResult.Blocked(fire, _lives.OnBlocked(arrow));

            _board.Remove(arrow);
            _remainingHits.Remove(arrow);
            _lives.OnExit(arrow);
            return TapResult.Exit(fire);
        }

        private int GetRemainingHits(Arrow arrow) =>
            _remainingHits.TryGetValue(arrow, out var remaining) ? remaining : arrow.Hits;
    }
}
