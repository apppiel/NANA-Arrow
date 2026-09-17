using System;

namespace NanaArrow.Core
{
    /// <summary>
    /// 광고 정책 판단 (GAME_RULES §7, UI_FLOW §8) — 순수 C#. "광고 차례인가" 만 정하고, "띄울 수 있는가"(재고·네트워크) 는 SDK 쪽.
    /// NO.3 InterstitialGate 를 AdsConfig 값으로 확장한 것. 앱 실행 세션 안에서만 산다 (저장 안 함) — 재시작하면 카운터 0.
    /// 값은 전부 생성자 인자 (<see cref="FromConfig"/>).
    /// </summary>
    public sealed class AdsManager
    {
        private readonly int _interstitialEveryNLevels;
        private readonly int _adFreeLevels;
        private readonly int _interstitialAfterFails;
        private readonly int _maxContinues;
        private readonly int _continueLives;

        private int _clearsSinceAd;

        /// <summary>마지막 전면 광고 차례(또는 시작) 이후 등록된 클리어 수 (광고 없는 레벨은 세지 않음).</summary>
        public int ClearsSinceAd => _clearsSinceAd;
        public int ContinueLives => _continueLives;
        public int MaxContinues => _maxContinues;

        public AdsManager(int interstitialEveryNLevels, int adFreeLevels, int interstitialAfterFails, int maxContinues, int continueLives)
        {
            if (interstitialEveryNLevels < 0) throw new ArgumentOutOfRangeException(nameof(interstitialEveryNLevels), "0 = 끔, 음수 불가");
            if (adFreeLevels < 0) throw new ArgumentOutOfRangeException(nameof(adFreeLevels));
            if (interstitialAfterFails < 0) throw new ArgumentOutOfRangeException(nameof(interstitialAfterFails), "0 = 끔, 음수 불가");
            if (maxContinues < 0) throw new ArgumentOutOfRangeException(nameof(maxContinues));
            if (continueLives < 1) throw new ArgumentOutOfRangeException(nameof(continueLives));

            _interstitialEveryNLevels = interstitialEveryNLevels;
            _adFreeLevels = adFreeLevels;
            _interstitialAfterFails = interstitialAfterFails;
            _maxContinues = maxContinues;
            _continueLives = continueLives;
        }

        public static AdsManager FromConfig(AdsConfig config) =>
            new AdsManager(config.InterstitialEveryNLevels, config.AdFreeLevels, config.InterstitialAfterFails, config.MaxContinues, config.ContinueLives);

        /// <summary>
        /// 레벨 클리어 등록 (클리어 팝업 버튼 시점). adFreeLevels 이하면 세지도 않고 <see cref="InterstitialVerdict.SkippedFreeLevel"/>.
        /// 그 위는 N번째 클리어마다 <see cref="InterstitialVerdict.Due"/> + 카운터 0. 재클리어(<paramref name="alreadyCleared"/>)도 똑같이 센다 (GAME_RULES §7).
        /// </summary>
        public InterstitialVerdict LevelCleared(int level, bool alreadyCleared)
        {
            if (level <= _adFreeLevels) return InterstitialVerdict.SkippedFreeLevel;
            if (_interstitialEveryNLevels <= 0) return InterstitialVerdict.NotDue;

            _clearsSinceAd++;
            if (_clearsSinceAd < _interstitialEveryNLevels) return InterstitialVerdict.NotDue;

            _clearsSinceAd = 0;
            return InterstitialVerdict.Due;
        }

        /// <summary>실패 팝업 `다시하기`. 같은 레벨 실패 횟수(이번 실패 포함, 1부터) 가 interstitialAfterFails 의 배수면 광고 차례.</summary>
        public InterstitialVerdict RetryPressed(int failCount)
        {
            if (_interstitialAfterFails <= 0 || failCount <= 0) return InterstitialVerdict.NotDue;
            return failCount % _interstitialAfterFails == 0 ? InterstitialVerdict.Due : InterstitialVerdict.NotDue;
        }

        /// <summary>이어하기 허용 여부 (레벨당 maxContinues). 광고 준비 여부는 별개.</summary>
        public bool ContinueRequested(int continuesUsed) => continuesUsed < _maxContinues;
    }
}
