using NanaArrow.UI;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class StringsTests
    {
        private Strings _strings;

        [SetUp]
        public void SetUp()
        {
            _strings = ScriptableObject.CreateInstance<Strings>();
            _strings.SetEntries(new[]
            {
                new StringEntry("main.start", "시작하기"),
                new StringEntry("clear.level", "레벨 {0}"),
            });
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_strings);

        [Test]
        public void Get_KnownKey_ReturnsText()
        {
            Assert.AreEqual("시작하기", _strings.Get("main.start"));
        }

        [Test]
        public void Get_UnknownKey_ReturnsKeyItself()
        {
            Assert.AreEqual("nope.key", _strings.Get("nope.key"));
            Assert.IsNull(_strings.Get(null));
        }

        [Test]
        public void Format_ReplacesPlaceholder()
        {
            Assert.AreEqual("레벨 7", _strings.Format("clear.level", 7));
        }

        [Test]
        public void SetEntries_RebuildsLookup()
        {
            _strings.SetEntries(new[] { new StringEntry("main.start", "Start") });

            Assert.AreEqual("Start", _strings.Get("main.start"));
            Assert.AreEqual("clear.level", _strings.Get("clear.level"));
        }
    }
}
