namespace NanaArrow.Gameplay
{
    /// <summary>TapHandler.Tap 결과. 연출은 이 값만 보고 진행한다.</summary>
    public readonly struct TapResult
    {
        public TapOutcome Outcome { get; }

        /// <summary>Exit: 가장자리까지 빈 칸 수 / Blocked: 막은 Arrow 직전까지 빈 칸 수 (튕김 거리).</summary>
        public int FreeCells { get; }

        /// <summary>Blocked 일 때 막은 Arrow, 그 외 null.</summary>
        public Arrow BlockedBy { get; }

        /// <summary>Blocked 로 목숨이 깎였는지 (Marked 재탭이면 false).</summary>
        public bool LifeLost { get; }

        /// <summary>IceBroken 후 Exit 까지 남은 탭 수.</summary>
        public int RemainingHits { get; }

        private TapResult(TapOutcome outcome, int freeCells, Arrow blockedBy, bool lifeLost, int remainingHits)
        {
            Outcome = outcome;
            FreeCells = freeCells;
            BlockedBy = blockedBy;
            LifeLost = lifeLost;
            RemainingHits = remainingHits;
        }

        public static TapResult Ignored() => new TapResult(TapOutcome.Ignored, 0, null, false, 0);
        public static TapResult Locked() => new TapResult(TapOutcome.Locked, 0, null, false, 0);
        public static TapResult IceBroken(int remainingHits) => new TapResult(TapOutcome.IceBroken, 0, null, false, remainingHits);
        public static TapResult Blocked(FireResult fire, bool lifeLost) => new TapResult(TapOutcome.Blocked, fire.FreeCells, fire.BlockedBy, lifeLost, 0);
        public static TapResult Exit(FireResult fire) => new TapResult(TapOutcome.Exit, fire.FreeCells, null, false, 0);
    }
}
