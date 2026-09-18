using NanaArrow.Gameplay.View;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class BoardLayoutTests
    {
        private const float CellSize = 1f;
        private const float CellGap = 0.1f;
        private const float Tolerance = 1e-4f;

        [Test]
        public void CellSizeFor_NarrowBoard_UsesFixedFractionOfScreenWidth()
        {
            // 5칸: 0.052 < 0.9/5 = 0.18 → 셀 = 폭 × 0.052
            Assert.AreEqual(10f * 0.052f, BoardLayout.CellSizeFor(10f, 5, 0.052f, 0.9f), Tolerance);
        }

        [Test]
        public void CellSizeFor_WideBoard_ShrinksToFitMaxArea()
        {
            // 20칸: 0.9/20 = 0.045 < 0.052 → 셀 = 폭 × 0.045
            Assert.AreEqual(10f * 0.045f, BoardLayout.CellSizeFor(10f, 20, 0.052f, 0.9f), Tolerance);
        }

        [Test]
        public void CellSizeFor_SameCellSizeRegardlessOfBoardWidth_UntilLimit()
        {
            var small = BoardLayout.CellSizeFor(10f, 4, 0.052f, 0.9f);
            var medium = BoardLayout.CellSizeFor(10f, 10, 0.052f, 0.9f);

            Assert.AreEqual(small, medium, Tolerance);
        }

        [Test]
        public void Constructor_BoardSizeAndPitch_FollowCellAndGap()
        {
            var layout = new BoardLayout(4, 3, 0.5f, 0.1f, Vector2.zero);

            Assert.AreEqual(0.6f, layout.Pitch, Tolerance);
            Assert.AreEqual(4 * 0.5f + 3 * 0.1f, layout.BoardSize.x, Tolerance);
            Assert.AreEqual(3 * 0.5f + 2 * 0.1f, layout.BoardSize.y, Tolerance);
        }

        [Test]
        public void CellToWorld_CornersAreSymmetricAroundCenter()
        {
            var center = new Vector2(3f, -2f);
            var layout = new BoardLayout(5, 4, CellSize, CellGap, center);

            var bottomLeft = layout.CellToWorld(new Vector2Int(0, 0));
            var topRight = layout.CellToWorld(new Vector2Int(4, 3));

            Assert.AreEqual(center.x, (bottomLeft.x + topRight.x) * 0.5f, Tolerance);
            Assert.AreEqual(center.y, (bottomLeft.y + topRight.y) * 0.5f, Tolerance);
            Assert.Less(bottomLeft.x, topRight.x);
            Assert.Less(bottomLeft.y, topRight.y);
        }

        [Test]
        public void CellToWorld_NeighboursAreOnePitchApart()
        {
            var layout = new BoardLayout(5, 5, CellSize, CellGap, Vector2.zero);

            var delta = layout.CellToWorld(new Vector2Int(1, 0)) - layout.CellToWorld(new Vector2Int(0, 0));

            Assert.AreEqual(CellSize + CellGap, layout.Pitch, Tolerance);
            Assert.AreEqual(layout.Pitch, delta.x, Tolerance);
            Assert.AreEqual(0f, delta.y, Tolerance);
        }

        [Test]
        public void TryWorldToCell_RoundTripsEveryCell()
        {
            var layout = new BoardLayout(6, 4, 0.5f, 0.05f, new Vector2(1f, 1f));

            for (var y = 0; y < 4; y++)
            for (var x = 0; x < 6; x++)
            {
                var cell = new Vector2Int(x, y);
                Assert.IsTrue(layout.TryWorldToCell(layout.CellToWorld(cell), out var found));
                Assert.AreEqual(cell, found);
            }
        }

        [Test]
        public void TryWorldToCell_PicksNearestCellInsideGap()
        {
            var layout = new BoardLayout(5, 5, CellSize, CellGap, Vector2.zero);
            var a = layout.CellToWorld(new Vector2Int(0, 0));
            var b = layout.CellToWorld(new Vector2Int(1, 0));
            var nearB = Vector2.Lerp(a, b, 0.6f);

            Assert.IsTrue(layout.TryWorldToCell(nearB, out var cell));
            Assert.AreEqual(new Vector2Int(1, 0), cell);
        }

        [Test]
        public void TryWorldToCell_OutsideBoard_IsFalse()
        {
            var layout = new BoardLayout(5, 5, CellSize, CellGap, Vector2.zero);

            Assert.IsFalse(layout.TryWorldToCell(new Vector2(50f, 0f), out _));
            Assert.IsFalse(layout.TryWorldToCell(layout.CellToWorld(new Vector2Int(0, 0)) - new Vector2(layout.Pitch, 0f), out _));
        }

        [Test]
        public void Grid_SpansWholeBoard_CenteredOnCenter()
        {
            var layout = new BoardLayout(5, 7, CellSize, CellGap, Vector2.zero);

            Assert.AreEqual(-5 * layout.Pitch * 0.5f, layout.GridMin.x, Tolerance);
            Assert.AreEqual(-7 * layout.Pitch * 0.5f, layout.GridMin.y, Tolerance);
            Assert.AreEqual(5 * layout.Pitch * 0.5f, layout.GridMax.x, Tolerance);
            Assert.AreEqual(7 * layout.Pitch * 0.5f, layout.GridMax.y, Tolerance);
        }

        [Test]
        public void Grid_CellCentersSitMidwayBetweenGridLines()
        {
            var layout = new BoardLayout(5, 7, CellSize, CellGap, new Vector2(3f, -2f));

            for (var x = 0; x < layout.Width; x++)
            {
                var lineBefore = layout.GridMin.x + x * layout.Pitch;
                Assert.AreEqual(lineBefore + layout.Pitch * 0.5f, layout.CellToWorld(new Vector2Int(x, 0)).x, Tolerance);
            }
        }
    }
}
