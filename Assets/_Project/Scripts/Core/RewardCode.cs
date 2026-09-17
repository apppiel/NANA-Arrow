using System;

namespace NanaArrow.Core
{
    /// <summary>
    /// 응모 코드 형식 XXXX-XXXX (GAME_RULES §7, NO.3 RewardCode 이식). 전 프로젝트와 형식이 같아야 홈페이지 검증이 같은 규칙으로 읽는다.
    /// 순수 C# — 난수는 <c>nextIndex</c> 로 밖에서 받아 조합만 한다 (테스트에서 결정적).
    /// </summary>
    public static class RewardCode
    {
        /// <summary>코드에 쓰는 32글자. 눈으로 헷갈리는 0 O 1 I 제외 — 유저가 화면을 보고 웹에 옮겨 적는다.</summary>
        public const string Charset = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        /// <summary>하이픈 앞뒤 각 그룹의 길이.</summary>
        public const int GroupLength = 4;

        /// <summary>XXXX-XXXX = 9.</summary>
        public const int TotalLength = GroupLength * 2 + 1;

        /// <summary>문서·테스트용 예시. 한 곳에만 두고 테스트가 형식을 검증한다.</summary>
        public const string ExampleCode = "A3K9-XZ23";

        /// <param name="nextIndex">max 를 받아 [0, max) 정수를 돌려주는 함수 (보통 max => rng.Next(max)).</param>
        public static string Generate(Func<int, int> nextIndex)
        {
            if (nextIndex == null) throw new ArgumentNullException(nameof(nextIndex));

            var chars = new char[TotalLength];
            for (var i = 0; i < TotalLength; i++)
            {
                if (i == GroupLength) { chars[i] = '-'; continue; }

                var index = nextIndex(Charset.Length);
                if (index < 0 || index >= Charset.Length)
                    throw new ArgumentOutOfRangeException(nameof(nextIndex), $"[0, {Charset.Length}) 밖의 값 {index}");
                chars[i] = Charset[index];
            }
            return new string(chars);
        }

        /// <summary>코드 형식인가. 저장에서 읽은 값을 화면에 걸기 전에 쓴다.</summary>
        public static bool IsWellFormed(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length != TotalLength) return false;

            for (var i = 0; i < TotalLength; i++)
            {
                if (i == GroupLength)
                {
                    if (code[i] != '-') return false;
                }
                else if (Charset.IndexOf(code[i]) < 0) return false;
            }
            return true;
        }
    }
}
