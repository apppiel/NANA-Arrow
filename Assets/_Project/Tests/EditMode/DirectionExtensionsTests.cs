using NanaArrow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class DirectionExtensionsTests
    {
        [TestCase(Direction.Up, 0, 1)]
        [TestCase(Direction.Down, 0, -1)]
        [TestCase(Direction.Left, -1, 0)]
        [TestCase(Direction.Right, 1, 0)]
        public void ToOffset_ReturnsUnitStep(Direction direction, int x, int y)
        {
            Assert.AreEqual(new Vector2Int(x, y), direction.ToOffset());
        }

        [TestCase(Direction.Up, 0, 1)]
        [TestCase(Direction.Down, 0, -1)]
        [TestCase(Direction.Left, -1, 0)]
        [TestCase(Direction.Right, 1, 0)]
        public void TryFromOffset_RoundTripsUnitStep(Direction expected, int x, int y)
        {
            Assert.IsTrue(DirectionExtensions.TryFromOffset(new Vector2Int(x, y), out var direction));
            Assert.AreEqual(expected, direction);
        }

        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(2, 0)]
        [TestCase(0, -3)]
        public void TryFromOffset_NonUnitStep_IsFalse(int x, int y)
        {
            Assert.IsFalse(DirectionExtensions.TryFromOffset(new Vector2Int(x, y), out _));
        }
    }
}
