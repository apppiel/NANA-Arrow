using NanaArrow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class FireResolverTests
    {
        private Board _board;

        private static Arrow Basic(string id, int x, int y, Direction direction) =>
            new Arrow(id, ArrowType.Basic, direction, new Vector2Int(x, y));

        [SetUp]
        public void SetUp()
        {
            _board = new Board(5, 5);
        }

        [TestCase(Direction.Up)]
        [TestCase(Direction.Down)]
        [TestCase(Direction.Left)]
        [TestCase(Direction.Right)]
        public void Resolve_EmptyPathFromCenter_IsExit_WithTwoFreeCells(Direction direction)
        {
            var arrow = Basic("a", 2, 2, direction);
            _board.Place(arrow);

            var result = FireResolver.Resolve(_board, arrow);

            Assert.IsTrue(result.IsExit);
            Assert.IsFalse(result.IsBlocked);
            Assert.IsNull(result.BlockedBy);
            Assert.AreEqual(2, result.FreeCells);
        }

        [Test]
        public void Resolve_HeadAtEdge_IsExit_WithZeroFreeCells()
        {
            var arrow = Basic("a", 2, 4, Direction.Up);
            _board.Place(arrow);

            var result = FireResolver.Resolve(_board, arrow);

            Assert.IsTrue(result.IsExit);
            Assert.AreEqual(0, result.FreeCells);
        }

        [Test]
        public void Resolve_AdjacentBlocker_IsBlocked_WithZeroFreeCells()
        {
            var arrow = Basic("a", 2, 2, Direction.Up);
            var blocker = Basic("b", 2, 3, Direction.Left);
            _board.Place(arrow);
            _board.Place(blocker);

            var result = FireResolver.Resolve(_board, arrow);

            Assert.IsTrue(result.IsBlocked);
            Assert.AreSame(blocker, result.BlockedBy);
            Assert.AreEqual(0, result.FreeCells);
        }

        [Test]
        public void Resolve_BlockerWithGap_IsBlocked_WithFreeCellsUpToBlocker()
        {
            var arrow = Basic("a", 2, 0, Direction.Up);
            var blocker = Basic("b", 2, 3, Direction.Left);
            _board.Place(arrow);
            _board.Place(blocker);

            var result = FireResolver.Resolve(_board, arrow);

            Assert.IsTrue(result.IsBlocked);
            Assert.AreSame(blocker, result.BlockedBy);
            Assert.AreEqual(2, result.FreeCells);
        }

        [Test]
        public void Resolve_FirstBlockerWins_WhenSeveralInPath()
        {
            var arrow = Basic("a", 2, 0, Direction.Up);
            var near = Basic("b", 2, 2, Direction.Left);
            var far = Basic("c", 2, 4, Direction.Left);
            _board.Place(arrow);
            _board.Place(near);
            _board.Place(far);

            var result = FireResolver.Resolve(_board, arrow);

            Assert.AreSame(near, result.BlockedBy);
            Assert.AreEqual(1, result.FreeCells);
        }

        [Test]
        public void Resolve_ArrowBehind_DoesNotBlock()
        {
            var arrow = Basic("a", 2, 2, Direction.Up);
            var behind = Basic("b", 2, 1, Direction.Up);
            _board.Place(arrow);
            _board.Place(behind);

            Assert.IsTrue(FireResolver.Resolve(_board, arrow).IsExit);
        }

        [Test]
        public void Resolve_ArrowOffAxis_DoesNotBlock()
        {
            var arrow = Basic("a", 2, 2, Direction.Up);
            var beside = Basic("b", 3, 3, Direction.Up);
            _board.Place(arrow);
            _board.Place(beside);

            Assert.IsTrue(FireResolver.Resolve(_board, arrow).IsExit);
        }

        [Test]
        public void Resolve_LongArrow_ChecksFromHead_NotFromTail()
        {
            var arrow = new Arrow("a", ArrowType.Long, Direction.Right,
                new Vector2Int(1, 4), new Vector2Int(2, 4), new Vector2Int(3, 4));
            _board.Place(arrow);

            var result = FireResolver.Resolve(_board, arrow);

            Assert.IsTrue(result.IsExit);
            Assert.AreEqual(1, result.FreeCells);
        }

        [Test]
        public void Resolve_LongArrow_BlockedRightInFrontOfHead()
        {
            var arrow = new Arrow("a", ArrowType.Long, Direction.Right,
                new Vector2Int(1, 4), new Vector2Int(2, 4), new Vector2Int(3, 4));
            var blocker = Basic("b", 4, 4, Direction.Down);
            _board.Place(arrow);
            _board.Place(blocker);

            var result = FireResolver.Resolve(_board, arrow);

            Assert.AreSame(blocker, result.BlockedBy);
            Assert.AreEqual(0, result.FreeCells);
        }

        [Test]
        public void Resolve_DoesNotModifyBoard()
        {
            var arrow = Basic("a", 2, 2, Direction.Up);
            _board.Place(arrow);

            FireResolver.Resolve(_board, arrow);

            Assert.AreSame(arrow, _board.GetArrowAt(new Vector2Int(2, 2)));
            Assert.AreEqual(1, _board.Arrows.Count);
        }
    }
}
