using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>레인 가이드 선 하나 (GAME_RULES v0.7.3 §9). 표 격자가 아니라 Arrow 머리가 있는 행·열에만 긋는다.</summary>
    public readonly struct LaneGuideLine
    {
        /// <summary>true = 가로선(행 <see cref="Index"/> 를 따라), false = 세로선(열 <see cref="Index"/> 를 따라).</summary>
        public readonly bool Horizontal;
        /// <summary>가로선이면 셀 y, 세로선이면 셀 x.</summary>
        public readonly int Index;

        public LaneGuideLine(bool horizontal, int index)
        {
            Horizontal = horizontal;
            Index = index;
        }
    }

    /// <summary>
    /// 레인 가이드 계산 (순수 C#, GAME_RULES v0.7.3 §9).
    /// 가로 Arrow(Left/Right)는 머리가 있는 <b>행</b>에, 세로 Arrow(Up/Down)는 머리가 있는 <b>열</b>에 선을 긋는다.
    /// Arrow 가 없는 행·열에는 선이 없고, 같은 행·열에 여러 Arrow 가 있어도 선은 하나다.
    /// </summary>
    public static class LaneGuides
    {
        /// <summary>가로 방향(Left/Right)인가.</summary>
        public static bool IsHorizontal(Direction direction) =>
            direction == Direction.Left || direction == Direction.Right;

        /// <summary>Arrow 하나가 만드는 선.</summary>
        public static LaneGuideLine LineFor(Arrow arrow) =>
            IsHorizontal(arrow.Direction)
                ? new LaneGuideLine(true, arrow.Head.y)
                : new LaneGuideLine(false, arrow.Head.x);

        /// <summary>남아 있는 Arrow 들이 만드는 선 목록 (중복 제거).</summary>
        public static List<LaneGuideLine> For(IEnumerable<Arrow> arrows)
        {
            var lines = new List<LaneGuideLine>();
            var rows = new HashSet<int>();
            var columns = new HashSet<int>();
            foreach (var arrow in arrows)
            {
                var line = LineFor(arrow);
                var seen = line.Horizontal ? rows : columns;
                if (seen.Add(line.Index))
                    lines.Add(line);
            }
            return lines;
        }

        /// <summary>보드 사각형 안에서 Arrow 가 덮지 않은 빈 칸 (빈 칸 점용, GAME_RULES v0.7.3 §9).</summary>
        public static List<Vector2Int> EmptyCells(int width, int height, IEnumerable<Arrow> arrows)
        {
            var occupied = new HashSet<Vector2Int>();
            foreach (var arrow in arrows)
                for (var i = 0; i < arrow.Cells.Count; i++)
                    occupied.Add(arrow.Cells[i]);

            var empty = new List<Vector2Int>();
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (!occupied.Contains(cell)) empty.Add(cell);
                }
            return empty;
        }
    }
}
