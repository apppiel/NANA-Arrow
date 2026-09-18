using UnityEngine;

namespace NanaArrow.UI
{
    /// <summary>
    /// UI 팔레트 (GAME_RULES §9, UI_FLOW §12-1). 색은 코드에 박지 않고 여기서만 정한다 (W-028 디렉터 지시).
    /// <para>
    /// 보드(선·화살촉·레인 가이드·빈 칸 점)는 <see cref="Gameplay.View.ArrowViewStyle"/> 이 따로 들고 있다.
    /// 이 에셋은 <b>코드가 런타임에 칠하는 UI 색</b>만 담는다. 버튼 배경·글자처럼 씬/프리팹에 박히는 색은
    /// 아트 교체 때 팀장이 인스펙터에서 바꾼다.
    /// </para>
    /// </summary>
    [CreateAssetMenu(menuName = "NanaArrow/UI Theme", fileName = "UITheme")]
    public sealed class UITheme : ScriptableObject
    {
        [Header("기본 팔레트 (GAME_RULES §9)")]
        [SerializeField, Tooltip("남색 #141A33 — 글자·주 버튼")]
        private Color navy = new Color(0.078f, 0.102f, 0.2f);

        [SerializeField, Tooltip("연보라 #E9E4FF — 보조 버튼·비활성")]
        private Color lilac = new Color(0.914f, 0.894f, 1f);

        [SerializeField, Tooltip("하트 빨강 #E8453C")]
        private Color heartRed = new Color(0.91f, 0.27f, 0.235f);

        [SerializeField, Tooltip("팝업 뒤 딤 — 검정 알파 50%")]
        private Color dim = new Color(0f, 0f, 0f, 0.5f);

        [Header("토글 (켜짐/꺼짐을 색으로만 구분하는 버튼)")]
        [SerializeField, Tooltip("켜짐. 그림이 그려진 스프라이트에는 흰색이 원색 그대로다")]
        private Color toggleOn = Color.white;

        [SerializeField, Tooltip("꺼짐 — 흐리게")]
        private Color toggleOff = new Color(0.75f, 0.75f, 0.78f);

        public Color Navy => navy;
        public Color Lilac => lilac;
        public Color HeartRed => heartRed;
        public Color Dim => dim;
        public Color ToggleOn => toggleOn;
        public Color ToggleOff => toggleOff;
    }
}
