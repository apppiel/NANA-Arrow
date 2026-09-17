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
        public void Scale_ShrinksBoardThatExceedsArea()
        {
            // 10칸: 10 + 9*0.1 = 10.9 → 영역 5.45 에 맞추면 0.5
            var layout = new BoardLayout(10, 10, CellSize, CellGap, new Vector2(5.45f, 20f), Vector2.zero);

            Assert.AreEqual(0.5f, layout.Scale, Tolerance);
            Assert.AreEqual(0.5f, layout.CellSize, Tolerance);
            Assert.AreEqual(0.05f, layout.CellGap, Tolerance);
            Assert.AreEqual(5.45f, layout.BoardSize.x, Tolerance);
            Assert.AreEqual(5.45f, layout.BoardSize.y, Tolerance);
        }

        [Test]
        public void Scale_UsesTighterAxis()
        {
            var layout = new BoardLayout(5, 5, CellSize, CellGap, new Vector2(20f, 2.7f), Vector2.zero);

            Assert.AreEqual(0.5f, layout.Scale, Tolerance);
        }

        [Test]
        public void Scale_NeverUpscalesSmallBoard()
        {
            var layout = new BoardLayout(3, 3, CellSize, CellGap, new Vector2(100f, 100f), Vector2.zero);

            Assert.AreEqual(1f, layout.Scale, Tolerance);
            Assert.AreEqual(CellSize, layout.CellSize, Tolerance);
        }

        [Test]
        public void CellToWorld_CornersAreSymmetricAroundCenter()
        {
            var center = new Vector2(3f, -2f);
            var layout = new BoardLayout(5, 4, CellSize, CellGap, new Vector2(100f, 100f), center);

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
            var layout = new BoardLayout(5, 5, CellSize, CellGap, new Vector2(100f, 100f), Vector2.zero);

            var delta = layout.CellToWorld(new Vector2Int(1, 0)) - layout.CellToWorld(new Vector2Int(0, 0));

            Assert.AreEqual(CellSize + CellGap, layout.Pitch, Tolerance);
            Assert.AreEqual(layout.Pitch, delta.x, Tolerance);
            Assert.AreEqual(0f, delta.y, Tolerance);
        }

        [Test]
        public void TryWorldToCell_RoundTripsEveryCell()
        {
            var layout = new BoardLayout(6, 4, CellSize, CellGap, new Vector2(3f, 3f), new Vector2(1f, 1f));

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
            var layout = new BoardLayout(5, 5, CellSize, CellGap, new Vector2(100f, 100f), Vector2.zero);
            var a = layout.CellToWorld(new Vector2Int(0, 0));
            var b = layout.CellToWorld(new Vector2Int(1, 0));
            var nearB = Vector2.Lerp(a, b, 0.6f);

            Assert.IsTrue(layout.TryWorldToCell(nearB, out var cell));
            Assert.AreEqual(new Vector2Int(1, 0), cell);
        }

        [Test]
        public void TryWorldToCell_OutsideBoard_IsFalse()
        {
            var layout = new BoardLayout(5, 5, CellSize, CellGap, new Vector2(100f, 100f), Vector2.zero);

            Assert.IsFalse(layout.TryWorldToCell(new Vector2(50f, 0f), out _));
            Assert.IsFalse(layout.TryWorldToCell(layout.CellToWorld(new Vector2Int(0, 0)) - new Vector2(layout.Pitch, 0f), out _));
        }
    }
}
