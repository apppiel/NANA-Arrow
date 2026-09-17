using NanaArrow.Core;
using NUnit.Framework;

namespace NanaArrow.Tests.EditMode
{
    public class SaveCodecTests
    {
        [Test]
        public void Encode_ThenDecode_RoundTrips()
        {
            var original = new SaveData(12, true);

            var ok = SaveCodec.TryDecode(SaveCodec.Encode(original), out var decoded);

            Assert.IsTrue(ok);
            Assert.AreEqual(12, decoded.HighestClearedLevel);
            Assert.IsTrue(decoded.RewardCodeIssued);
            Assert.AreEqual(SaveData.CurrentVersion, decoded.Version);
        }

        [Test]
        public void Decode_TamperedPayload_Fails()
        {
            var text = SaveCodec.Encode(new SaveData(3, false));
            var tampered = text.Replace("\\\"highestClearedLevel\\\":3", "\\\"highestClearedLevel\\\":99");
            Assert.AreNotEqual(text, tampered, "테스트 전제: payload 안의 값을 바꿨어야 함");

            Assert.IsFalse(SaveCodec.TryDecode(tampered, out _));
        }

        [Test]
        public void Decode_TamperedMac_Fails()
        {
            var text = SaveCodec.Encode(new SaveData(3, false));
            var tampered = text.Substring(0, text.Length - 3) + "00\"}";

            Assert.IsFalse(SaveCodec.TryDecode(tampered, out _));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("not json")]
        [TestCase("{}")]
        [TestCase("{\"payload\":\"\",\"mac\":\"\"}")]
        public void Decode_Garbage_Fails(string text)
        {
            Assert.IsFalse(SaveCodec.TryDecode(text, out _));
        }

        [Test]
        public void Fresh_IsLevelZero_NoReward()
        {
            var fresh = SaveData.Fresh();

            Assert.AreEqual(0, fresh.HighestClearedLevel);
            Assert.IsFalse(fresh.RewardCodeIssued);
            Assert.AreEqual(SaveData.CurrentVersion, fresh.Version);
        }
    }
}
