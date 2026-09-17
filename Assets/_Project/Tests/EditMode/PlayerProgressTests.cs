using System.IO;
using NanaArrow.Core;
using NUnit.Framework;

namespace NanaArrow.Tests.EditMode
{
    public class PlayerProgressTests
    {
        private string _dir;
        private SaveService _save;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "nana-arrow-tests", Path.GetRandomFileName());
            _save = new SaveService(Path.Combine(_dir, "save.json"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
        }

        [Test]
        public void Fresh_NextLevelIsOne_NothingCleared()
        {
            var progress = new PlayerProgress(_save);

            Assert.AreEqual(0, progress.HighestClearedLevel);
            Assert.AreEqual(1, progress.NextLevel);
            Assert.IsFalse(progress.IsCleared(1));
            Assert.IsFalse(progress.RewardCodeIssued);
        }

        [Test]
        public void MarkCleared_RaisesHighest_AndPersists()
        {
            var progress = new PlayerProgress(_save);
            var changed = 0;
            progress.Changed += () => changed++;

            progress.MarkCleared(3);

            Assert.AreEqual(3, progress.HighestClearedLevel);
            Assert.AreEqual(4, progress.NextLevel);
            Assert.IsTrue(progress.IsCleared(2));
            Assert.AreEqual(1, changed);
            Assert.AreEqual(3, new PlayerProgress(_save).HighestClearedLevel, "새 인스턴스가 파일에서 읽어야 함");
        }

        [Test]
        public void MarkCleared_LowerLevel_DoesNotLowerHighest()
        {
            var progress = new PlayerProgress(_save);
            progress.MarkCleared(5);
            var changed = 0;
            progress.Changed += () => changed++;

            progress.MarkCleared(2);

            Assert.AreEqual(5, progress.HighestClearedLevel);
            Assert.AreEqual(0, changed);
        }

        [Test]
        public void MarkRewardCodeIssued_PersistsOnce()
        {
            var progress = new PlayerProgress(_save);
            progress.MarkCleared(100);

            progress.MarkRewardCodeIssued();

            Assert.IsTrue(new PlayerProgress(_save).RewardCodeIssued);
        }

        [Test]
        public void Reset_ReturnsToFresh()
        {
            var progress = new PlayerProgress(_save);
            progress.MarkCleared(9);

            progress.Reset();

            Assert.AreEqual(0, progress.HighestClearedLevel);
            Assert.AreEqual(0, new PlayerProgress(_save).HighestClearedLevel);
        }
    }
}
