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

        [SerializeField, Min(0f), Tooltip("셀 사이 간격 (월드 단위)")]
        private float cellGap = 0.1f;

        [Header("연출 타이밍 (GAME_RULES §8)")]
        [SerializeField, Min(0f), Tooltip("Fire 후 보드 밖으로 날아가는 시간 (초)")]
        private float fireDuration = 0.25f;

        [SerializeField, Min(0f), Tooltip("Block 튕김 왕복 시간 (초)")]
        private float blockBounceDuration = 0.15f;

        [SerializeField, Min(0f), Tooltip("Block 튕김 거리 (셀 단위)")]
        private float blockBounceDistance = 0.2f;

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

        [Header("입력 (GAME_RULES §9)")]
        [SerializeField, Tooltip("Fire 연출 중에도 다른 Arrow 탭 허용 (연속 탭)")]
        private bool allowInputDuringFire = true;

        public int MaxLives => maxLives;
        public bool MarkedResetOnExit => markedResetOnExit;
        public int MinBoardSize => minBoardSize;
        public int MaxBoardSize => maxBoardSize;
        public float CellSize => cellSize;
        public float CellGap => cellGap;
        public float FireDuration => fireDuration;
        public float BlockBounceDuration => blockBounceDuration;
        public float BlockBounceDistance => blockBounceDistance;
        public float ClearPopupDelay => clearPopupDelay;
        public float CellSpawnStagger => cellSpawnStagger;
        public float CellSpawnDuration => cellSpawnDuration;
        public float IceBreakDuration => iceBreakDuration;
        public float LockShakeDuration => lockShakeDuration;
        public float LockShakeDistance => lockShakeDistance;
        public bool AllowInputDuringFire => allowInputDuringFire;
    }
}
