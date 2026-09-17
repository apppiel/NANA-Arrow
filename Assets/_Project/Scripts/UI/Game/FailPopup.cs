using NanaArrow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NanaArrow.UI.Game
{
    /// <summary>
    /// 실패 팝업 (UI_FLOW §6-2): `광고 보고 이어하기`(보상형, 레벨당 maxContinues) / `다시하기`(실패 횟수에 포함, 전면 판정).
    /// 닫기 없음 — 프리팹의 PopupBase 옵션 Closable By Back 을 끈다. 광고를 끝까지 안 보면 팝업 그대로.
    /// </summary>
    public sealed class FailPopup : PopupBase
    {
        [SerializeField] private AdsController ads;
        [SerializeField, Tooltip("PrimaryButton — 광고 보고 이어하기")] private Button continueButton;
        [SerializeField, Tooltip("AdUnavailableText (fail.ad_unavailable)")] private GameObject adUnavailableText;

        protected override void OnOpened()
        {
            var offer = ads.CanOfferContinue;
            var ready = ads.IsRewardedReady;
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(offer);
                continueButton.interactable = ready;
            }
            if (adUnavailableText != null) adUnavailableText.SetActive(offer && !ready);
            if (offer) ads.ReportContinueOffered();
        }

        /// <summary>PrimaryButton.onClick</summary>
        public void OnContinue()
        {
            if (ads.IsShowing) return;
            if (continueButton != null) continueButton.interactable = false;
            ads.RequestContinue(granted =>
            {
                if (granted) Close();
                else if (continueButton != null) continueButton.interactable = ads.IsRewardedReady;
            });
        }

        /// <summary>SecondaryButton.onClick</summary>
        public void OnRetry()
        {
            if (ads.IsShowing) return;
            Close();
            ads.RetryFromFailPopup();
        }
    }
}
