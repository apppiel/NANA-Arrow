using System;
using NanaArrow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class BoardTests
    {
        private static Arrow Basic(string id, int x, int y) =>
            new Arrow(id, ArrowType.Basic, Direction.Up, new Vector2Int(x, y));

        [Test]
        public void NewBoard_HasNoArrows_AndIsCleared()
        {
            var board = new Board(5, 5);

            Assert.AreEqual(0, board.Arrows.Count);
            Assert.IsTrue(board.IsCleared);
        }

        [TestCase(0, 0, true)]
        [TestCase(4, 4, true)]
        [TestCase(5, 0, false)]
        [TestCase(0, 5, false)]
        [TestCase(-1, 0, false)]
        [TestCase(0, -1, false)]
        public void IsInside_5x5(int x, int y, bool expected)
        {
            var board = new Board(5, 5);

            Assert.AreEqual(expected, board.IsInside(new Vector2Int(x, y)));
        }

        [Test]
        public void Place_LongArrow_EveryCellResolvesToIt()
        {
            var board = new Board(5, 5);
            var arrow = new Arrow("a", ArrowType.Long, Direction.Right,
                new Vector2Int(1, 4), new Vector2Int(2, 4), new Vector2Int(3, 4));

            board.Place(arrow);

            Assert.AreSame(arrow, board.GetArrowAt(new Vector2Int(1, 4)));
            Assert.AreSame(arrow, board.GetArrowAt(new Vector2Int(2, 4)));
            Assert.AreSame(arrow, board.GetArrowAt(new Vector2Int(3, 4)));
            Assert.AreSame(arrow, board.GetArrow("a"));
            Assert.IsFalse(board.IsCleared);
        }

        [Test]
        public void GetArrowAt_EmptyCell_ReturnsNull()
        {
            var board = new Board(5, 5);
            board.Place(Basic("a", 2, 2));

            Assert.IsNull(board.GetArrowAt(new Vector2Int(2, 3)));
            Assert.IsNull(board.GetArrow("missing"));
        }

        [Test]
        public void Place_OutsideBoard_Throws()
        {
            var board = new Board(5, 5);

            Assert.Throws<ArgumentOutOfRangeException>(() => board.Place(Basic("a", 5, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => board.Place(Basic("b", 0, -1)));
            Assert.AreEqual(0, board.Arrows.Count);
        }

        [Test]
        public void Place_OverlappingCell_Throws()
        {
            var board = new Board(5, 5);
            board.Place(Basic("a", 2, 2));

            Assert.Throws<InvalidOperationException>(() => board.Place(Basic("b", 2, 2)));
            Assert.AreEqual(1, board.Arrows.Count);
        }

        [Test]
        public void Place_DuplicateId_Throws()
        {
            var board = new Board(5, 5);
            board.Place(Basic("a", 0, 0));

            Assert.Throws<InvalidOperationException>(() => board.Place(Basic("a", 1, 1)));
        }

        [Test]
        public void Remove_ClearsCellsAndArrowList()
        {
            var board = new Board(5, 5);
            var arrow = new Arrow("a", ArrowType.Long, Direction.Up, new Vector2Int(0, 0), new Vector2Int(0, 1));
            board.Place(arrow);

            var removed = board.Remove(arrow);

            Assert.IsTrue(removed);
            Assert.IsNull(board.GetArrowAt(new Vector2Int(0, 0)));
            Assert.IsNull(board.GetArrowAt(new Vector2Int(0, 1)));
            Assert.IsNull(board.GetArrow("a"));
            Assert.IsTrue(board.IsCleared);
        }

        [Test]
        public void Remove_NotPlaced_ReturnsFalse()
        {
            var board = new Board(5, 5);

            Assert.IsFalse(board.Remove(Basic("a", 0, 0)));
        }

        [Test]
        public void Remove_AfterRemove_CellIsFreeForNewArrow()
        {
            var board = new Board(5, 5);
            var first = Basic("a", 2, 2);
            board.Place(first);
            board.Remove(first);

            Assert.DoesNotThrow(() => board.Place(Basic("b", 2, 2)));
        }
    }
}
