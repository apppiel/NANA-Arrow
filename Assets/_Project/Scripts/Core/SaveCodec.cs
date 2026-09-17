using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>
    /// <see cref="SaveData"/> ↔ 파일 문자열 (NO.3 SaveCodec 이식). 디스크를 모르므로 EditMode 에서 왕복·변조를 파일 없이 검증한다.
    /// 무결성 = HMAC-SHA256. 파일 모양: {"payload":"{...}","mac":"3f9a…"} — payload 를 문자열로 감싸 서명 대상 바이트가 파일에 적힌 그대로다.
    /// 막는 것: 파일 직접 편집·복사본 수정 (서명 불일치 → 첫 설치). 못 막는 것: 바이너리에서 키를 뽑아 재서명 (서버 없이는 한계).
    /// </summary>
    public static class SaveCodec
    {
        // 서명 키. 튜닝값이 아니라 비밀이므로 SO 에 두지 않는다 (매직 넘버 금지의 의도된 예외). 바꾸면 기존 저장 파일이 전부 무효.
        private static readonly byte[] Key =
        {
            0x5e, 0x1a, 0x9c, 0x47, 0xd2, 0x08, 0xb3, 0x6f, 0x21, 0xe4, 0x7d, 0x90, 0x3b, 0xc6, 0x58, 0xaa,
            0x0f, 0x83, 0x2d, 0xf1, 0x6c, 0xb7, 0x49, 0x15, 0xde, 0x62, 0x98, 0x3a, 0xc0, 0x77, 0x1b, 0xe9,
        };

        [Serializable]
        private struct Envelope
        {
            [SerializeField] private string payload;
            [SerializeField] private string mac;

            public string Payload => payload;
            public string Mac => mac;

            public Envelope(string payload, string mac)
            {
                this.payload = payload;
                this.mac = mac;
            }
        }

        public static string Encode(in SaveData data)
        {
            var payload = JsonUtility.ToJson(data);
            return JsonUtility.ToJson(new Envelope(payload, ComputeMac(payload)));
        }

        /// <summary>파싱 실패 · mac 불일치 · 버전 불일치 · 음수 레벨이면 false → 호출자는 파일 없음(첫 설치)과 같게 취급한다.</summary>
        public static bool TryDecode(string text, out SaveData data)
        {
            data = default;
            if (string.IsNullOrEmpty(text)) return false;

            Envelope env;
            try { env = JsonUtility.FromJson<Envelope>(text); }
            catch (ArgumentException) { return false; }

            if (string.IsNullOrEmpty(env.Payload) || string.IsNullOrEmpty(env.Mac)) return false;
            if (!string.Equals(ComputeMac(env.Payload), env.Mac, StringComparison.Ordinal)) return false;

            SaveData decoded;
            try { decoded = JsonUtility.FromJson<SaveData>(env.Payload); }
            catch (ArgumentException) { return false; }

            if (decoded.Version != SaveData.CurrentVersion) return false;
            if (decoded.HighestClearedLevel < 0) return false;

            data = decoded;
            return true;
        }

        private static string ComputeMac(string payload)
        {
            using (var hmac = new HMACSHA256(Key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
