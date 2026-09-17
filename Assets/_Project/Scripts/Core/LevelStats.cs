using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>레벨 한 판의 애널리틱스 지표 (ANALYTICS §3-1). GameController 가 세션 이벤트로 채운다.</summary>
    public sealed class LevelStats
    {
        private readonly float _startedAt;

        public int Level { get; }
        public int BoardWidth { get; }
        public int BoardHeight { get; }
        public int Arrows { get; }
        public bool IsReplay { get; }
        public int Attempt { get; }
        public LevelStartReason StartReason { get; }

        /// <summary>발사 시도 수 (얼음 깨기 포함).</summary>
        public int Taps { get; private set; }
        /// <summary>Block 횟수 (Marked 재탭 포함).</summary>
        public int Blocks { get; private set; }
        public int LanePreviews { get; private set; }
        public int ContinuesUsed { get; private set; }
        public float DurationSec => Time.realtimeSinceStartup - _startedAt;

        public LevelStats(int level, int boardWidth, int boardHeight, int arrows, bool isReplay, int attempt, LevelStartReason startReason)
        {
            Level = level;
            BoardWidth = boardWidth;
            BoardHeight = boardHeight;
            Arrows = arrows;
            IsReplay = isReplay;
            Attempt = attempt;
            StartReason = startReason;
            _startedAt = Time.realtimeSinceStartup;
        }

        public void CountTap() => Taps++;
        public void CountBlock() => Blocks++;
        public void CountLanePreview() => LanePreviews++;
        public void CountContinue() => ContinuesUsed++;
    }
}
