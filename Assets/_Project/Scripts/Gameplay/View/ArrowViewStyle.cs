using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>
    /// 경로형 Arrow 표시 스타일 (GAME_RULES v0.6.1 §0 그래픽). 인스턴스는 Assets/_Project/Settings/ArrowViewStyle.asset.
    /// 스프라이트·머티리얼을 비우면 임시 도형 / Sprites-Default 를 쓴다.
    /// </summary>
    [CreateAssetMenu(fileName = "ArrowViewStyle", menuName = "NanaArrow/Arrow View Style")]
    public sealed class ArrowViewStyle : ScriptableObject
    {
        [Header("선 (레퍼런스: 짙은 남색, 둥근 꺾임)")]
        [SerializeField, Tooltip("몸통·화살촉 기본색")] private Color lineColor = new Color(0.078f, 0.102f, 0.2f);
        [SerializeField, Range(0.02f, 0.6f), Tooltip("선 굵기 = 셀 × 이 값")] private float lineWidthCellRatio = 0.15f;
        [SerializeField, Range(0, 16), Tooltip("꺾임을 둥글게 하는 정점 수 (0 = 각진 꺾임)")] private int cornerVertices = 8;
        [SerializeField, Range(0, 16), Tooltip("꼬리 끝을 둥글게 하는 정점 수 (0 = 평평)")] private int capVertices = 8;
        [SerializeField, Tooltip("비우면 Sprites/Default")] private Material lineMaterial;

        [Header("화살촉 (셀 크기 비율)")]
        [SerializeField, Range(0.1f, 1f)] private float arrowHeadLengthCellRatio = 0.45f;
        [SerializeField, Range(0.1f, 1f)] private float arrowHeadWidthCellRatio = 0.45f;
        [SerializeField, Tooltip("위(+Y)를 가리키는 화살촉. 비우면 임시 삼각형")] private Sprite headSprite;

        [Header("상태 색")]
        [SerializeField, Tooltip("Block 으로 Marked 된 Arrow 전체 색 (GAME_RULES §2-6)")] private Color markedColor = new Color(0.93f, 0.26f, 0.26f);
        [SerializeField, Tooltip("Block 시 레인 번쩍 색")] private Color laneFlashColor = new Color(0.93f, 0.26f, 0.26f, 0.55f);
        [SerializeField, Tooltip("길게 누르기 레인 미리보기 색")] private Color lanePreviewColor = new Color(0.36f, 0.42f, 0.9f, 0.45f);
        [SerializeField, Tooltip("미리보기 중 해당 Arrow 선 색")] private Color previewLineColor = new Color(0.36f, 0.42f, 0.9f);
        [SerializeField, Range(0.05f, 1f), Tooltip("레인 표시 굵기 = 셀 × 이 값")] private float laneWidthCellRatio = 0.5f;

        [Header("머리 위 타입 표시 (셀 크기 비율)")]
        [SerializeField, Range(0.1f, 1f)] private float iconSizeCellRatio = 0.55f;
        [SerializeField, Tooltip("Frozen 얼음 (알파가 남은 얼음)")] private Color iceColor = new Color(0.62f, 0.85f, 1f, 0.85f);
        [SerializeField, Tooltip("비우면 임시 원")] private Sprite iceSprite;
        [SerializeField, Tooltip("비우면 임시 자물쇠")] private Sprite lockSprite;
        [SerializeField, Tooltip("비우면 임시 원")] private Sprite keySprite;
        [SerializeField, Tooltip("keyGroup 색이 부족할 때 Key/Locked 기본색")] private Color keyFallbackColor = new Color(0.9f, 0.64f, 0.24f);
        [SerializeField, Tooltip("레벨에 등장한 keyGroup 순서대로. 같은 그룹의 Key 와 Locked 는 같은 색")] private Color[] keyGroupColors =
        {
            new Color(0.9f, 0.64f, 0.24f),
            new Color(0.61f, 0.15f, 0.69f),
            new Color(0.15f, 0.65f, 0.6f),
        };

        [Header("정렬 순서")]
        [SerializeField] private int laneOrder = 5;
        [SerializeField] private int lineOrder = 10;
        [SerializeField] private int headOrder = 11;
        [SerializeField] private int iconOrder = 12;

        private static Material _defaultLineMaterial;

        public Color LineColor => lineColor;
        public float LineWidthCellRatio => lineWidthCellRatio;
        public int CornerVertices => cornerVertices;
        public int CapVertices => capVertices;
        public Material LineMaterial => lineMaterial != null ? lineMaterial : DefaultLineMaterial;
        public float ArrowHeadLengthCellRatio => arrowHeadLengthCellRatio;
        public float ArrowHeadWidthCellRatio => arrowHeadWidthCellRatio;
        public Sprite HeadSprite => headSprite != null ? headSprite : PlaceholderSprites.Triangle;
        public Color MarkedColor => markedColor;
        public Color LaneFlashColor => laneFlashColor;
        public Color LanePreviewColor => lanePreviewColor;
        public Color PreviewLineColor => previewLineColor;
        public float LaneWidthCellRatio => laneWidthCellRatio;
        public float IconSizeCellRatio => iconSizeCellRatio;
        public Color IceColor => iceColor;
        public Sprite IceSprite => iceSprite != null ? iceSprite : PlaceholderSprites.Circle;
        public Sprite LockSprite => lockSprite != null ? lockSprite : PlaceholderSprites.Padlock;
        public Sprite KeySprite => keySprite != null ? keySprite : PlaceholderSprites.Circle;
        public int LaneOrder => laneOrder;
        public int LineOrder => lineOrder;
        public int HeadOrder => headOrder;
        public int IconOrder => iconOrder;

        /// <param name="keyGroupIndex">레벨 내 keyGroup 등장 순서. 음수면 그룹 없음</param>
        public Color GetKeyGroupColor(int keyGroupIndex)
        {
            if (keyGroupIndex < 0 || keyGroupColors == null || keyGroupColors.Length == 0)
                return keyFallbackColor;
            return keyGroupColors[keyGroupIndex % keyGroupColors.Length];
        }

        private static Material DefaultLineMaterial
        {
            get
            {
                if (_defaultLineMaterial == null)
                    _defaultLineMaterial = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.DontSave };
                return _defaultLineMaterial;
            }
        }
    }
}
