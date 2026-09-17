using System;
using UnityEngine;

namespace NanaArrow.Gameplay
{
    public static class DirectionExtensions
    {
        /// <summary>한 칸 이동 오프셋.</summary>
        public static Vector2Int ToOffset(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Vector2Int.up;
                case Direction.Down: return Vector2Int.down;
                case Direction.Left: return Vector2Int.left;
                case Direction.Right: return Vector2Int.right;
                default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>인접 셀 사이의 한 칸 오프셋 → 방향. 상하좌우 한 칸이 아니면 false.</summary>
        public static bool TryFromOffset(Vector2Int offset, out Direction direction)
        {
            if (offset == Vector2Int.up) { direction = Direction.Up; return true; }
            if (offset == Vector2Int.down) { direction = Direction.Down; return true; }
            if (offset == Vector2Int.left) { direction = Direction.Left; return true; }
            if (offset == Vector2Int.right) { direction = Direction.Right; return true; }
            direction = default;
            return false;
        }
    }
}
