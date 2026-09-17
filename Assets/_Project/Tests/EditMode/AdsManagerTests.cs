using System;
using NanaArrow.Core;
using NUnit.Framework;

namespace NanaArrow.Tests.EditMode
{
    /// <summary>GAME_RULES §7 기본값: 클리어 3회마다 전면, 1~5 레벨 광고 없음, 같은 레벨 2번 실패마다 전면, 이어하기 1회.</summary>
    public class AdsManagerTests
    {
        private const int EveryN = 3;
        private const int AdFree = 5;
        private const int AfterFails = 2;
        private const int MaxContinues = 1;
        private const int ContinueLives = 1;

        private static AdsManager Default() => new AdsManager(EveryN, AdFree, AfterFails, MaxContinues, ContinueLives);

        [Test]
        public void Ctor_RejectsNegativeOrZeroLives()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdsManager(-1, AdFree, AfterFails, MaxContinues, ContinueLives));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdsManager(EveryN, -1, AfterFails, MaxContinues, ContinueLives));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdsManager(EveryN, AdFree, -1, MaxContinues, ContinueLives));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdsManager(EveryN, AdFree, AfterFails, -1, ContinueLives));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdsManager(EveryN, AdFree, AfterFails, MaxContinues, 0));
        }

        [Test]
        public void LevelCleared_FreeLevels_SkippedAndNotCounted()
        {
            var ads = Default();

            for (var level = 1; level <= AdFree; level++)
                Assert.AreEqual(InterstitialVerdict.SkippedFreeLevel, ads.LevelCleared(level, false), $"레벨 {level}");
            Assert.AreEqual(0, ads.ClearsSinceAd);
        }

        [Test]
        public void LevelCleared_EveryThird_DueAndResets()
        {
            var ads = Default();

            Assert.AreEqual(InterstitialVerdict.NotDue, ads.LevelCleared(6, false));
            Assert.AreEqual(InterstitialVerdict.NotDue, ads.LevelCleared(7, false));
            Assert.AreEqual(InterstitialVerdict.Due, ads.LevelCleared(8, false));
            Assert.AreEqual(0, ads.ClearsSinceAd);
            Assert.AreEqual(InterstitialVerdict.NotDue, ads.LevelCleared(9, false));
            Assert.AreEqual(InterstitialVerdict.NotDue, ads.LevelCleared(10, false));
            Assert.AreEqual(InterstitialVerdict.Due, ads.LevelCleared(11, false));
        }

        [Test]
        public void LevelCleared_ReplayCountsToo()
        {
            var ads = Default();

            ads.LevelCleared(6, true);
            ads.LevelCleared(6, true);

            Assert.AreEqual(InterstitialVerdict.Due, ads.LevelCleared(6, true));
        }

        [Test]
        public void LevelCleared_IntervalZero_NeverDue()
        {
            var ads = new AdsManager(0, AdFree, AfterFails, MaxContinues, ContinueLives);

            for (var i = 0; i < 10; i++)
                Assert.AreEqual(InterstitialVerdict.NotDue, ads.LevelCleared(20, false));
            Assert.AreEqual(0, ads.ClearsSinceAd);
        }

        [TestCase(0, InterstitialVerdict.NotDue)]
        [TestCase(1, InterstitialVerdict.NotDue)]
        [TestCase(2, InterstitialVerdict.Due)]
        [TestCase(3, InterstitialVerdict.NotDue)]
        [TestCase(4, InterstitialVerdict.Due)]
        public void RetryPressed_EverySecondFail(int failCount, InterstitialVerdict expected)
        {
            Assert.AreEqual(expected, Default().RetryPressed(failCount));
        }

        [Test]
        public void RetryPressed_AfterFailsZero_NeverDue()
        {
            var ads = new AdsManager(EveryN, AdFree, 0, MaxContinues, ContinueLives);

            Assert.AreEqual(InterstitialVerdict.NotDue, ads.RetryPressed(2));
            Assert.AreEqual(InterstitialVerdict.NotDue, ads.RetryPressed(4));
        }

        [Test]
        public void ContinueRequested_AllowedOncePerLevel()
        {
            var ads = Default();

            Assert.IsTrue(ads.ContinueRequested(0));
            Assert.IsFalse(ads.ContinueRequested(1));
        }

        [Test]
        public void ContinueRequested_MaxZero_NeverAllowed()
        {
            var ads = new AdsManager(EveryN, AdFree, AfterFails, 0, ContinueLives);

            Assert.IsFalse(ads.ContinueRequested(0));
        }

        [Test]
        public void FromConfig_UsesAssetValues()
        {
            var config = UnityEngine.ScriptableObject.CreateInstance<AdsConfig>();
            try
            {
                var ads = AdsManager.FromConfig(config);

                Assert.AreEqual(1, ads.ContinueLives);
                Assert.AreEqual(1, ads.MaxContinues);
                Assert.AreEqual(InterstitialVerdict.SkippedFreeLevel, ads.LevelCleared(5, false));
                Assert.AreEqual(InterstitialVerdict.Due, ads.RetryPressed(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
    }
}
