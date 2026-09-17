using NanaArrow.UI.Tutorial;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class TutorialConfigTests
    {
        private TutorialConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<TutorialConfig>();
            _config.SetSteps(new[]
            {
                new TutorialStep(2, TutorialTrigger.LevelStart, "tut.free_first", "a3", FingerAnchor.Head, Vector2.zero, TutorialTrigger.FirstTap),
                new TutorialStep(1, TutorialTrigger.LevelStart, "tut.tap", "a2", FingerAnchor.Middle, Vector2.zero, TutorialTrigger.FirstExit),
                new TutorialStep(2, TutorialTrigger.FirstBlock, "tut.block", "", FingerAnchor.Head, Vector2.zero, TutorialTrigger.None),
            });
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void Defaults_MatchUiFlow()
        {
            Assert.AreEqual(3f, _config.HideDelay);
            Assert.IsFalse(_config.ShowOnReplay);
        }

        [Test]
        public void StepsFor_ReturnsOnlyThatLevel_InOrder()
        {
            var steps = _config.StepsFor(2);

            Assert.AreEqual(2, steps.Count);
            Assert.AreEqual("tut.free_first", steps[0].TextKey);
            Assert.AreEqual("tut.block", steps[1].TextKey);
            Assert.IsTrue(steps[0].HasFinger);
            Assert.IsFalse(steps[1].HasFinger);
        }

        [Test]
        public void StepsFor_UnknownLevel_Empty()
        {
            Assert.IsEmpty(_config.StepsFor(3));
        }
    }
}
