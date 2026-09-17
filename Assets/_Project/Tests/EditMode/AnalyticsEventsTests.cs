using NanaArrow.Core;
using NanaArrow.Services;
using NUnit.Framework;

namespace NanaArrow.Tests.EditMode
{
    public class AnalyticsEventsTests
    {
        [TestCase(1, "level_001")]
        [TestCase(12, "level_012")]
        [TestCase(100, "level_100")]
        public void LevelName_IsZeroPaddedToThree(int level, string expected)
        {
            Assert.AreEqual(expected, AnalyticsEvents.LevelName(level));
        }

        [Test]
        public void Board_IsWidthXHeight()
        {
            Assert.AreEqual("8x10", AnalyticsEvents.Board(8, 10));
        }

        [TestCase(LevelStartReason.First, "first")]
        [TestCase(LevelStartReason.RetryHud, "retry_hud")]
        [TestCase(LevelStartReason.RetryFail, "retry_fail")]
        public void SnakeCase_MatchesAnalyticsDocValues(LevelStartReason reason, string expected)
        {
            Assert.AreEqual(expected, AnalyticsEvents.SnakeCase(reason));
        }

        [Test]
        public void EventNames_AreSnakeCase_AndUnder40Chars()
        {
            var names = new[]
            {
                AnalyticsEvents.LevelStart, AnalyticsEvents.LevelClear, AnalyticsEvents.LevelFail, AnalyticsEvents.LevelQuit, AnalyticsEvents.Retry,
                AnalyticsEvents.ContinueOffer, AnalyticsEvents.ContinueRequest, AnalyticsEvents.ContinueGranted, AnalyticsEvents.AdInterstitial, AnalyticsEvents.AdRewardedResult,
                AnalyticsEvents.TutorialStep, AnalyticsEvents.TutorialDone, AnalyticsEvents.LanePreviewFirst,
                AnalyticsEvents.LevelSelectOpen, AnalyticsEvents.SettingsChange, AnalyticsEvents.RewardCodeIssued, AnalyticsEvents.RewardCodeCopy, AnalyticsEvents.RewardLinkOpen,
            };

            Assert.AreEqual(18, names.Length, "ANALYTICS.md v0.1 이벤트 18종");
            foreach (var name in names)
            {
                Assert.LessOrEqual(name.Length, 40, name);
                StringAssert.IsMatch("^[a-z][a-z0-9_]*$", name);
            }
        }
    }
}
