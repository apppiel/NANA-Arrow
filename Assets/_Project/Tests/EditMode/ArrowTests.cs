using System;
using NanaArrow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class ArrowTests
    {
        [Test]
        public void Head_BasicArrow_IsItsOnlyCell()
        {
            var arrow = new Arrow("a", ArrowType.Basic, Direction.Up, new Vector2Int(2, 3));

            Assert.AreEqual(new Vector2Int(2, 3), arrow.Head);
        }

        [TestCase(Direction.Right, 3)]
        [TestCase(Direction.Left, 1)]
        public void Head_HorizontalLongArrow_IsFrontCellAlongDirection(Direction direction, int expectedX)
        {
            var arrow = new Arrow("a", ArrowType.Long, direction,
                new Vector2Int(1, 4), new Vector2Int(2, 4), new Vector2Int(3, 4));

            Assert.AreEqual(new Vector2Int(expectedX, 4), arrow.Head);
        }

        [TestCase(Direction.Up, 2)]
        [TestCase(Direction.Down, 0)]
        public void Head_VerticalLongArrow_IsFrontCellAlongDirection(Direction direction, int expectedY)
        {
            var arrow = new Arrow("a", ArrowType.Long, direction,
                new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2));

            Assert.AreEqual(new Vector2Int(0, expectedY), arrow.Head);
        }

        [Test]
        public void Head_CellOrderDoesNotMatter()
        {
            var arrow = new Arrow("a", ArrowType.Long, Direction.Right,
                new Vector2Int(3, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));

            Assert.AreEqual(new Vector2Int(3, 0), arrow.Head);
        }

        [Test]
        public void Cells_AreCopied_NotAliased()
        {
            var input = new[] { new Vector2Int(1, 1) };
            var arrow = new Arrow("a", ArrowType.Basic, Direction.Up, input);

            input[0] = new Vector2Int(9, 9);

            Assert.AreEqual(new Vector2Int(1, 1), arrow.Cells[0]);
        }

        [Test]
        public void Constructor_NoCells_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Arrow("a", ArrowType.Basic, Direction.Up));
        }
    }
}
