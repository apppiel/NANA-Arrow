using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>전면 광고 소스 (Services 의 AdMobService 가 구현, App.Interstitial 로 등록). Core 는 SDK 를 모른다.</summary>
    public interface IInterstitialAd
    {
        /// <summary>지금 띄울 수 있는가 (로드 완료, 다른 광고 표시 중 아님). 에디터는 false.</summary>
        bool IsReady { get; }

        /// <summary>표시하고 닫힐 때까지 기다린다. 반환 = 실제로 표시됐는가. 준비 안 됐으면 즉시 false — 호출자는 광고 없이 진행한다.</summary>
        Awaitable<bool> ShowAsync();
    }
}
