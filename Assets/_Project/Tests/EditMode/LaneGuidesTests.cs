using System.Linq;
using NanaArrow.Gameplay;
using NanaArrow.Gameplay.View;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    /// <summary>레인 가이드·빈 칸 점 계산 (GAME_RULES v0.7.3 §9, W-025 7·8).</summary>
    public class LaneGuidesTests
    {
        private static Arrow Make(string id, Direction dir, params Vector2Int[] cells) =>
            new Arrow(id, ArrowType.Basic, dir, cells);

        private static Vector2Int C(int x, int y) => new Vector2Int(x, y);

        [TestCase(Direction.Left, true)]
        [TestCase(Direction.Right, true)]
        [TestCase(Direction.Up, false)]
        [TestCase(Direction.Down, false)]
        public void IsHorizontal_ByDirection(Direction dir, bool expected)
        {
            Assert.AreEqual(expected, LaneGuides.IsHorizontal(dir));
        }

        [Test]
        public void LineFor_HorizontalArrow_UsesHeadRow()
        {
            var arrow = Make("a", Direction.Right, C(0, 3), C(1, 3), C(2, 3));

            var line = LaneGuides.LineFor(arrow);

            Assert.IsTrue(line.Horizontal);
            Assert.AreEqual(3, line.Index, "머리가 있는 행");
        }

        [Test]
        public void LineFor_VerticalArrow_UsesHeadColumn()
        {
            var arrow = Make("a", Direction.Up, C(4, 0), C(4, 1));

            var line = LaneGuides.LineFor(arrow);

            Assert.IsFalse(line.Horizontal);
            Assert.AreEqual(4, line.Index, "머리가 있는 열");
        }

        [Test]
        public void LineFor_BentArrow_UsesHeadCellNotTail()
        {
            // 꼬리는 (0,0) 이고 가로로 가다 꺾여 위로 올라간다 → 머리 (2,2), 열 2 (꼬리의 행 0 이 아니다)
            var arrow = Make("a", Direction.Up, C(0, 0), C(1, 0), C(2, 0), C(2, 1), C(2, 2));

            var line = LaneGuides.LineFor(arrow);

            Assert.IsFalse(line.Horizontal);
            Assert.AreEqual(2, line.Index);
        }

        [Test]
        public void For_DeduplicatesSameRowOrColumn()
        {
            var arrows = new[]
            {
                Make("a", Direction.Right, C(0, 2)),
                Make("b", Direction.Left, C(5, 2)),   // 같은 행 2
                Make("c", Direction.Up, C(3, 0)),
            };

            var lines = LaneGuides.For(arrows);

            Assert.AreEqual(2, lines.Count, "같은 행은 선 하나");
            Assert.AreEqual(1, lines.Count(l => l.Horizontal && l.Index == 2));
            Assert.AreEqual(1, lines.Count(l => !l.Horizontal && l.Index == 3));
        }

        [Test]
        public void For_RowAndColumnWithSameIndex_AreSeparateLines()
        {
            var arrows = new[]
            {
                Make("a", Direction.Right, C(0, 2)),
                Make("b", Direction.Up, C(2, 0)),
            };

            var lines = LaneGuides.For(arrows);

            Assert.AreEqual(2, lines.Count, "행 2 와 열 2 는 다른 선");
        }

        [Test]
        public void For_NoArrows_NoLines()
        {
            Assert.IsEmpty(LaneGuides.For(new Arrow[0]), "Arrow 가 없으면 격자도 없다");
        }

        [Test]
        public void EmptyCells_ExcludesEveryArrowCell()
        {
            var arrows = new[] { Make("a", Direction.Up, C(0, 0), C(0, 1)) };

            var empty = LaneGuides.EmptyCells(2, 2, arrows);

            CollectionAssert.AreEquivalent(new[] { C(1, 0), C(1, 1) }, empty);
        }

        [Test]
        public void EmptyCells_CountsWholeBoardWhenNoArrows()
        {
            Assert.AreEqual(3 * 4, LaneGuides.EmptyCells(3, 4, new Arrow[0]).Count);
        }

        [Test]
        public void EmptyCells_ArrowThatLeftFreesItsCells()
        {
            var a = Make("a", Direction.Up, C(1, 0));
            var b = Make("b", Direction.Up, C(2, 0));

            var before = LaneGuides.EmptyCells(3, 1, new[] { a, b }).Count;
            var after = LaneGuides.EmptyCells(3, 1, new[] { b }).Count;

            Assert.AreEqual(1, before);
            Assert.AreEqual(2, after, "Arrow 가 나가면 그 칸에도 점이 생긴다");
        }

        [Test]
        public void EmptyCells_OnlyInsideBoardRect()
        {
            var empty = LaneGuides.EmptyCells(2, 2, new Arrow[0]);

            Assert.IsTrue(empty.All(c => c.x >= 0 && c.x < 2 && c.y >= 0 && c.y < 2), "보드 바깥엔 점이 없다");
        }
    }
}
