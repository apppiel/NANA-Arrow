using System;
using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay
{
    /// <summary>
    /// 보드 위 화살표 = 순서 있는 셀 경로 (GAME_RULES v0.6 §0). cells[0] 이 꼬리, 마지막이 머리. 불변.
    /// 경로 전체의 인접·자기 겹침 검사는 LevelValidator 규칙 2 몫이고, 여기서는 Direction 을 정하는 마지막 두 칸만 본다.
    /// </summary>
    public sealed class Arrow
    {
        /// <summary>Frozen 이 아닌 Arrow 는 탭 1회로 Fire (GAME_RULES §3).</summary>
        public const int DefaultHits = 1;

        private readonly Vector2Int[] _cells;

        public string Id { get; }
        public ArrowType Type { get; }

        /// <summary>머리의 진행 방향. 길이 2 이상이면 마지막 두 칸에서 계산, 길이 1 이면 생성자 인자.</summary>
        public Direction Direction { get; }

        /// <summary>꼬리 → 머리 순서의 경로.</summary>
        public IReadOnlyList<Vector2Int> Cells => _cells;
        public int Length => _cells.Length;
        public Vector2Int Tail => _cells[0];
        public Vector2Int Head => _cells[_cells.Length - 1];

        /// <summary>Exit 까지 필요한 총 탭 수 (마지막 Fire 포함). Frozen 만 2 이상 (LEVEL_FORMAT hits).</summary>
        public int Hits { get; }

        /// <summary>Locked/Key 의 그룹. 그 외 null (LEVEL_FORMAT keyGroup).</summary>
        public string KeyGroup { get; }

        public Arrow(string id, ArrowType type, Direction direction, params Vector2Int[] cells)
            : this(id, type, direction, cells, DefaultHits, null)
        {
        }

        /// <param name="direction">길이 1 이면 머리 방향. 길이 2 이상이면 경로의 마지막 두 칸과 일치해야 한다 (불일치 시 ArgumentException).</param>
        public Arrow(string id, ArrowType type, Direction direction, Vector2Int[] cells, int hits, string keyGroup)
        {
            if (cells == null || cells.Length == 0)
                throw new ArgumentException("Arrow needs at least one cell.", nameof(cells));

            if (cells.Length >= 2)
            {
                var lastStep = cells[cells.Length - 1] - cells[cells.Length - 2];
                if (!DirectionExtensions.TryFromOffset(lastStep, out var pathDirection))
                    throw new ArgumentException($"Arrow '{id}': last two cells {cells[cells.Length - 2]} -> {cells[cells.Length - 1]} are not adjacent.", nameof(cells));
                if (pathDirection != direction)
                    throw new ArgumentException($"Arrow '{id}': dir {direction} does not match the path's last step ({pathDirection}).", nameof(direction));
            }

            Id = id;
            Type = type;
            Direction = direction;
            Hits = hits;
            KeyGroup = keyGroup;
            _cells = (Vector2Int[])cells.Clone();
        }
    }
}
