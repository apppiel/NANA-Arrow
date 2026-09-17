using NanaArrow.UI.Main;
using NUnit.Framework;

namespace NanaArrow.Tests.EditMode
{
    public class MainMenuTests
    {
        [TestCase(1, 100, 1)]
        [TestCase(5, 100, 5)]
        [TestCase(101, 100, 100)] // 전부 깼으면 마지막 레벨
        [TestCase(1, 0, 1)]       // 카탈로그 비어 있음
        public void StartLevelFor_ClampsToCatalog(int next, int count, int expected)
        {
            Assert.AreEqual(expected, MainMenu.StartLevelFor(next, count));
        }

        [TestCase(1, 3, LevelCellState.Cleared)]
        [TestCase(3, 3, LevelCellState.Cleared)]
        [TestCase(4, 3, LevelCellState.Next)]
        [TestCase(5, 3, LevelCellState.Locked)]
        [TestCase(1, 0, LevelCellState.Next)]
        public void LevelSelect_StateFor(int level, int highest, LevelCellState expected)
        {
            Assert.AreEqual(expected, LevelSelectView.StateFor(level, highest));
        }
    }
}
