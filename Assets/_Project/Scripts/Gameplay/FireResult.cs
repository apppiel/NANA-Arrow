using System;
using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay
{
    /// <summary>FireResolver 판정 결과.</summary>
    public readonly struct FireResult
    {
        /// <summary>머리 앞의 빈 칸 (레인). Exit 면 가장자리까지, Blocked 면 막은 Arrow 직전까지. 순서는 머리에서 멀어지는 방향.</summary>
        public IReadOnlyList<Vector2Int> Lane { get; }

        /// <summary>Lane 의 칸 수 (튕김·Fire 연출 거리).</summary>
        public int FreeCells => Lane.Count;

        /// <summary>경로를 막은 Arrow. Exit 면 null.</summary>
        public Arrow BlockedBy { get; }

        public bool IsExit => BlockedBy == null;
        public bool IsBlocked => BlockedBy != null;

        private FireResult(IReadOnlyList<Vector2Int> lane, Arrow blockedBy)
        {
            Lane = lane ?? Array.Empty<Vector2Int>();
            BlockedBy = blockedBy;
        }

        public static FireResult Exit(IReadOnlyList<Vector2Int> lane) => new FireResult(lane, null);
        public static FireResult Blocked(IReadOnlyList<Vector2Int> lane, Arrow blockedBy) => new FireResult(lane, blockedBy);
    }
}
