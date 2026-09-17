using System;
using System.Collections.Generic;

namespace NanaArrow.Gameplay
{
    /// <summary>GAME_RULES §2-6 목숨 + Marked 규칙. 레벨마다 새로 만든다 (레벨 간 공유 없음).</summary>
    public sealed class LivesTracker
    {
        private readonly HashSet<Arrow> _marked = new HashSet<Arrow>();
        private readonly bool _markedResetOnExit;

        public int MaxLives { get; }
        public int Lives { get; private set; }
        public bool IsOutOfLives => Lives <= 0;

        /// <summary>목숨 수가 바뀔 때 (새 값). 하트 UI·진동용.</summary>
        public event Action<int> LivesChanged;

        /// <param name="maxLives">GameConfig.maxLives</param>
        /// <param name="markedResetOnExit">GameConfig.markedResetOnExit</param>
        public LivesTracker(int maxLives, bool markedResetOnExit)
        {
            MaxLives = maxLives;
            Lives = maxLives;
            _markedResetOnExit = markedResetOnExit;
        }

        public bool IsMarked(Arrow arrow) => _marked.Contains(arrow);

        /// <summary>Block 된 Arrow 를 탭했을 때. 처음이면 목숨 -1 후 Marked, 이미 Marked 면 목숨 유지 (배려 규칙).</summary>
        /// <returns>목숨이 깎였으면 true.</returns>
        public bool OnBlocked(Arrow arrow)
        {
            if (!_marked.Add(arrow))
                return false;

            Lives = Math.Max(0, Lives - 1);
            LivesChanged?.Invoke(Lives);
            return true;
        }

        /// <summary>어떤 Arrow 든 Exit 됐을 때. 보드가 바뀌었으므로 markedResetOnExit 면 Marked 전부 해제.</summary>
        public void OnExit(Arrow exited)
        {
            if (_markedResetOnExit)
                _marked.Clear();
            else
                _marked.Remove(exited);
        }

        /// <summary>이어하기 (AdsConfig.continueLives). MaxLives 를 넘지 않는다.</summary>
        public void AddLives(int amount)
        {
            var before = Lives;
            Lives = Math.Min(MaxLives, Lives + amount);
            if (Lives != before) LivesChanged?.Invoke(Lives);
        }
    }
}
