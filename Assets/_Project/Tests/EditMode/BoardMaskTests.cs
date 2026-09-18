using System.Linq;
using NanaArrow.Data;
using NanaArrow.Gameplay;
using NanaArrow.Gameplay.View;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    /// <summary>비직사각 보드 마스크 (LEVEL_FORMAT v0.7, W-027).</summary>
    public class BoardMaskTests
    {
        /// <summary>WORK.md W-027 의 하트 예시 (7×6).</summary>
        private static readonly string[] Heart =
        {
            ".##.##.",
            "#######",
            "#######",
            ".#####.",
            "..###..",
            "...#...",
        };

        private static BoardMask Parse(string[] rows, int w, int h)
        {
            Assert.IsTrue(BoardMask.TryParse(rows, w, h, out var mask, out var error), error);
            return mask;
        }

        [Test]
        public void TryParse_NoMask_IsFullRectangle()
        {
            Assert.IsTrue(BoardMask.TryParse(null, 5, 6, out var mask, out var error), error);
            Assert.IsNull(mask, "mask 가 없으면 null = 전체 사각형");

            Assert.IsTrue(BoardMask.TryParse(new string[0], 5, 6, out mask, out error), error);
            Assert.IsNull(mask);
        }

        [Test]
        public void TryParse_Heart_CountsIncludedCells()
        {
            var mask = Parse(Heart, 7, 6);

            Assert.AreEqual(7, mask.Width);
            Assert.AreEqual(6, mask.Height);
            // 4 + 7 + 7 + 5 + 3 + 1
            Assert.AreEqual(27, mask.IncludedCount);
        }

        [Test]
        public void TryParse_FirstRowIsTop_SoYIsFlipped()
        {
            var mask = Parse(Heart, 7, 6);

            // mask[0] = 맨 윗줄 ".##.##." → y = 5
            Assert.IsFalse(mask.Contains(0, 5), "윗줄 첫 칸은 '.'");
            Assert.IsTrue(mask.Contains(1, 5), "윗줄 둘째 칸은 '#'");
            Assert.IsFalse(mask.Contains(3, 5), "윗줄 가운데는 '.' (하트의 파인 곳)");

            // mask[5] = 맨 아랫줄 "...#..." → y = 0
            Assert.IsTrue(mask.Contains(3, 0), "아랫줄 가운데는 '#' (하트 꼭짓점)");
            Assert.IsFalse(mask.Contains(0, 0), "아랫줄 첫 칸은 '.'");
        }

        [Test]
        public void Contains_OutsideRect_IsFalse()
        {
            var mask = Parse(Heart, 7, 6);

            Assert.IsFalse(mask.Contains(-1, 0));
            Assert.IsFalse(mask.Contains(7, 0));
            Assert.IsFalse(mask.Contains(0, 6));
        }

        [Test]
        public void ToRows_RoundTrips()
        {
            var mask = Parse(Heart, 7, 6);

            CollectionAssert.AreEqual(Heart, mask.ToRows());
        }

        [TestCase(new[] { "###", "###" }, 3, 3, "rows", TestName = "행 수가 height 와 다르면 실패")]
        [TestCase(new[] { "##", "###", "###" }, 3, 3, "characters", TestName = "행 길이가 width 와 다르면 실패")]
        [TestCase(new[] { "#x#", "###", "###" }, 3, 3, "unexpected", TestName = "모르는 문자면 실패")]
        public void TryParse_Invalid_ReportsError(string[] rows, int w, int h, string expectedFragment)
        {
            Assert.IsFalse(BoardMask.TryParse(rows, w, h, out var mask, out var error));
            Assert.IsNull(mask);
            StringAssert.Contains(expectedFragment, error);
        }

        // ── Board 연동

        [Test]
        public void Board_WithoutMask_EverythingInsideIsUsable()
        {
            var board = new Board(3, 2);

            Assert.IsNull(board.Mask);
            Assert.IsTrue(board.IsInMask(new Vector2Int(2, 1)));
            Assert.AreEqual(6, board.UsableCellCount);
        }

        [Test]
        public void Board_WithMask_IsInsideIgnoresMask()
        {
            var board = new Board(7, 6, Parse(Heart, 7, 6));

            var notch = new Vector2Int(3, 5);   // 하트 윗변 가운데 파인 곳
            Assert.IsTrue(board.IsInside(notch), "레인 판정은 사각형 기준이라 마스크 밖도 '안'이다 (W-027)");
            Assert.IsFalse(board.IsInMask(notch), "표시·검증은 마스크 기준");
            Assert.AreEqual(27, board.UsableCellCount);
        }

        [Test]
        public void Board_MaskSizeMismatch_Throws()
        {
            var mask = Parse(Heart, 7, 6);

            Assert.Throws<System.ArgumentException>(() => new Board(5, 6, mask));
        }

        // ── 로더

        [Test]
        public void Loader_ParsesMaskFromJson()
        {
            const string json = @"{
              ""version"": 1, ""id"": 99, ""width"": 3, ""height"": 3,
              ""mask"": [ "".#."", ""###"", "".#."" ],
              ""arrows"": [ { ""id"": ""a1"", ""type"": ""Basic"", ""dir"": ""Up"", ""cells"": [[1,1]] } ]
            }";

            var level = LevelLoader.Parse(json);
            CollectionAssert.AreEqual(new[] { ".#.", "###", ".#." }, level.Mask);

            var board = LevelLoader.CreateBoard(level, 2);
            Assert.AreEqual(5, board.UsableCellCount, "십자 모양 = 5칸");
            Assert.IsFalse(board.IsInMask(new Vector2Int(0, 0)), "모서리는 마스크 밖");
        }

        [Test]
        public void Loader_NoMask_BoardHasNoMask()
        {
            var board = LevelLoader.CreateBoard(TestLevels.FormatExample(), 2);

            Assert.IsNull(board.Mask);
        }

        // ── 빈 칸 점 (W-027: 마스크 안에만)

        [Test]
        public void EmptyCells_FilteredByMask_OnlyCountsUsableCells()
        {
            var mask = Parse(new[] { ".#.", "###", ".#." }, 3, 3);
            var arrow = new Arrow("a1", ArrowType.Basic, Direction.Up, new Vector2Int(1, 1));

            var all = LaneGuides.EmptyCells(3, 3, new[] { arrow });
            var inMask = all.Where(c => mask.Contains(c)).ToList();

            Assert.AreEqual(8, all.Count, "사각형 기준 빈 칸은 8개");
            Assert.AreEqual(4, inMask.Count, "마스크 안 빈 칸은 십자 5칸 - Arrow 1칸 = 4개");
            Assert.IsFalse(inMask.Contains(new Vector2Int(0, 0)), "모서리엔 점이 없다");
        }
    }
}
