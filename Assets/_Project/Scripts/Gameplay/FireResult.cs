namespace NanaArrow.Gameplay
{
    /// <summary>FireResolver 판정 결과.</summary>
    public readonly struct FireResult
    {
        /// <summary>Head 앞의 빈 칸 수. Exit 면 가장자리까지, Blocked 면 막은 Arrow 직전까지 (튕김 연출 거리).</summary>
        public int FreeCells { get; }

        /// <summary>경로를 막은 Arrow. Exit 면 null.</summary>
        public Arrow BlockedBy { get; }

        public bool IsExit => BlockedBy == null;
        public bool IsBlocked => BlockedBy != null;

        private FireResult(int freeCells, Arrow blockedBy)
        {
            FreeCells = freeCells;
            BlockedBy = blockedBy;
        }

        public static FireResult Exit(int freeCells) => new FireResult(freeCells, null);
        public static FireResult Blocked(int freeCells, Arrow blockedBy) => new FireResult(freeCells, blockedBy);
    }
}
