using System.IO;
using NanaArrow.Core;
using NUnit.Framework;

namespace NanaArrow.Tests.EditMode
{
    public class SaveServiceTests
    {
        private string _dir;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "nana-arrow-tests", Path.GetRandomFileName());
            _path = Path.Combine(_dir, "save.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
        }

        [Test]
        public void Load_NoFile_ReturnsNull()
        {
            Assert.IsNull(new SaveService(_path).Load());
        }

        [Test]
        public void Save_ThenLoad_ReturnsSameData()
        {
            var service = new SaveService(_path);

            Assert.IsTrue(service.Save(new SaveData(7, false)));
            var loaded = service.Load();

            Assert.IsTrue(loaded.HasValue);
            Assert.AreEqual(7, loaded.Value.HighestClearedLevel);
            Assert.IsFalse(File.Exists(_path + ".tmp"), "임시 파일은 바꿔치기 후 남지 않아야 함");
        }

        [Test]
        public void Save_Twice_OverwritesAtomically()
        {
            var service = new SaveService(_path);
            service.Save(new SaveData(1, false));

            service.Save(new SaveData(2, true));

            var loaded = service.Load().Value;
            Assert.AreEqual(2, loaded.HighestClearedLevel);
            Assert.IsTrue(loaded.RewardCodeIssued);
        }

        [Test]
        public void Load_CorruptedFile_ReturnsNull()
        {
            var service = new SaveService(_path);
            service.Save(new SaveData(5, false));
            File.WriteAllText(_path, File.ReadAllText(_path).Replace("\\\"highestClearedLevel\\\":5", "\\\"highestClearedLevel\\\":50"));

            Assert.IsNull(service.Load());
        }

        [Test]
        public void Delete_RemovesFile()
        {
            var service = new SaveService(_path);
            service.Save(new SaveData(5, false));

            service.Delete();

            Assert.IsFalse(File.Exists(_path));
            Assert.IsNull(service.Load());
        }
    }
}
