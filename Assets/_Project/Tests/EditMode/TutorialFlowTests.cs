using NanaArrow.UI.Tutorial;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class TutorialFlowTests
    {
        private const float HideDelay = 3f;

        private static TutorialStep Step(TutorialTrigger trigger, string key, TutorialTrigger hideOn) =>
            new TutorialStep(1, trigger, key, "a1", FingerAnchor.Head, Vector2.zero, hideOn);

        [Test]
        public void Start_OpensLevelStartStep()
        {
            var flow = new TutorialFlow(new[] { Step(TutorialTrigger.LevelStart, "tut.tap", TutorialTrigger.FirstExit) }, HideDelay);

            var opened = flow.Start();

            Assert.IsTrue(opened.HasValue);
            Assert.AreEqual("tut.tap", opened.Value.TextKey);
            Assert.AreEqual("tut.tap", flow.Current.Value.TextKey);
        }

        [Test]
        public void Fire_HideOnTrigger_ClosesCurrentWithElapsed()
        {
            var flow = new TutorialFlow(new[] { Step(TutorialTrigger.LevelStart, "tut.tap", TutorialTrigger.FirstExit) }, HideDelay);
            flow.Start();
            TutorialStep? closed = null;
            var elapsed = -1f;
            flow.Closed += (step, t) => { closed = step; elapsed = t; };
            flow.Tick(1.5f);

            flow.Fire(TutorialTrigger.FirstExit);

            Assert.IsFalse(flow.Current.HasValue);
            Assert.AreEqual("tut.tap", closed.Value.TextKey);
            Assert.AreEqual(1.5f, elapsed, 1e-4f);
            Assert.IsTrue(flow.IsDone);
        }

        [Test]
        public void Fire_UnrelatedTrigger_KeepsCurrent()
        {
            var flow = new TutorialFlow(new[] { Step(TutorialTrigger.LevelStart, "tut.tap", TutorialTrigger.FirstExit) }, HideDelay);
            flow.Start();

            flow.Fire(TutorialTrigger.FirstBlock);

            Assert.IsTrue(flow.Current.HasValue);
        }

        [Test]
        public void Fire_SameTriggerTwice_OnlyFirstCounts()
        {
            // L2: free_first 는 FirstTap 에 닫히고, block 은 FirstBlock 에 열린다. 두 번째 FirstBlock 은 무시.
            var flow = new TutorialFlow(new[]
            {
                Step(TutorialTrigger.LevelStart, "tut.free_first", TutorialTrigger.FirstTap),
                Step(TutorialTrigger.FirstBlock, "tut.block", TutorialTrigger.None),
                Step(TutorialTrigger.FirstBlock, "tut.block2", TutorialTrigger.None),
            }, HideDelay);
            flow.Start();

            flow.Fire(TutorialTrigger.FirstTap);
            var opened = flow.Fire(TutorialTrigger.FirstBlock);
            var second = flow.Fire(TutorialTrigger.FirstBlock);

            Assert.AreEqual("tut.block", opened.Value.TextKey);
            Assert.IsFalse(second.HasValue);
            Assert.AreEqual("tut.block", flow.Current.Value.TextKey);
        }

        [Test]
        public void Fire_NewStep_ReplacesCurrent()
        {
            var flow = new TutorialFlow(new[]
            {
                Step(TutorialTrigger.LevelStart, "first", TutorialTrigger.None),
                Step(TutorialTrigger.FirstTap, "second", TutorialTrigger.None),
            }, 0f);
            flow.Start();
            var closedKey = "";
            flow.Closed += (step, _) => closedKey = step.TextKey;

            flow.Fire(TutorialTrigger.FirstTap);

            Assert.AreEqual("first", closedKey);
            Assert.AreEqual("second", flow.Current.Value.TextKey);
        }

        [Test]
        public void Tick_HideOnNone_ClosesAfterHideDelay()
        {
            var flow = new TutorialFlow(new[] { Step(TutorialTrigger.LevelStart, "tut.block", TutorialTrigger.None) }, HideDelay);
            flow.Start();

            Assert.IsFalse(flow.Tick(HideDelay - 0.1f));
            Assert.IsTrue(flow.Current.HasValue);
            Assert.IsTrue(flow.Tick(0.2f));
            Assert.IsFalse(flow.Current.HasValue);
        }

        [Test]
        public void Tick_HideOnTrigger_IgnoresHideDelay()
        {
            var flow = new TutorialFlow(new[] { Step(TutorialTrigger.LevelStart, "tut.tap", TutorialTrigger.FirstExit) }, HideDelay);
            flow.Start();

            Assert.IsFalse(flow.Tick(HideDelay * 10f));
            Assert.IsTrue(flow.Current.HasValue);
        }

        [Test]
        public void Tick_HideDelayZero_KeepsUntilAbort()
        {
            var flow = new TutorialFlow(new[] { Step(TutorialTrigger.LevelStart, "tut.block", TutorialTrigger.None) }, 0f);
            flow.Start();

            Assert.IsFalse(flow.Tick(100f));
            flow.Abort();

            Assert.IsFalse(flow.Current.HasValue);
            Assert.IsTrue(flow.IsDone);
        }

        [Test]
        public void Fire_None_IsIgnored()
        {
            var flow = new TutorialFlow(new[] { Step(TutorialTrigger.LevelStart, "tut.block", TutorialTrigger.None) }, HideDelay);
            flow.Start();

            Assert.IsFalse(flow.Fire(TutorialTrigger.None).HasValue);
            Assert.IsTrue(flow.Current.HasValue);
        }
    }
}
