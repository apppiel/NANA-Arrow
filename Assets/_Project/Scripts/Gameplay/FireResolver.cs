namespace NanaArrow.Gameplay
{
    /// <summary>
    /// GAME_RULES §2-1~3: Head 앞 칸부터 보드 가장자리까지 검사해 Exit / Block 을 판정한다.
    /// 판정만 하고 보드는 바꾸지 않는다 (Exit 시 제거는 호출자 책임).
    /// </summary>
    public static class FireResolver
    {
        public static FireResult Resolve(Board board, Arrow arrow)
        {
            var step = arrow.Direction.ToOffset();
            var cell = arrow.Head + step;
            var freeCells = 0;

            while (board.IsInside(cell))
            {
                var other = board.GetArrowAt(cell);
                if (other != null)
                    return FireResult.Blocked(freeCells, other);

                freeCells++;
                cell += step;
            }

            return FireResult.Exit(freeCells);
        }
    }
}
