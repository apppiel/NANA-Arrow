using NanaArrow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NanaArrow.UI.Game
{
    /// <summary>
    /// 게임 화면 우하단 `#` 레인 가이드 토글 (GAME_RULES v0.7.3 §9·§10). 켜면 남아 있는 Arrow 마다
    /// 머리가 있는 행·열에 화면 끝까지 옅은 선이 생긴다 (표 격자가 아니다).
    /// 상태는 SettingsStore(PlayerPrefs)에 저장되고 GameController 가 BoardView 에 전달한다.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class LaneGuideToggleButton : MonoBehaviour
    {
        [SerializeField, Tooltip("버튼 배경 (비우면 이 오브젝트의 Image)")] private Image background;
        [SerializeField, Tooltip("켜짐/꺼짐 색을 가져올 팔레트 (Settings/UITheme). 비우면 색을 건드리지 않는다")]
        private UITheme theme;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (background == null) background = GetComponent<Image>();
            _button.onClick.AddListener(Toggle);
            SettingsStore.LaneGuideChanged += Refresh;
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(Toggle);
            SettingsStore.LaneGuideChanged -= Refresh;
        }

        private void Start() => Refresh(SettingsStore.LaneGuideOn);

        /// <summary>버튼 onClick (인스펙터에서 따로 연결할 필요는 없다 — Awake 에서 등록한다).</summary>
        public void Toggle()
        {
            AudioManager.Instance?.Play(SoundId.SfxButton);
            SettingsStore.LaneGuideOn = !SettingsStore.LaneGuideOn;
        }

        /// <summary>색은 <see cref="UITheme"/> 에서만 가져온다 — 코드에 팔레트를 박지 않는다 (W-028).</summary>
        private void Refresh(bool on)
        {
            if (background == null || theme == null) return;
            background.color = on ? theme.ToggleOn : theme.ToggleOff;
        }
    }
}
