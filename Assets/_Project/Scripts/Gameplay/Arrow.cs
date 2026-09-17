using System;
using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay
{
    /// <summary>보드 위 화살표 블록. 불변. 셀이 직선·연속인지는 LevelValidator 가 검사한다.</summary>
    public sealed class Arrow
    {
        private readonly Vector2Int[] _cells;

        public string Id { get; }
        public ArrowType Type { get; }
        public Direction Direction { get; }
        public IReadOnlyList<Vector2Int> Cells => _cells;

        /// <summary>Direction 방향의 맨 앞 칸 (GAME_RULES §1 Head).</summary>
        public Vector2Int Head { get; }

        public Arrow(string id, ArrowType type, Direction direction, params Vector2Int[] cells)
        {
            if (cells == null || cells.Length == 0)
                throw new ArgumentException("Arrow needs at least one cell.", nameof(cells));

            Id = id;
            Type = type;
            Direction = direction;
            _cells = (Vector2Int[])cells.Clone();
            Head = FindHead(_cells, direction.ToOffset());
        }

        private static Vector2Int FindHead(Vector2Int[] cells, Vector2Int step)
        {
            var head = cells[0];
            var best = Dot(head, step);
            for (var i = 1; i < cells.Length; i++)
            {
                var d = Dot(cells[i], step);
                if (d <= best) continue;
                best = d;
                head = cells[i];
            }
            return head;
        }

        private static int Dot(Vector2Int a, Vector2Int b) => a.x * b.x + a.y * b.y;
    }
}
