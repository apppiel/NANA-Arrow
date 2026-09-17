using System;
using NanaArrow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class ArrowTests
    {
        private static Vector2Int C(int x, int y) => new Vector2Int(x, y);

        [Test]
        public void SingleCell_UsesGivenDirection_HeadEqualsTail()
        {
            var arrow = new Arrow("a", ArrowType.Basic, Direction.Left, C(2, 3));

            Assert.AreEqual(Direction.Left, arrow.Direction);
            Assert.AreEqual(C(2, 3), arrow.Head);
            Assert.AreEqual(C(2, 3), arrow.Tail);
            Assert.AreEqual(1, arrow.Length);
        }

        [Test]
        public void Path_HeadIsLastCell_TailIsFirst()
        {
            var arrow = new Arrow("a", ArrowType.Basic, Direction.Right, C(1, 4), C(2, 4), C(3, 4));

            Assert.AreEqual(C(3, 4), arrow.Head);
            Assert.AreEqual(C(1, 4), arrow.Tail);
            Assert.AreEqual(3, arrow.Length);
        }

        [TestCase(Direction.Up, 0, 1)]
        [TestCase(Direction.Down, 0, -1)]
        [TestCase(Direction.Left, -1, 0)]
        [TestCase(Direction.Right, 1, 0)]
        public void Path_DirectionComesFromLastStep(Direction expected, int dx, int dy)
        {
            var head = C(5, 5);
            var arrow = new Arrow("a", ArrowType.Basic, expected, C(5 - dx, 5 - dy), head);

            Assert.AreEqual(expected, arrow.Direction);
        }

        [Test]
        public void BentPath_DirectionIsLastSegmentOnly()
        {
            // 위로 올라가다 오른쪽으로 꺾임 → 머리 방향 Right
            var arrow = new Arrow("a", ArrowType.Basic, Direction.Right, C(0, 0), C(0, 1), C(0, 2), C(1, 2));

            Assert.AreEqual(Direction.Right, arrow.Direction);
            Assert.AreEqual(C(1, 2), arrow.Head);
        }

        [Test]
        public void Path_DirectionMismatch_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Arrow("a", ArrowType.Basic, Direction.Up, C(0, 0), C(1, 0)));
        }

        [Test]
        public void Path_LastStepNotAdjacent_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Arrow("a", ArrowType.Basic, Direction.Right, C(0, 0), C(2, 0)));
            Assert.Throws<ArgumentException>(() => new Arrow("a", ArrowType.Basic, Direction.Right, C(0, 0), C(1, 1)));
        }

        [Test]
        public void Cells_AreCopied_NotAliased()
        {
            var input = new[] { C(1, 1) };
            var arrow = new Arrow("a", ArrowType.Basic, Direction.Up, input);

            input[0] = C(9, 9);

            Assert.AreEqual(C(1, 1), arrow.Cells[0]);
        }

        [Test]
        public void Constructor_ParamsCells_DefaultsHitsAndKeyGroup()
        {
            var arrow = new Arrow("a", ArrowType.Basic, Direction.Up, C(0, 0));

            Assert.AreEqual(Arrow.DefaultHits, arrow.Hits);
            Assert.IsNull(arrow.KeyGroup);
        }

        [Test]
        public void Constructor_WithHitsAndKeyGroup_KeepsThem()
        {
            var arrow = new Arrow("a", ArrowType.Locked, Direction.Up, new[] { C(0, 0) }, 2, "red");

            Assert.AreEqual(2, arrow.Hits);
            Assert.AreEqual("red", arrow.KeyGroup);
        }

        [Test]
        public void Constructor_NoCells_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Arrow("a", ArrowType.Basic, Direction.Up));
        }
    }
}
