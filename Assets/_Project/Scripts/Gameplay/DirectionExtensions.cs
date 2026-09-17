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
    }
}
