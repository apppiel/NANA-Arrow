using NanaArrow.Core;
using NanaArrow.Data;
using TMPro;
using UnityEngine;

namespace NanaArrow.UI.Main
{
    /// <summary>
    /// Main 화면 (UI_FLOW §4, §12-6): 시작 레벨 표시·이동, 레벨 선택 패널, 설정 팝업, 응모 코드 버튼(100 클리어 후), 뒤로가기 → 종료 확인.
    /// 버튼 onClick 은 인스펙터에서 이 컴포넌트의 public 메서드에 연결한다.
    /// </summary>
    public sealed class MainMenu : MonoBehaviour
    {
        [SerializeField] private Strings strings;
        [SerializeField] private LevelCatalog catalog;

        [Header("씬 참조")]
        [SerializeField, Tooltip("StartButton/LevelLabel — `레벨 N`")] private TMP_Text startLevelLabel;
        [SerializeField] private LevelSelectView levelSelect;
        [SerializeField, Tooltip("Popup_Settings")] private PopupBase settingsPopup;
        [SerializeField, Tooltip("Popup_Quit")] private PopupBase quitPopup;
        [SerializeField, Tooltip("Popup_Raffle (W-011). 비워도 됨")] private PopupBase rafflePopup;
        [SerializeField, Tooltip("RaffleButton — 응모 코드 발급 뒤에만 표시")] private GameObject raffleButton;

        private const string KeyLevelSub = "main.level_sub";

        /// <summary>시작하기가 열 레벨: 안 깬 가장 낮은 레벨, 전부 깼으면 마지막 레벨 (카탈로그가 비었으면 1).</summary>
        public static int StartLevelFor(int nextLevel, int catalogCount) => Mathf.Clamp(nextLevel, 1, Mathf.Max(1, catalogCount));

        public int StartLevel => StartLevelFor(App.Progress.NextLevel, catalog != null ? catalog.Count : 0);

        private void Awake() => BackButton.Pressed += OnBackPressed;
        private void OnDestroy() => BackButton.Pressed -= OnBackPressed;

        private void Start()
        {
            PopupBase.CloseAll();   // 씬에 활성으로 저장된 팝업 정리 (W-025 4)
            AudioManager.Instance?.PlayBgm(SoundId.BgmMain);
            Refresh();
        }

        private void OnEnable() => App.Progress.Changed += Refresh;
        private void OnDisable() => App.Progress.Changed -= Refresh;

        /// <summary>StartButton.onClick</summary>
        public void StartGame()
        {
            AudioManager.Instance?.Play(SoundId.SfxButton);
            SceneLoader.LoadGame(StartLevel);
        }

        /// <summary>LevelSelectButton.onClick</summary>
        public void OpenLevelSelect()
        {
            AudioManager.Instance?.Play(SoundId.SfxButton);
            levelSelect.Open();
        }

        /// <summary>SettingsButton.onClick</summary>
        public void OpenSettings() => settingsPopup.Open();

        /// <summary>RaffleButton.onClick</summary>
        public void OpenRaffle()
        {
            if (rafflePopup != null) rafflePopup.Open();
        }

        /// <summary>Popup_Quit 의 `종료` 버튼</summary>
        public void Quit() => Application.Quit();

        private void Refresh()
        {
            if (startLevelLabel != null) startLevelLabel.text = strings.Format(KeyLevelSub, StartLevel);
            if (raffleButton != null) raffleButton.SetActive(App.Progress.RewardCodeIssued);
        }

        private void OnBackPressed()
        {
            if (PopupBase.ConsumedBack) return;
            if (levelSelect != null && levelSelect.IsOpen) return; // LevelSelectView 가 직접 닫는다
            if (quitPopup != null) quitPopup.Open();
        }
    }
}
