using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay
{
    /// <summary>
    /// GAME_RULES §2-1~3 + v0.6 §0: 머리 앞 칸부터 보드 가장자리까지 직선 레인을 검사해 Exit / Block 을 판정한다.
    /// 몸통은 자기 경로를 따라오므로 자기 자신의 셀은 레인을 막지 않는다. 판정만 하고 보드는 바꾸지 않는다.
    /// </summary>
    public static class FireResolver
    {
        public static FireResult Resolve(Board board, Arrow arrow)
        {
            var step = arrow.Direction.ToOffset();
            var cell = arrow.Head + step;
            var lane = new List<Vector2Int>();

            while (board.IsInside(cell))
            {
                var other = board.GetArrowAt(cell);
                if (other != null && other != arrow)
                    return FireResult.Blocked(lane, other);

                lane.Add(cell);
                cell += step;
            }

            return FireResult.Exit(lane);
        }
    }
}
