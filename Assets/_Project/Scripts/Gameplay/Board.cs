using System;
using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay
{
    /// <summary>N×M 논리 보드. 셀 하나는 비어 있거나 Arrow 하나의 일부 (GAME_RULES §1).</summary>
    public sealed class Board
    {
        private readonly Dictionary<Vector2Int, Arrow> _cellToArrow = new Dictionary<Vector2Int, Arrow>();
        private readonly Dictionary<string, Arrow> _arrowsById = new Dictionary<string, Arrow>();

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyCollection<Arrow> Arrows => _arrowsById.Values;

        /// <summary>비직사각 보드 마스크 (W-027). null 이면 전체 사각형.</summary>
        public BoardMask Mask { get; }

        /// <summary>모든 Arrow 가 Exit 됨 = 레벨 클리어 (GAME_RULES §2-4).</summary>
        public bool IsCleared => _arrowsById.Count == 0;

        public Board(int width, int height, BoardMask mask = null)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), $"Board size must be positive: {width}x{height}");
            if (mask != null && (mask.Width != width || mask.Height != height))
                throw new ArgumentException($"Mask {mask.Width}x{mask.Height} does not match board {width}x{height}.", nameof(mask));
            Width = width;
            Height = height;
            Mask = mask;
        }

        /// <summary>사각형 경계 안인가. <b>마스크와 무관하다</b> — 레인 판정·Fire 는 사각형 기준 (W-027).</summary>
        public bool IsInside(Vector2Int cell) =>
            cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;

        /// <summary>실제로 쓰는 칸인가 (마스크가 없으면 사각형 안이면 true). 표시·검증용.</summary>
        public bool IsInMask(Vector2Int cell) =>
            Mask == null ? IsInside(cell) : Mask.Contains(cell);

        /// <summary>실제로 쓰는 칸 수 (마스크 없으면 Width × Height).</summary>
        public int UsableCellCount => Mask?.IncludedCount ?? Width * Height;

        /// <returns>해당 칸을 차지한 Arrow, 비어 있으면 null.</returns>
        public Arrow GetArrowAt(Vector2Int cell) =>
            _cellToArrow.TryGetValue(cell, out var arrow) ? arrow : null;

        /// <returns>id 로 찾은 Arrow, 없으면 null.</returns>
        public Arrow GetArrow(string id) =>
            _arrowsById.TryGetValue(id, out var arrow) ? arrow : null;

        /// <summary>GAME_RULES §3 Locked: 같은 keyGroup 의 Key 가 하나라도 보드에 남아 있으면 잠김.</summary>
        public bool IsLocked(Arrow arrow)
        {
            if (arrow.Type != ArrowType.Locked)
                return false;

            foreach (var other in _arrowsById.Values)
                if (other.Type == ArrowType.Key && other.KeyGroup == arrow.KeyGroup)
                    return true;
            return false;
        }

        public void Place(Arrow arrow)
        {
            if (_arrowsById.ContainsKey(arrow.Id))
                throw new InvalidOperationException($"Duplicate arrow id '{arrow.Id}'.");

            foreach (var cell in arrow.Cells)
            {
                if (!IsInside(cell))
                    throw new ArgumentOutOfRangeException(nameof(arrow), $"Arrow '{arrow.Id}' cell {cell} is outside the {Width}x{Height} board.");
                if (_cellToArrow.TryGetValue(cell, out var other))
                    throw new InvalidOperationException($"Arrow '{arrow.Id}' overlaps '{other.Id}' at {cell}.");
            }

            foreach (var cell in arrow.Cells)
                _cellToArrow[cell] = arrow;
            _arrowsById[arrow.Id] = arrow;
        }

        /// <returns>보드에 있던 Arrow 를 제거했으면 true.</returns>
        public bool Remove(Arrow arrow)
        {
            if (!_arrowsById.TryGetValue(arrow.Id, out var placed) || placed != arrow)
                return false;

            _arrowsById.Remove(arrow.Id);
            foreach (var cell in placed.Cells)
                _cellToArrow.Remove(cell);
            return true;
        }
    }
}
