using UnityEngine;

namespace NanaArrow.Gameplay
{
    /// <summary>
    /// 게임 밸런스·연출 값. 기본값은 GAME_RULES v0.4.
    /// 인스턴스는 Assets/_Project/Settings/GameConfig.asset 에 두고 코드는 참조만 한다.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "NanaArrow/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("목숨 (GAME_RULES §2-6)")]
        [SerializeField, Min(1), Tooltip("레벨 시작 시 목숨. 레벨 간 공유 없음")]
        private int maxLives = 3;

        [SerializeField, Tooltip("어떤 Arrow 든 Exit 되면 Marked(빨간색) 전부 해제")]
        private bool markedResetOnExit = true;

        [Header("보드 (GAME_RULES v0.7 §4)")]
        [SerializeField, Min(1), Tooltip("보드 가로·세로 최소 칸 수 (검증기)")]
        private int minBoardSize = 3;

        [SerializeField, Min(1), Tooltip("보드 가로 최대 칸 수 (검증기)")]
        private int maxBoardWidth = 10;

        [SerializeField, Min(1), Tooltip("보드 세로 최대 칸 수 (검증기)")]
        private int maxBoardHeight = 14;

        [SerializeField, Min(1), Tooltip("Arrow 경로 최대 칸 수 (검증기)")]
        private int maxArrowLength = 40;

        [Header("셀 크기 (GAME_RULES v0.7 §9): cell = min(화면폭 × cellWidthFraction, 화면폭 × maxAreaFraction ÷ 가로칸수)")]
        [SerializeField, Range(0.01f, 0.5f), Tooltip("보드 크기와 무관한 기본 셀 크기 = 화면 폭 × 이 값")]
        private float cellWidthFraction = 0.052f;

        [SerializeField, Range(0.1f, 1f), Tooltip("보드가 차지할 수 있는 화면 폭 최대 비율 (넓은 보드는 이 안에 맞춰 축소)")]
        private float maxAreaFraction = 0.9f;

        [SerializeField, Range(0f, 0.5f), Tooltip("셀 사이 간격 = 셀 × 이 값 (0 = 선이 이어짐)")]
        private float cellGapRatio = 0f;

        [Header("연출 타이밍 (GAME_RULES §8)")]
        [SerializeField, Min(0.1f), Tooltip("Fire 시 머리가 레인을 직진하는 속도 (셀/초). 몸통은 경로를 따라 같은 속도로 따라간다")]
        private float fireSpeedCellsPerSec = 24f;

        [SerializeField, Min(0f), Tooltip("Block 튕김 한 방향 시간 (초)")]
        private float blockBounceDuration = 0.15f;

        [SerializeField, Min(0f), Tooltip("Block 튕김 거리 (셀 단위)")]
        private float blockBounceDistance = 0.2f;

        [SerializeField, Min(0f), Tooltip("Block 시 레인이 빨갛게 번쩍이는 시간 (초)")]
        private float laneFlashDuration = 0.35f;

        [SerializeField, Min(0f), Tooltip("마지막 Exit 후 클리어 팝업까지 대기 (초)")]
        private float clearPopupDelay = 0.6f;

        [SerializeField, Min(0f), Tooltip("레벨 시작 시 셀 등장 간격 (초)")]
        private float cellSpawnStagger = 0.03f;

        [Header("연출 타이밍 — 추가분 (GAME_RULES §8 미기재, 디렉터 확인)")]
        [SerializeField, Min(0f), Tooltip("셀·Arrow 하나가 등장하는 데 걸리는 시간 (초)")]
        private float cellSpawnDuration = 0.15f;

        [SerializeField, Min(0f), Tooltip("Frozen 얼음 깨기 연출 시간 (초)")]
        private float iceBreakDuration = 0.15f;

        [SerializeField, Min(0f), Tooltip("잠긴 Locked 탭 시 흔들림 시간 (초)")]
        private float lockShakeDuration = 0.2f;

        [SerializeField, Min(0f), Tooltip("잠긴 Locked 탭 시 흔들림 진폭 (셀 단위)")]
        private float lockShakeDistance = 0.08f;

        [Header("입력 (GAME_RULES §9, v0.6 §0)")]
        [SerializeField, Tooltip("Fire 연출 중에도 다른 Arrow 탭 허용 (연속 탭)")]
        private bool allowInputDuringFire = true;

        [SerializeField, Min(0.05f), Tooltip("이 시간 이상 누르면 탭 대신 레인 미리보기 (초)")]
        private float longPressSeconds = 0.35f;

        [Header("보드 확대·축소·이동 (GAME_RULES v0.7.1 §10)")]
        [SerializeField, Min(1f), Tooltip("최소 줌 = 기본 크기 (1.0 에서는 이동 불가)")]
        private float zoomMin = 1f;

        [SerializeField, Min(1f), Tooltip("핀치 최대 확대 배율")]
        private float zoomMax = 3f;

        [SerializeField, Min(0.01f), Tooltip("이 거리(셀 단위) 이상 움직여야 드래그(이동)로 판정. 그 전까지는 탭/길게 누르기 후보")]
        private float dragThresholdCells = 0.3f;

        [SerializeField, Min(0f), Tooltip("이동 시 보드 가장자리 밖으로 허용하는 여백 (셀 단위)")]
        private float panMarginCells = 1f;

        [SerializeField, Min(0.05f), Tooltip("빈 곳 더블 탭 = 줌 리셋 판정 간격 (초)")]
        private float doubleTapSeconds = 0.3f;

        public int MaxLives => maxLives;
        public bool MarkedResetOnExit => markedResetOnExit;
        public int MinBoardSize => minBoardSize;
        public int MaxBoardWidth => maxBoardWidth;
        public int MaxBoardHeight => maxBoardHeight;
        public int MaxArrowLength => maxArrowLength;
        public float CellWidthFraction => cellWidthFraction;
        public float MaxAreaFraction => maxAreaFraction;
        public float CellGapRatio => cellGapRatio;
        public float FireSpeedCellsPerSec => fireSpeedCellsPerSec;
        public float LaneFlashDuration => laneFlashDuration;
        public float BlockBounceDuration => blockBounceDuration;
        public float BlockBounceDistance => blockBounceDistance;
        public float ClearPopupDelay => clearPopupDelay;
        public float CellSpawnStagger => cellSpawnStagger;
        public float CellSpawnDuration => cellSpawnDuration;
        public float IceBreakDuration => iceBreakDuration;
        public float LockShakeDuration => lockShakeDuration;
        public float LockShakeDistance => lockShakeDistance;
        public bool AllowInputDuringFire => allowInputDuringFire;
        public float LongPressSeconds => longPressSeconds;
        public float ZoomMin => zoomMin;
        public float ZoomMax => zoomMax;
        public float DragThresholdCells => dragThresholdCells;
        public float PanMarginCells => panMarginCells;
        public float DoubleTapSeconds => doubleTapSeconds;
    }
}
