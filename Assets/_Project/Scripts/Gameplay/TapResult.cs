using System;
using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay
{
    /// <summary>TapHandler.Tap 결과. 연출은 이 값만 보고 진행한다.</summary>
    public readonly struct TapResult
    {
        public TapOutcome Outcome { get; }

        /// <summary>Exit / Blocked: 머리 앞의 빈 칸 목록 (레인 번쩍·미리보기·연출 거리). 그 외 빈 목록.</summary>
        public IReadOnlyList<Vector2Int> Lane { get; }

        /// <summary>Lane 의 칸 수.</summary>
        public int FreeCells => Lane.Count;

        /// <summary>Blocked 일 때 막은 Arrow, 그 외 null.</summary>
        public Arrow BlockedBy { get; }

        /// <summary>Blocked 로 목숨이 깎였는지 (Marked 재탭이면 false).</summary>
        public bool LifeLost { get; }

        /// <summary>IceBroken 후 Exit 까지 남은 탭 수.</summary>
        public int RemainingHits { get; }

        private TapResult(TapOutcome outcome, IReadOnlyList<Vector2Int> lane, Arrow blockedBy, bool lifeLost, int remainingHits)
        {
            Outcome = outcome;
            Lane = lane ?? Array.Empty<Vector2Int>();
            BlockedBy = blockedBy;
            LifeLost = lifeLost;
            RemainingHits = remainingHits;
        }

        public static TapResult Ignored() => new TapResult(TapOutcome.Ignored, null, null, false, 0);
        public static TapResult Locked() => new TapResult(TapOutcome.Locked, null, null, false, 0);
        public static TapResult IceBroken(int remainingHits) => new TapResult(TapOutcome.IceBroken, null, null, false, remainingHits);
        public static TapResult Blocked(FireResult fire, bool lifeLost) => new TapResult(TapOutcome.Blocked, fire.Lane, fire.BlockedBy, lifeLost, 0);
        public static TapResult Exit(FireResult fire) => new TapResult(TapOutcome.Exit, fire.Lane, null, false, 0);
    }
}
