using NanaArrow.Core;
using UnityEngine;

namespace NanaArrow.UI.Game
{
    /// <summary>Game 씬의 Android 뒤로가기 (UI_FLOW §2): 팝업이 없으면 HUD 뒤로가기와 같이 '메인으로 확인' 팝업을 연다.</summary>
    public sealed class GameScreen : MonoBehaviour
    {
        [SerializeField, Tooltip("Popup_ConfirmMain 의 PopupBase")] private PopupBase confirmMainPopup;

        private void Awake() => BackButton.Pressed += OnBackPressed;
        private void OnDestroy() => BackButton.Pressed -= OnBackPressed;

        /// <summary>씬에 활성으로 저장된 팝업이 게임 시작과 동시에 겹쳐 보이는 것을 막는다 (W-025 4).</summary>
        private void Start() => PopupBase.CloseAll();

        private void OnBackPressed()
        {
            if (PopupBase.ConsumedBack || confirmMainPopup == null) return;
            confirmMainPopup.Open();
        }
    }
}
