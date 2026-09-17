using NanaArrow.Core;
using TMPro;
using UnityEngine;

namespace NanaArrow.UI.Game
{
    /// <summary>
    /// 클리어 팝업 (UI_FLOW §6-1): 부제 `레벨 N`, 다음 레벨 / 메인으로. 마지막 레벨이면 주 버튼이 `메인으로`, 부제는 clear.all_done.
    /// 광고 판단(§8)은 W-011 AdsManager 가 끼어든다 — 여기서는 이동만.
    /// </summary>
    public sealed class ClearPopup : PopupBase
    {
        [SerializeField] private Strings strings;
        [SerializeField] private GameController gameController;
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
        }

        /// <summary>PrimaryButton.onClick</summary>
        public void OnPrimary()
        {
            Close();
            if (_last) gameController.GoToMain();
            else gameController.LoadNextLevel();
        }

        /// <summary>SecondaryButton.onClick</summary>
        public void OnSecondary()
        {
            Close();
            gameController.GoToMain();
        }
    }
}
