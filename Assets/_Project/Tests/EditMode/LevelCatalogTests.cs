using NanaArrow.Data;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class LevelCatalogTests
    {
        private LevelCatalog _catalog;
        private TextAsset _first;
        private TextAsset _second;

        [SetUp]
        public void SetUp()
        {
            _catalog = ScriptableObject.CreateInstance<LevelCatalog>();
            _first = new TextAsset("{}");
            _second = new TextAsset("{}");
            _catalog.SetLevels(new[] { _first, _second });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_catalog);
            Object.DestroyImmediate(_first);
            Object.DestroyImmediate(_second);
        }

        [Test]
        public void Count_MatchesListLength()
        {
            Assert.AreEqual(2, _catalog.Count);
        }

        [Test]
        public void TryGet_IsOneBased()
        {
            Assert.IsTrue(_catalog.TryGet(1, out var a));
            Assert.AreSame(_first, a);
            Assert.IsTrue(_catalog.TryGet(2, out var b));
            Assert.AreSame(_second, b);
        }

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(-1)]
        public void TryGet_OutOfRange_IsFalse(int level)
        {
            Assert.IsFalse(_catalog.TryGet(level, out var asset));
            Assert.IsNull(asset);
        }

        [Test]
        public void TryGet_EmptySlot_IsFalse()
        {
            _catalog.SetLevels(new TextAsset[] { null });

            Assert.IsFalse(_catalog.TryGet(1, out _));
        }
    }
}
