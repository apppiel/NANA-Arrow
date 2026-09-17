using NanaArrow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class LivesTrackerTests
    {
        private const int MaxLives = 3;

        private static Arrow NewArrow(string id) =>
            new Arrow(id, ArrowType.Basic, Direction.Up, Vector2Int.zero);

        [Test]
        public void Constructor_StartsAtMaxLives()
        {
            var tracker = new LivesTracker(MaxLives, true);

            Assert.AreEqual(MaxLives, tracker.MaxLives);
            Assert.AreEqual(MaxLives, tracker.Lives);
            Assert.IsFalse(tracker.IsOutOfLives);
        }

        [Test]
        public void OnBlocked_FirstTime_LosesLifeAndMarks()
        {
            var tracker = new LivesTracker(MaxLives, true);
            var arrow = NewArrow("a");

            var lifeLost = tracker.OnBlocked(arrow);

            Assert.IsTrue(lifeLost);
            Assert.AreEqual(MaxLives - 1, tracker.Lives);
            Assert.IsTrue(tracker.IsMarked(arrow));
        }

        [Test]
        public void OnBlocked_AlreadyMarked_KeepsLives()
        {
            var tracker = new LivesTracker(MaxLives, true);
            var arrow = NewArrow("a");
            tracker.OnBlocked(arrow);

            var lifeLost = tracker.OnBlocked(arrow);

            Assert.IsFalse(lifeLost);
            Assert.AreEqual(MaxLives - 1, tracker.Lives);
            Assert.IsTrue(tracker.IsMarked(arrow));
        }

        [Test]
        public void OnBlocked_DifferentArrows_EachCostsALife()
        {
            var tracker = new LivesTracker(MaxLives, true);

            tracker.OnBlocked(NewArrow("a"));
            tracker.OnBlocked(NewArrow("b"));

            Assert.AreEqual(MaxLives - 2, tracker.Lives);
        }

        [Test]
        public void OnExit_WithResetOnExit_ClearsAllMarks_SoNextBlockCostsLifeAgain()
        {
            var tracker = new LivesTracker(MaxLives, true);
            var a = NewArrow("a");
            var b = NewArrow("b");
            tracker.OnBlocked(a);
            tracker.OnBlocked(b);

            tracker.OnExit(NewArrow("other"));

            Assert.IsFalse(tracker.IsMarked(a));
            Assert.IsFalse(tracker.IsMarked(b));
            Assert.IsTrue(tracker.OnBlocked(a));
            Assert.AreEqual(0, tracker.Lives);
        }

        [Test]
        public void OnExit_WithoutResetOnExit_KeepsOtherMarks_ButUnmarksExitedArrow()
        {
            var tracker = new LivesTracker(MaxLives, false);
            var a = NewArrow("a");
            var b = NewArrow("b");
            tracker.OnBlocked(a);
            tracker.OnBlocked(b);

            tracker.OnExit(a);

            Assert.IsFalse(tracker.IsMarked(a));
            Assert.IsTrue(tracker.IsMarked(b));
            Assert.IsFalse(tracker.OnBlocked(b));
            Assert.AreEqual(MaxLives - 2, tracker.Lives);
        }

        [Test]
        public void Lives_NeverGoBelowZero_AndIsOutOfLives()
        {
            var tracker = new LivesTracker(1, true);

            tracker.OnBlocked(NewArrow("a"));
            tracker.OnBlocked(NewArrow("b"));

            Assert.AreEqual(0, tracker.Lives);
            Assert.IsTrue(tracker.IsOutOfLives);
        }

        [Test]
        public void AddLives_AtZero_RestoresPlay()
        {
            var tracker = new LivesTracker(1, true);
            tracker.OnBlocked(NewArrow("a"));

            tracker.AddLives(1);

            Assert.AreEqual(1, tracker.Lives);
            Assert.IsFalse(tracker.IsOutOfLives);
        }

        [Test]
        public void AddLives_ClampsToMax()
        {
            var tracker = new LivesTracker(MaxLives, true);

            tracker.AddLives(5);

            Assert.AreEqual(MaxLives, tracker.Lives);
        }
    }
}
