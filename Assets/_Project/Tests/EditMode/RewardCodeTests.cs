using System;
using System.Linq;
using NanaArrow.Core;
using NUnit.Framework;

namespace NanaArrow.Tests.EditMode
{
    public class RewardCodeTests
    {
        [Test]
        public void Generate_Returns_XXXX_Dash_XXXX()
        {
            var rng = new Random(12345);

            var code = RewardCode.Generate(max => rng.Next(max));

            Assert.AreEqual(RewardCode.TotalLength, code.Length);
            Assert.AreEqual('-', code[RewardCode.GroupLength]);
            Assert.IsTrue(RewardCode.IsWellFormed(code));
        }

        [Test]
        public void Generate_UsesOnlyCharsetCharacters()
        {
            var rng = new Random(999);
            for (var i = 0; i < 200; i++)
            {
                var code = RewardCode.Generate(max => rng.Next(max));
                foreach (var c in code.Where(c => c != '-'))
                    Assert.GreaterOrEqual(RewardCode.Charset.IndexOf(c), 0, $"문자셋 밖 글자 '{c}' (code={code})");
            }
        }

        [TestCase('0')]
        [TestCase('O')]
        [TestCase('1')]
        [TestCase('I')]
        public void Charset_ExcludesConfusableCharacters(char confusable)
        {
            Assert.AreEqual(-1, RewardCode.Charset.IndexOf(confusable));
        }

        [Test]
        public void Charset_Has32Characters_NoDuplicates()
        {
            Assert.AreEqual(32, RewardCode.Charset.Length);
            Assert.AreEqual(32, RewardCode.Charset.Distinct().Count());
        }

        [Test]
        public void Generate_IsDeterministic_GivenIndexFunction()
        {
            var code = RewardCode.Generate(_ => 0);

            Assert.AreEqual("AAAA-AAAA", code);
        }

        [Test]
        public void Generate_NullFunction_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => RewardCode.Generate(null));
        }

        [TestCase(-1)]
        [TestCase(32)]
        public void Generate_IndexOutOfRange_Throws(int bad)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RewardCode.Generate(_ => bad));
        }

        [Test]
        public void ExampleCode_IsWellFormed()
        {
            Assert.IsTrue(RewardCode.IsWellFormed(RewardCode.ExampleCode));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("A3K9XZ23")]     // 하이픈 없음
        [TestCase("A3K9-XZ2")]     // 짧음
        [TestCase("A3K9-XZ233")]   // 김
        [TestCase("A3K-9XZ23")]    // 하이픈 위치
        [TestCase("A3K0-XZ23")]    // 금지 글자 0
        [TestCase("a3k9-xz23")]    // 소문자
        public void IsWellFormed_RejectsBadInputs(string code)
        {
            Assert.IsFalse(RewardCode.IsWellFormed(code));
        }
    }
}
