using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>
    /// 보드 배치 계산 (순수 C#). 셀 (0,0) 은 좌하단, 보드는 center 에 중앙 정렬.
    /// 셀 크기는 GAME_RULES v0.7 §9: 보드 크기와 무관하게 일정하되 넓은 보드는 화면 폭 안에 맞춘다 (<see cref="CellSizeFor"/>).
    /// </summary>
    public sealed class BoardLayout
    {
        private readonly Vector2 _origin;

        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public float CellGap { get; }
        /// <summary>인접 셀 중심 간 거리.</summary>
        public float Pitch { get; }
        public Vector2 Center { get; }
        public Vector2 BoardSize { get; }

        public BoardLayout(int width, int height, float cellSize, float cellGap, Vector2 center)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            CellGap = cellGap;
            Pitch = cellSize + cellGap;
            Center = center;
            BoardSize = new Vector2(width * cellSize + (width - 1) * cellGap, height * cellSize + (height - 1) * cellGap);
            _origin = center - BoardSize * 0.5f + new Vector2(cellSize, cellSize) * 0.5f;
        }

        /// <summary>격자가 덮는 영역의 좌하단 (셀 피치 기준 — 가장자리 셀의 바깥 경계). <see cref="GridMax"/> 와 쌍.</summary>
        public Vector2 GridMin => Center - new Vector2(Width, Height) * Pitch * 0.5f;

        /// <summary>격자가 덮는 영역의 우상단.</summary>
        public Vector2 GridMax => Center + new Vector2(Width, Height) * Pitch * 0.5f;

        /// <summary>cell = min(화면폭 × cellWidthFraction, 화면폭 × maxAreaFraction ÷ 가로칸수).</summary>
        public static float CellSizeFor(float screenWidth, int boardWidth, float cellWidthFraction, float maxAreaFraction) =>
            screenWidth * Mathf.Min(cellWidthFraction, maxAreaFraction / boardWidth);

        /// <summary>셀 중심의 월드 좌표.</summary>
        public Vector2 CellToWorld(Vector2Int cell) => _origin + new Vector2(cell.x, cell.y) * Pitch;

        /// <summary>가장 가까운 셀. 보드 밖(가장자리 간격 절반 이상 벗어남)이면 false.</summary>
        public bool TryWorldToCell(Vector2 world, out Vector2Int cell)
        {
            var local = (world - _origin) / Pitch;
            cell = new Vector2Int(Mathf.RoundToInt(local.x), Mathf.RoundToInt(local.y));
            return cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;
        }
    }
}
