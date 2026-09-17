using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>보상형 광고 소스 (Services 의 AdMobService 가 구현, App.Rewarded 로 등록).</summary>
    public interface IRewardedAd
    {
        /// <summary>지금 띄울 수 있는가. false 면 실패 팝업의 이어하기 버튼을 비활성 (UI_FLOW §6-2).</summary>
        bool IsReady { get; }

        /// <summary>표시하고 닫힐 때까지 기다린다. 반환 true = 시청 완료 보상 콜백이 실제로 발화. 중간에 닫음·실패는 false — 보상 없음.</summary>
        Awaitable<bool> ShowAsync();
    }
}
