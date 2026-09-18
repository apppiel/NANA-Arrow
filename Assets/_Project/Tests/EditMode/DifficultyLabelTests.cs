using NanaArrow.Data;
using NanaArrow.UI.Game;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NanaArrow.Tests.EditMode
{
    /// <summary>HUD 난이도 라벨 (GAME_RULES v0.7.2 §10): meta.difficulty 1~3 → 문구 키.</summary>
    public class DifficultyLabelTests
    {
        [TestCase(1, "hud.difficulty.easy")]
        [TestCase(2, "hud.difficulty.normal")]
        [TestCase(3, "hud.difficulty.hard")]
        public void KeyFor_MapsOneToThree(int difficulty, string expected)
        {
            Assert.AreEqual(expected, DifficultyLabel.KeyFor(difficulty));
        }

        [TestCase(0)]
        [TestCase(4)]
        [TestCase(-1)]
        public void KeyFor_OutOfRange_IsNull(int difficulty)
        {
            Assert.IsNull(DifficultyLabel.KeyFor(difficulty), "범위 밖이면 라벨을 숨긴다");
        }

        [Test]
        public void Difficulty_ReadsMeta()
        {
            var level = new LevelData { Meta = JObject.Parse("{\"difficulty\": 2}") };

            Assert.AreEqual(2, level.Difficulty);
        }

        [Test]
        public void Difficulty_NoMeta_IsZero()
        {
            Assert.AreEqual(0, new LevelData().Difficulty);
        }

        [Test]
        public void Difficulty_MetaWithoutKey_IsZero()
        {
            var level = new LevelData { Meta = JObject.Parse("{\"author\": \"desktop\"}") };

            Assert.AreEqual(0, level.Difficulty);
        }

        [TestCase("{\"difficulty\": 0}")]
        [TestCase("{\"difficulty\": 7}")]
        [TestCase("{\"difficulty\": \"보통\"}")]
        [TestCase("{\"difficulty\": null}")]
        public void Difficulty_InvalidValue_IsZero(string json)
        {
            var level = new LevelData { Meta = JObject.Parse(json) };

            Assert.AreEqual(0, level.Difficulty);
        }

        [Test]
        public void Difficulty_ParsedFromLevelFile()
        {
            var level = TestLevels.FormatExample();

            Assert.AreEqual(1, level.Difficulty);
            Assert.AreEqual("hud.difficulty.easy", DifficultyLabel.KeyFor(level.Difficulty));
        }
    }
}
