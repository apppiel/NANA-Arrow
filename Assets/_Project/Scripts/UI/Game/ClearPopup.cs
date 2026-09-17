using NanaArrow.Core;
using TMPro;
using UnityEngine;

namespace NanaArrow.UI.Game
{
    /// <summary>
    /// 클리어 팝업 (UI_FLOW §6-1): 부제 `레벨 N`, 다음 레벨 / 메인으로. 마지막 레벨이면 주 버튼이 `메인으로`, 부제는 clear.all_done.
    /// 두 버튼 모두 AdsController 의 전면 판정(§8) 뒤 이동. 응모 레벨 클리어면 응모 코드 팝업이 먼저 뜨고, 닫히면 이 팝업이 다시 열린다 (§6-5).
    /// </summary>
    public sealed class ClearPopup : PopupBase
    {
        [SerializeField] private Strings strings;
        [SerializeField] private GameController gameController;
        [SerializeField, Tooltip("없으면 광고 없이 바로 이동")] private AdsController ads;
        [SerializeField, Tooltip("Popup_Raffle (응모 레벨 클리어 시 이 팝업보다 먼저). 비워도 됨")] private RewardCodePanel rewardPanel;
        [SerializeField, Tooltip("Panel/Subtitle")] private TMP_Text subtitle;
        [SerializeField, Tooltip("PrimaryButton 의 라벨")] private TMP_Text primaryLabel;
        [SerializeField, Tooltip("SecondaryButton (마지막 레벨이면 숨김)")] private GameObject secondaryButton;

        private const string KeyLevel = "clear.level";
        private const string KeyAllDone = "clear.all_done";
        private const string KeyNext = "clear.next";
        private const string KeyMain = "clear.main";

        private bool _last;

        protected override void OnOpened()
        {
            _last = gameController.IsLastLevel;
            if (subtitle != null) subtitle.text = _last ? strings.Get(KeyAllDone) : strings.Format(KeyLevel, gameController.CurrentLevel);
            if (primaryLabel != null) primaryLabel.text = strings.Get(_last ? KeyMain : KeyNext);
            if (secondaryButton != null) secondaryButton.SetActive(!_last);

            if (rewardPanel != null && rewardPanel.ShouldIssueFor(gameController.CurrentLevel))
                rewardPanel.OpenThen(this);
        }

        /// <summary>PrimaryButton.onClick</summary>
        public void OnPrimary()
        {
            Close();
            Then(_last ? (System.Action)gameController.GoToMain : () => gameController.LoadNextLevel());
        }

        /// <summary>SecondaryButton.onClick</summary>
        public void OnSecondary()
        {
            Close();
            Then(gameController.GoToMain);
        }

        private void Then(System.Action move)
        {
            if (ads != null) ads.AfterLevelCleared(move);
            else move();
        }
    }
}
