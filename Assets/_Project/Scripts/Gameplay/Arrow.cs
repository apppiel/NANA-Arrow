using System;
using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay
{
    /// <summary>보드 위 화살표 블록. 불변. 셀이 직선·연속인지는 LevelValidator 가 검사한다.</summary>
    public sealed class Arrow
    {
        /// <summary>Frozen 이 아닌 Arrow 는 탭 1회로 Fire (GAME_RULES §3).</summary>
        public const int DefaultHits = 1;

        private readonly Vector2Int[] _cells;

        public string Id { get; }
        public ArrowType Type { get; }
        public Direction Direction { get; }
        public IReadOnlyList<Vector2Int> Cells => _cells;

        /// <summary>Exit 까지 필요한 총 탭 수 (마지막 Fire 포함). Frozen 만 2 이상 (LEVEL_FORMAT hits).</summary>
        public int Hits { get; }

        /// <summary>Locked/Key 의 그룹. 그 외 null (LEVEL_FORMAT keyGroup).</summary>
        public string KeyGroup { get; }

        /// <summary>Direction 방향의 맨 앞 칸 (GAME_RULES §1 Head).</summary>
        public Vector2Int Head { get; }

        public Arrow(string id, ArrowType type, Direction direction, params Vector2Int[] cells)
            : this(id, type, direction, cells, DefaultHits, null)
        {
        }

        public Arrow(string id, ArrowType type, Direction direction, Vector2Int[] cells, int hits, string keyGroup)
        {
            if (cells == null || cells.Length == 0)
                throw new ArgumentException("Arrow needs at least one cell.", nameof(cells));

            Id = id;
            Type = type;
            Direction = direction;
            Hits = hits;
            KeyGroup = keyGroup;
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
