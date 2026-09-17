using NanaArrow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NanaArrow.UI.Main
{
    /// <summary>설정 팝업 (UI_FLOW §6-4): 사운드·진동 토글 ↔ SettingsStore(PlayerPrefs). Main 전용.</summary>
    public sealed class SettingsPopup : PopupBase
    {
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle vibrationToggle;

        protected override void Awake()
        {
            base.Awake();
            soundToggle.onValueChanged.AddListener(on => SettingsStore.SoundOn = on);
            vibrationToggle.onValueChanged.AddListener(on => SettingsStore.VibrationOn = on);
        }

        protected override void OnOpened()
        {
            soundToggle.SetIsOnWithoutNotify(SettingsStore.SoundOn);
            vibrationToggle.SetIsOnWithoutNotify(SettingsStore.VibrationOn);
        }
    }
}
