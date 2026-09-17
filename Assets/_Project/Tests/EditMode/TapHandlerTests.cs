using NanaArrow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class TapHandlerTests
    {
        private const int MaxLives = 3;

        private Board _board;
        private LivesTracker _lives;
        private TapHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _board = new Board(5, 5);
            _lives = new LivesTracker(MaxLives, true);
            _handler = new TapHandler(_board, _lives);
        }

        private Arrow Place(string id, ArrowType type, Direction dir, int x, int y, int hits = Arrow.DefaultHits, string keyGroup = null)
        {
            var arrow = new Arrow(id, type, dir, new[] { new Vector2Int(x, y) }, hits, keyGroup);
            _board.Place(arrow);
            return arrow;
        }

        [Test]
        public void Tap_ClearPath_Exits_AndRemovesFromBoard()
        {
            var arrow = Place("a", ArrowType.Basic, Direction.Up, 2, 2);

            var result = _handler.Tap(arrow);

            Assert.AreEqual(TapOutcome.Exit, result.Outcome);
            Assert.AreEqual(2, result.FreeCells);
            Assert.IsNull(_board.GetArrow("a"));
            Assert.IsTrue(_board.IsCleared);
            Assert.AreEqual(MaxLives, _lives.Lives);
        }

        [Test]
        public void Tap_Blocked_LosesLife_AndMarks()
        {
            var arrow = Place("a", ArrowType.Basic, Direction.Up, 2, 1);
            var blocker = Place("b", ArrowType.Basic, Direction.Left, 2, 3);

            var result = _handler.Tap(arrow);

            Assert.AreEqual(TapOutcome.Blocked, result.Outcome);
            Assert.AreSame(blocker, result.BlockedBy);
            Assert.AreEqual(1, result.FreeCells);
            Assert.IsTrue(result.LifeLost);
            Assert.AreEqual(MaxLives - 1, _lives.Lives);
            Assert.IsTrue(_lives.IsMarked(arrow));
            Assert.AreSame(arrow, _board.GetArrow("a"));
        }

        [Test]
        public void Tap_BlockedAgainWhileMarked_NoLifeLost()
        {
            var arrow = Place("a", ArrowType.Basic, Direction.Up, 2, 1);
            Place("b", ArrowType.Basic, Direction.Left, 2, 3);
            _handler.Tap(arrow);

            var result = _handler.Tap(arrow);

            Assert.AreEqual(TapOutcome.Blocked, result.Outcome);
            Assert.IsFalse(result.LifeLost);
            Assert.AreEqual(MaxLives - 1, _lives.Lives);
        }

        [Test]
        public void Tap_Exit_ClearsMarksOfOtherArrows()
        {
            var marked = Place("a", ArrowType.Basic, Direction.Up, 2, 1);
            Place("b", ArrowType.Basic, Direction.Left, 2, 3);
            var free = Place("c", ArrowType.Basic, Direction.Down, 4, 0);
            _handler.Tap(marked);

            _handler.Tap(free);

            Assert.IsFalse(_lives.IsMarked(marked));
        }

        [Test]
        public void Tap_ArrowAlreadyExited_Ignored()
        {
            var arrow = Place("a", ArrowType.Basic, Direction.Up, 2, 2);
            _handler.Tap(arrow);

            var result = _handler.Tap(arrow);

            Assert.AreEqual(TapOutcome.Ignored, result.Outcome);
        }

        [Test]
        public void Tap_WhenOutOfLives_Ignored_AndBoardUnchanged()
        {
            _lives = new LivesTracker(1, true);
            _handler = new TapHandler(_board, _lives);
            var blocked = Place("a", ArrowType.Basic, Direction.Up, 2, 1);
            Place("b", ArrowType.Basic, Direction.Left, 2, 3);
            var free = Place("c", ArrowType.Basic, Direction.Down, 4, 0);
            _handler.Tap(blocked);
            Assert.IsTrue(_lives.IsOutOfLives);

            var result = _handler.Tap(free);

            Assert.AreEqual(TapOutcome.Ignored, result.Outcome);
            Assert.AreSame(free, _board.GetArrow("c"));
        }

        [Test]
        public void Tap_AfterContinue_WorksAgain()
        {
            _lives = new LivesTracker(1, true);
            _handler = new TapHandler(_board, _lives);
            var blocked = Place("a", ArrowType.Basic, Direction.Up, 2, 1);
            Place("b", ArrowType.Basic, Direction.Left, 2, 3);
            var free = Place("c", ArrowType.Basic, Direction.Down, 4, 0);
            _handler.Tap(blocked);
            _lives.AddLives(1);

            Assert.AreEqual(TapOutcome.Exit, _handler.Tap(free).Outcome);
        }

        [Test]
        public void Tap_Frozen_FirstTap_BreaksIce_IgnoringPathAndLives()
        {
            var frozen = Place("f", ArrowType.Frozen, Direction.Up, 2, 1, hits: 2);
            Place("b", ArrowType.Basic, Direction.Left, 2, 2);

            var result = _handler.Tap(frozen);

            Assert.AreEqual(TapOutcome.IceBroken, result.Outcome);
            Assert.AreEqual(1, result.RemainingHits);
            Assert.AreEqual(MaxLives, _lives.Lives);
            Assert.IsFalse(_lives.IsMarked(frozen));
            Assert.AreSame(frozen, _board.GetArrow("f"));
        }

        [Test]
        public void Tap_Frozen_LastTap_IsBlockJudged()
        {
            var frozen = Place("f", ArrowType.Frozen, Direction.Up, 2, 1, hits: 2);
            var blocker = Place("b", ArrowType.Basic, Direction.Left, 2, 2);
            _handler.Tap(frozen);

            var result = _handler.Tap(frozen);

            Assert.AreEqual(TapOutcome.Blocked, result.Outcome);
            Assert.AreSame(blocker, result.BlockedBy);
            Assert.IsTrue(result.LifeLost);
            Assert.AreEqual(MaxLives - 1, _lives.Lives);
        }

        [Test]
        public void Tap_Frozen_LastTap_ClearPath_Exits()
        {
            var frozen = Place("f", ArrowType.Frozen, Direction.Up, 2, 2, hits: 2);
            _handler.Tap(frozen);

            Assert.AreEqual(TapOutcome.Exit, _handler.Tap(frozen).Outcome);
            Assert.IsTrue(_board.IsCleared);
        }

        [Test]
        public void Tap_Frozen_Hits3_NeedsTwoIceTaps()
        {
            var frozen = Place("f", ArrowType.Frozen, Direction.Up, 2, 2, hits: 3);

            var first = _handler.Tap(frozen);
            var second = _handler.Tap(frozen);
            var third = _handler.Tap(frozen);

            Assert.AreEqual(TapOutcome.IceBroken, first.Outcome);
            Assert.AreEqual(2, first.RemainingHits);
            Assert.AreEqual(TapOutcome.IceBroken, second.Outcome);
            Assert.AreEqual(1, second.RemainingHits);
            Assert.AreEqual(TapOutcome.Exit, third.Outcome);
        }

        [Test]
        public void Tap_Frozen_IceStaysBroken_AfterBlock()
        {
            var frozen = Place("f", ArrowType.Frozen, Direction.Up, 2, 1, hits: 2);
            var blocker = Place("b", ArrowType.Basic, Direction.Left, 2, 2);
            _handler.Tap(frozen);
            _handler.Tap(frozen);
            _handler.Tap(blocker);

            Assert.AreEqual(TapOutcome.Exit, _handler.Tap(frozen).Outcome);
        }

        [Test]
        public void Tap_Locked_WhileKeyOnBoard_ReturnsLocked_NoLifeLost()
        {
            var locked = Place("l", ArrowType.Locked, Direction.Up, 2, 2, keyGroup: "red");
            Place("k", ArrowType.Key, Direction.Down, 0, 0, keyGroup: "red");

            var result = _handler.Tap(locked);

            Assert.AreEqual(TapOutcome.Locked, result.Outcome);
            Assert.AreEqual(MaxLives, _lives.Lives);
            Assert.IsFalse(_lives.IsMarked(locked));
            Assert.AreSame(locked, _board.GetArrow("l"));
        }

        [Test]
        public void Tap_Locked_AfterKeyExits_Fires()
        {
            var locked = Place("l", ArrowType.Locked, Direction.Up, 2, 2, keyGroup: "red");
            var key = Place("k", ArrowType.Key, Direction.Down, 0, 0, keyGroup: "red");
            _handler.Tap(key);

            Assert.AreEqual(TapOutcome.Exit, _handler.Tap(locked).Outcome);
        }

        [Test]
        public void Tap_Locked_StaysLocked_UntilEveryKeyOfGroupExits()
        {
            var locked = Place("l", ArrowType.Locked, Direction.Up, 2, 2, keyGroup: "red");
            var key1 = Place("k1", ArrowType.Key, Direction.Down, 0, 0, keyGroup: "red");
            Place("k2", ArrowType.Key, Direction.Down, 1, 0, keyGroup: "red");
            _handler.Tap(key1);

            Assert.AreEqual(TapOutcome.Locked, _handler.Tap(locked).Outcome);
        }

        [Test]
        public void Tap_Locked_KeyOfOtherGroup_DoesNotLock()
        {
            var locked = Place("l", ArrowType.Locked, Direction.Up, 2, 2, keyGroup: "red");
            Place("k", ArrowType.Key, Direction.Down, 0, 0, keyGroup: "blue");

            Assert.AreEqual(TapOutcome.Exit, _handler.Tap(locked).Outcome);
        }

        [Test]
        public void Tap_Key_BehavesLikeBasic()
        {
            var key = Place("k", ArrowType.Key, Direction.Up, 2, 1, keyGroup: "red");
            Place("b", ArrowType.Basic, Direction.Left, 2, 3);

            var result = _handler.Tap(key);

            Assert.AreEqual(TapOutcome.Blocked, result.Outcome);
            Assert.IsTrue(result.LifeLost);
        }
    }
}
