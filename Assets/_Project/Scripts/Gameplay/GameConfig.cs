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

        [Header("보드 (GAME_RULES §4, LEVEL_FORMAT 3~10)")]
        [SerializeField, Min(1), Tooltip("보드 한 변 최소 칸 수 (검증기)")]
        private int minBoardSize = 3;

        [SerializeField, Min(1), Tooltip("보드 한 변 최대 칸 수 (검증기)")]
        private int maxBoardSize = 10;

        [SerializeField, Min(0.01f), Tooltip("셀 한 칸 최대 크기 (월드 단위). 보드가 영역보다 크면 축소, 작아도 확대하지 않음")]
        private float cellSize = 1f;

        [SerializeField, Min(0f), Tooltip("셀 사이 간격 (월드 단위). 경로형에서는 선 사이 여백에만 영향")]
        private float cellGap = 0f;

        [Header("Arrow 경로 (GAME_RULES v0.6 §0)")]
        [SerializeField, Min(1), Tooltip("Arrow 경로 최대 칸 수 (검증기)")]
        private int maxArrowLength = 12;

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

        public int MaxLives => maxLives;
        public bool MarkedResetOnExit => markedResetOnExit;
        public int MinBoardSize => minBoardSize;
        public int MaxBoardSize => maxBoardSize;
        public float CellSize => cellSize;
        public float CellGap => cellGap;
        public int MaxArrowLength => maxArrowLength;
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
    }
}
