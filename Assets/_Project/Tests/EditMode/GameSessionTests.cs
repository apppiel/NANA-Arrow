using System.Collections.Generic;
using NanaArrow.Core;
using NanaArrow.Data;
using NanaArrow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class GameSessionTests
    {
        private GameConfig _config;
        private ArrowTypeConfig _arrowTypes;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<GameConfig>();
            _arrowTypes = ScriptableObject.CreateInstance<ArrowTypeConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_arrowTypes);
        }

        private static ArrowData Basic(string id, Direction dir, int x, int y) =>
            new ArrowData { Id = id, Type = ArrowType.Basic, Direction = dir, Cells = new[] { new[] { x, y } } };

        private static LevelData Level(int? lives, params ArrowData[] arrows) =>
            new LevelData { Version = 1, Id = 1, Width = 5, Height = 5, Lives = lives, Arrows = arrows };

        /// <summary>a 는 b 에 막힘, c 는 자유.</summary>
        private static LevelData BlockedAndFree(int? lives) => Level(lives,
            Basic("a", Direction.Up, 2, 1),
            Basic("b", Direction.Left, 2, 3),
            Basic("c", Direction.Down, 4, 0));

        private GameSession NewSession(LevelData level) => new GameSession(level, _config, _arrowTypes);

        [Test]
        public void Constructor_UsesLevelLives_WhenPresent()
        {
            var session = NewSession(Level(lives: 5, Basic("a", Direction.Up, 0, 0)));

            Assert.AreEqual(5, session.Lives.MaxLives);
        }

        [Test]
        public void Constructor_FallsBackToConfigMaxLives()
        {
            var session = NewSession(Level(lives: null, Basic("a", Direction.Up, 0, 0)));

            Assert.AreEqual(_config.MaxLives, session.Lives.MaxLives);
        }

        [Test]
        public void Tap_RaisesTapped_WithArrowAndResult()
        {
            var session = NewSession(BlockedAndFree(3));
            var a = session.Board.GetArrow("a");
            Arrow tappedArrow = null;
            var tappedResult = default(TapResult);
            session.Tapped += (arrow, result) => { tappedArrow = arrow; tappedResult = result; };

            session.Tap(a);

            Assert.AreSame(a, tappedArrow);
            Assert.AreEqual(TapOutcome.Blocked, tappedResult.Outcome);
        }

        [Test]
        public void TapAt_EmptyCell_IsIgnored_AndRaisesNothing()
        {
            var session = NewSession(BlockedAndFree(3));
            var tapped = 0;
            session.Tapped += (_, __) => tapped++;

            var result = session.TapAt(new Vector2Int(0, 4));

            Assert.AreEqual(TapOutcome.Ignored, result.Outcome);
            Assert.AreEqual(0, tapped);
        }

        [Test]
        public void TapAt_ArrowCell_TapsThatArrow()
        {
            var session = NewSession(BlockedAndFree(3));

            var result = session.TapAt(new Vector2Int(4, 0));

            Assert.AreEqual(TapOutcome.Exit, result.Outcome);
            Assert.IsNull(session.Board.GetArrow("c"));
        }

        [Test]
        public void Cleared_RaisedOnce_WhenLastArrowExits()
        {
            var session = NewSession(Level(3, Basic("a", Direction.Up, 0, 0), Basic("b", Direction.Up, 1, 0)));
            var cleared = 0;
            session.Cleared += () => cleared++;

            session.Tap(session.Board.GetArrow("a"));
            Assert.AreEqual(0, cleared);
            Assert.IsFalse(session.IsCleared);

            session.Tap(session.Board.GetArrow("b"));
            Assert.AreEqual(1, cleared);
            Assert.IsTrue(session.IsCleared);
        }

        [Test]
        public void Failed_RaisedOnce_WhenLivesReachZero_ThenTapsAreIgnored()
        {
            var session = NewSession(BlockedAndFree(1));
            var a = session.Board.GetArrow("a");
            var c = session.Board.GetArrow("c");
            var failed = 0;
            session.Failed += () => failed++;

            session.Tap(a);
            var afterFail = session.Tap(c);

            Assert.AreEqual(1, failed);
            Assert.IsTrue(session.IsFailed);
            Assert.AreEqual(TapOutcome.Ignored, afterFail.Outcome);
            Assert.AreSame(c, session.Board.GetArrow("c"));
        }

        [Test]
        public void Failed_NotRaised_ForMarkedRetap()
        {
            var session = NewSession(BlockedAndFree(2));
            var a = session.Board.GetArrow("a");
            var failed = 0;
            session.Failed += () => failed++;

            session.Tap(a);
            session.Tap(a);

            Assert.AreEqual(0, failed);
            Assert.AreEqual(1, session.Lives.Lives);
        }

        [Test]
        public void Failed_RaisedAgain_AfterContinueAndAnotherLoss()
        {
            var session = NewSession(BlockedAndFree(1));
            var a = session.Board.GetArrow("a");
            var failed = 0;
            session.Failed += () => failed++;
            session.Tap(a);
            session.Lives.AddLives(1);
            session.Tap(session.Board.GetArrow("c"));

            session.Tap(a);

            Assert.AreEqual(2, failed);
        }

        [Test]
        public void FormatExample_PlayedInSolutionOrder_Clears()
        {
            var level = TestLevels.FormatExample();
            var session = NewSession(level);
            var outcomes = new List<TapOutcome>();
            var cleared = 0;
            var failed = 0;
            session.Tapped += (_, result) => outcomes.Add(result.Outcome);
            session.Cleared += () => cleared++;
            session.Failed += () => failed++;

            foreach (var id in level.Solution)
            {
                var arrow = session.Board.GetArrow(id);
                for (var tap = 0; tap < arrow.Hits; tap++)
                    session.Tap(arrow);
            }

            CollectionAssert.AreEqual(
                new[] { TapOutcome.Exit, TapOutcome.Exit, TapOutcome.Exit, TapOutcome.Exit, TapOutcome.IceBroken, TapOutcome.Exit },
                outcomes);
            Assert.AreEqual((int)level.Meta["minTaps"], outcomes.Count);
            Assert.AreEqual(1, cleared);
            Assert.AreEqual(0, failed);
            Assert.AreEqual(session.Lives.MaxLives, session.Lives.Lives);
        }

        [Test]
        public void FormatExample_WrongOrder_IsBlockedOrLocked_WithoutClearing()
        {
            var session = NewSession(TestLevels.FormatExample());

            var lockedTap = session.Tap(session.Board.GetArrow("a4"));
            var blockedTap = session.Tap(session.Board.GetArrow("a2"));

            Assert.AreEqual(TapOutcome.Locked, lockedTap.Outcome);
            Assert.AreEqual(TapOutcome.Blocked, blockedTap.Outcome);
            Assert.AreEqual(session.Lives.MaxLives - 1, session.Lives.Lives);
            Assert.IsFalse(session.IsCleared);
        }
    }
}
