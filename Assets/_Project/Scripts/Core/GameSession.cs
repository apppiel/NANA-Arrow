using System;
using System.Collections.Generic;
using NanaArrow.Data;
using NanaArrow.Gameplay;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>
    /// 레벨 하나의 플레이 상태 (순수 C#). LevelData → Board/LivesTracker/TapHandler 를 묶고 클리어·실패 이벤트를 낸다.
    /// 광고·저장·씬 전환은 모른다. 다시하기는 새 GameSession 을 만든다.
    /// </summary>
    public sealed class GameSession
    {
        private readonly TapHandler _tapHandler;

        public LevelData Level { get; }
        public Board Board { get; }
        public LivesTracker Lives { get; }
        public bool IsCleared => Board.IsCleared;
        public bool IsFailed => Lives.IsOutOfLives;

        /// <summary>Ignored 가 아닌 탭마다. 연출은 이 이벤트만 본다.</summary>
        public event Action<Arrow, TapResult> Tapped;
        /// <summary>마지막 Arrow 가 Exit 된 직후 한 번.</summary>
        public event Action Cleared;
        /// <summary>목숨이 0 이 된 탭 직후 한 번. 이어하기(Lives.AddLives) 후 다시 0 이 되면 또 한 번.</summary>
        public event Action Failed;

        public GameSession(LevelData level, GameConfig config, ArrowTypeConfig arrowTypes)
        {
            Level = level;
            Board = LevelLoader.CreateBoard(level, arrowTypes.FrozenDefaultHits);
            Lives = new LivesTracker(level.Lives ?? config.MaxLives, config.MarkedResetOnExit);
            _tapHandler = new TapHandler(Board, Lives);
        }

        public TapResult Tap(Arrow arrow)
        {
            var result = _tapHandler.Tap(arrow);
            if (result.Outcome == TapOutcome.Ignored)
                return result;

            Tapped?.Invoke(arrow, result);
            if (Board.IsCleared)
                Cleared?.Invoke();
            else if (result.LifeLost && Lives.IsOutOfLives)
                Failed?.Invoke();
            return result;
        }

        /// <summary>레인 미리보기 (길게 누르기, GAME_RULES §2-7). 판정만 하고 상태는 바꾸지 않는다.</summary>
        public FireResult Preview(Arrow arrow) => FireResolver.Resolve(Board, arrow);

        /// <summary>치트 "즉시 클리어": 남은 Arrow 를 전부 제거하고 Cleared 를 낸다 (Tapped 는 안 냄).</summary>
        public void ForceClear()
        {
            if (Board.IsCleared) return;
            foreach (var arrow in new List<Arrow>(Board.Arrows))
                Board.Remove(arrow);
            Cleared?.Invoke();
        }

        /// <summary>빈 칸이면 Ignored.</summary>
        public TapResult TapAt(Vector2Int cell)
        {
            var arrow = Board.GetArrowAt(cell);
            return arrow == null ? TapResult.Ignored() : Tap(arrow);
        }
    }
}
