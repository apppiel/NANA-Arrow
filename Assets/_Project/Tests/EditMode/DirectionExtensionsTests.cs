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
    }
}
