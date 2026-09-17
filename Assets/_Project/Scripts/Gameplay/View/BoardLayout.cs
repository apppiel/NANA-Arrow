using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>
    /// 보드를 주어진 영역 중앙에 맞추는 배치 계산 (순수 C#). 셀 (0,0) 은 좌하단.
    /// cellSize 는 최대 크기: 영역보다 크면 축소, 작아도 확대하지 않는다.
    /// </summary>
    public sealed class BoardLayout
    {
        private const float MaxScale = 1f;

        private readonly Vector2 _origin;

        public int Width { get; }
        public int Height { get; }
        public float Scale { get; }
        public float CellSize { get; }
        public float CellGap { get; }
        /// <summary>인접 셀 중심 간 거리.</summary>
        public float Pitch { get; }
        public Vector2 Center { get; }
        public Vector2 BoardSize { get; }

        public BoardLayout(int width, int height, float cellSize, float cellGap, Vector2 availableSize, Vector2 center)
        {
            Width = width;
            Height = height;
            Center = center;

            var nominal = new Vector2(
                width * cellSize + (width - 1) * cellGap,
                height * cellSize + (height - 1) * cellGap);
            Scale = Mathf.Min(MaxScale, availableSize.x / nominal.x, availableSize.y / nominal.y);

            CellSize = cellSize * Scale;
            CellGap = cellGap * Scale;
            Pitch = CellSize + CellGap;
            BoardSize = nominal * Scale;
            _origin = center - BoardSize * 0.5f + new Vector2(CellSize, CellSize) * 0.5f;
        }

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
