using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>Arrow 표시 스타일. 인스턴스는 Assets/_Project/Settings/ArrowViewStyle.asset. 스프라이트를 비우면 임시 도형을 쓴다.</summary>
    [CreateAssetMenu(fileName = "ArrowViewStyle", menuName = "NanaArrow/Arrow View Style")]
    public sealed class ArrowViewStyle : ScriptableObject
    {
        [Header("색 — 타입")]
        [SerializeField] private Color basicColor = new Color(0.29f, 0.56f, 0.89f);
        [SerializeField] private Color frozenColor = new Color(0.55f, 0.75f, 0.95f);
        [SerializeField, Tooltip("keyGroup 색이 부족할 때 Key/Locked 기본색")] private Color keyFallbackColor = new Color(0.9f, 0.64f, 0.24f);
        [SerializeField, Tooltip("레벨에 등장한 keyGroup 순서대로 사용. Key 와 같은 그룹의 Locked 는 같은 색")] private Color[] keyGroupColors =
        {
            new Color(0.9f, 0.64f, 0.24f),
            new Color(0.61f, 0.15f, 0.69f),
            new Color(0.15f, 0.65f, 0.6f),
        };

        [Header("색 — 상태")]
        [SerializeField, Tooltip("Block 으로 Marked 된 Arrow 몸통 색 (GAME_RULES §2-6 빨간색)")] private Color markedColor = new Color(0.9f, 0.22f, 0.21f);
        [SerializeField, Tooltip("화살촉 = 몸통 색 × 이 값")] private Color headTint = new Color(0.65f, 0.65f, 0.65f);
        [SerializeField, Tooltip("Frozen 얼음 레이어 (알파가 남은 얼음 두께)")] private Color iceColor = new Color(0.85f, 0.95f, 1f, 0.7f);
        [SerializeField] private Color lockColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        [Header("스프라이트 (비우면 임시 도형)")]
        [SerializeField] private Sprite bodySprite;
        [SerializeField, Tooltip("위(+Y)를 가리키는 화살촉")] private Sprite headSprite;
        [SerializeField] private Sprite iceSprite;
        [SerializeField] private Sprite lockSprite;

        [Header("형태 (셀 크기 비율)")]
        [SerializeField, Range(0f, 0.45f), Tooltip("몸통이 셀 가장자리에서 들어가는 여백")] private float bodyInset = 0.1f;
        [SerializeField, Range(0.1f, 1f)] private float headLength = 0.45f;
        [SerializeField, Range(0.1f, 1f)] private float headWidth = 0.6f;
        [SerializeField, Range(0.1f, 1f)] private float lockSize = 0.5f;

        [Header("정렬 순서")]
        [SerializeField] private int bodyOrder = 10;
        [SerializeField] private int headOrder = 11;
        [SerializeField] private int overlayOrder = 12;

        public Color BasicColor => basicColor;
        public Color FrozenColor => frozenColor;
        public Color MarkedColor => markedColor;
        public Color HeadTint => headTint;
        public Color IceColor => iceColor;
        public Color LockColor => lockColor;
        public Sprite BodySprite => bodySprite != null ? bodySprite : PlaceholderSprites.Square;
        public Sprite HeadSprite => headSprite != null ? headSprite : PlaceholderSprites.Triangle;
        public Sprite IceSprite => iceSprite != null ? iceSprite : PlaceholderSprites.Square;
        public Sprite LockSprite => lockSprite != null ? lockSprite : PlaceholderSprites.Padlock;
        public float BodyInset => bodyInset;
        public float HeadLength => headLength;
        public float HeadWidth => headWidth;
        public float LockSize => lockSize;
        public int BodyOrder => bodyOrder;
        public int HeadOrder => headOrder;
        public int OverlayOrder => overlayOrder;

        /// <param name="keyGroupIndex">레벨 내 keyGroup 등장 순서. 음수면 그룹 없음</param>
        public Color GetKeyGroupColor(int keyGroupIndex)
        {
            if (keyGroupIndex < 0 || keyGroupColors == null || keyGroupColors.Length == 0)
                return keyFallbackColor;
            return keyGroupColors[keyGroupIndex % keyGroupColors.Length];
        }
    }
}
