using System.IO;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>앱 전역 서비스 로케이터 (CLAUDE.md Core). 씬을 넘나드는 순수 C# 객체만 둔다. MonoBehaviour 싱글턴은 각자 Instance.</summary>
    public static class App
    {
        private const string SaveFileName = "save.json";

        private static PlayerProgress _progress;

        public static PlayerProgress Progress =>
            _progress ??= new PlayerProgress(new SaveService(Path.Combine(Application.persistentDataPath, SaveFileName)));

        /// <summary>테스트·치트용 교체.</summary>
        public static void SetProgress(PlayerProgress progress) => _progress = progress;

        /// <summary>전면 광고 소스. Services 의 AdMobService 가 등록. null 이면 광고 없이 진행.</summary>
        public static IInterstitialAd Interstitial { get; private set; }
        /// <summary>보상형 광고 소스. null 이면 이어하기 불가.</summary>
        public static IRewardedAd Rewarded { get; private set; }

        public static void SetAds(IInterstitialAd interstitial, IRewardedAd rewarded)
        {
            Interstitial = interstitial;
            Rewarded = rewarded;
        }

        /// <summary>응모 코드 발급 서비스. Services 의 RewardCodeService 가 등록. null 이면 팝업은 오프라인 문구.</summary>
        public static IRewardCodeService RewardCodes { get; private set; }
        /// <summary>발급 진행 통지 (code 가 비어 있으면 이전 표시 유지). 서비스가 <see cref="NotifyRewardCode"/> 로 올린다.</summary>
        public static event System.Action<string, RewardCodeStatus> RewardCodeIssued;

        public static void SetRewardCodes(IRewardCodeService service) => RewardCodes = service;
        public static void NotifyRewardCode(string code, RewardCodeStatus status) => RewardCodeIssued?.Invoke(code, status);
    }
}
