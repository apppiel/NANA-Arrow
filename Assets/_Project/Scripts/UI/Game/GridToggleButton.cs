using NanaArrow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NanaArrow.UI.Game
{
    /// <summary>
    /// 게임 화면 우하단 `#` 격자 토글 (GAME_RULES v0.7.2 §9·§10). 상태는 SettingsStore(PlayerPrefs)에 저장되고
    /// GameController 가 BoardView 에 전달한다. 켜짐/꺼짐은 버튼 이미지 색으로만 구분한다.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class GridToggleButton : MonoBehaviour
    {
        [SerializeField, Tooltip("버튼 배경 (비우면 이 오브젝트의 Image)")] private Image background;
        [SerializeField, Tooltip("격자 켜짐 색")] private Color onColor = new Color(0.078f, 0.102f, 0.2f);
        [SerializeField, Tooltip("격자 꺼짐 색")] private Color offColor = new Color(0.914f, 0.894f, 1f);

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (background == null) background = GetComponent<Image>();
            _button.onClick.AddListener(Toggle);
            SettingsStore.GridChanged += Refresh;
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(Toggle);
            SettingsStore.GridChanged -= Refresh;
        }

        private void Start() => Refresh(SettingsStore.GridOn);

        /// <summary>버튼 onClick (인스펙터에서 따로 연결할 필요는 없다 — Awake 에서 등록한다).</summary>
        public void Toggle()
        {
            AudioManager.Instance?.Play(SoundId.SfxButton);
            SettingsStore.GridOn = !SettingsStore.GridOn;
        }

        private void Refresh(bool on)
        {
            if (background != null)
                background.color = on ? onColor : offColor;
        }
    }
}
