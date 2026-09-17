using System.Collections;
using NanaArrow.Data;
using NanaArrow.Gameplay;
using NanaArrow.Gameplay.Input;
using NanaArrow.Gameplay.View;
using UnityEngine;
using UnityEngine.Events;

namespace NanaArrow.Core
{
    /// <summary>
    /// Game 씬 조립 지점: LevelCatalog 에서 레벨 로드 → GameSession → BoardView/TapInput 연결, GameEvents 발행.
    /// 광고·팝업 판단은 하지 않는다 (UI 와 AdsManager 가 이벤트를 듣는다).
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private ArrowTypeConfig arrowTypeConfig;

        [Header("씬 참조")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private TapInput tapInput;

        [Header("레벨")]
        [SerializeField, Tooltip("레벨 목록 (Settings/LevelCatalog)")] private LevelCatalog catalog;
        [SerializeField, Min(1), Tooltip("Game 씬을 직접 Play 했을 때 시작할 레벨. Main 에서 오면 SceneLoader.PendingLevel 이 우선")]
        private int startLevel = 1;

        [Header("이벤트 (팝업 연결용)")]
        [SerializeField, Tooltip("클리어 후 GameConfig.clearPopupDelay 뒤")] private UnityEvent levelCleared = new UnityEvent();
        [SerializeField, Tooltip("하트 0 즉시")] private UnityEvent levelFailed = new UnityEvent();

        private LevelStats _stats;

        public GameSession Session { get; private set; }
        public LevelCatalog Catalog => catalog;
        public int CurrentLevel { get; private set; }
        /// <summary>현재 판의 애널리틱스 지표. 레벨 로드 전엔 null.</summary>
        public LevelStats Stats => _stats;
        public bool IsLastLevel => catalog != null && CurrentLevel >= catalog.Count;

        private void Awake()
        {
            tapInput.CellTapped += OnCellTapped;
            tapInput.LanePreviewRequested += OnLanePreviewRequested;
            tapInput.LanePreviewReleased += OnLanePreviewReleased;
        }

        private void OnDestroy()
        {
            tapInput.CellTapped -= OnCellTapped;
            tapInput.LanePreviewRequested -= OnLanePreviewRequested;
            tapInput.LanePreviewReleased -= OnLanePreviewReleased;
            Unsubscribe();
        }

        private void Start()
        {
            var pending = SceneLoader.ConsumePendingLevel();
            LoadLevel(pending ?? startLevel, pending.HasValue ? LevelStartReason.Select : LevelStartReason.First);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && Session != null && !Session.IsCleared)
                GameEvents.RaiseLevelQuit(_stats, true, Session.Board.Arrows.Count, Session.Lives.Lives);
        }

        /// <summary>카탈로그의 레벨 번호(1부터)로 로드. 범위 밖이면 false.</summary>
        public bool LoadLevel(int level, LevelStartReason reason = LevelStartReason.Select)
        {
            if (catalog == null || !catalog.TryGet(level, out var asset))
            {
                Debug.LogWarning($"[GameController] 레벨 {level} 이(가) 카탈로그에 없습니다.");
                return false;
            }
            CurrentLevel = level;
            LoadLevel(LevelLoader.Parse(asset.text), reason);
            return true;
        }

        /// <summary>파싱된 레벨로 직접 로드 (치트·테스트).</summary>
        public void LoadLevel(LevelData level, LevelStartReason reason = LevelStartReason.Select)
        {
            Unsubscribe();
            StopAllCoroutines();

            Session = new GameSession(level, gameConfig, arrowTypeConfig);
            Session.Tapped += OnTapped;
            Session.Cleared += OnCleared;
            Session.Failed += OnFailed;

            boardView.Build(Session.Board);
            boardView.Refresh(Session.Board, Session.Lives);

            var progress = App.Progress;
            _stats = new LevelStats(level.Id, level.Width, level.Height, level.Arrows.Length,
                progress.IsCleared(level.Id), progress.IncrementAttempts(level.Id), reason);
            GameEvents.RaiseLevelStarted(_stats);
        }

        /// <summary>HUD 다시하기 (실패로 안 셈, 광고 없음) / 실패 팝업 다시하기.</summary>
        public void Restart(LevelStartReason reason = LevelStartReason.RetryHud)
        {
            if (Session == null) return;
            GameEvents.RaiseRetryPressed(_stats, reason, Session.Board.Arrows.Count);
            LoadLevel(Session.Level, reason);
        }

        /// <summary>다음 레벨. 마지막이면 false.</summary>
        public bool LoadNextLevel() => !IsLastLevel && LoadLevel(CurrentLevel + 1, LevelStartReason.Next);

        /// <summary>메인으로 나가기 (확인 팝업 뒤). 광고 없음.</summary>
        public void GoToMain()
        {
            if (Session != null && !Session.IsCleared)
                GameEvents.RaiseLevelQuit(_stats, false, Session.Board.Arrows.Count, Session.Lives.Lives);
            SceneLoader.Load(SceneId.Main);
        }

        /// <summary>치트: 즉시 클리어.</summary>
        public void CheatClear() => Session?.ForceClear();

        /// <summary>치트: 하트 채우기.</summary>
        public void CheatRefillLives()
        {
            if (Session == null) return;
            Session.Lives.AddLives(Session.Lives.MaxLives);
            boardView.Refresh(Session.Board, Session.Lives);
        }

        private void Unsubscribe()
        {
            if (Session == null) return;
            Session.Tapped -= OnTapped;
            Session.Cleared -= OnCleared;
            Session.Failed -= OnFailed;
        }

        private void OnCellTapped(Vector2Int cell)
        {
            if (Session == null || Session.IsCleared) return;
            if (!gameConfig.AllowInputDuringFire && boardView.IsFiring) return;
            Session.TapAt(cell);
        }

        private void OnLanePreviewRequested(Vector2Int cell)
        {
            var arrow = Session?.Board.GetArrowAt(cell);
            if (arrow == null) return;
            _stats.CountLanePreview();
            boardView.ShowLanePreview(arrow, Session.Preview(arrow));
        }

        private void OnLanePreviewReleased() => boardView.HideLanePreview();

        private void OnTapped(Arrow arrow, TapResult result)
        {
            if (result.Outcome != TapOutcome.Locked) _stats.CountTap();
            if (result.Outcome == TapOutcome.Blocked) _stats.CountBlock();
            boardView.Play(arrow, result);
            boardView.Refresh(Session.Board, Session.Lives);
        }

        private void OnCleared()
        {
            App.Progress.MarkCleared(CurrentLevel);
            GameEvents.RaiseLevelCleared(_stats, Session.Lives.Lives);
            StartCoroutine(RaiseClearedAfterDelay());
        }

        private void OnFailed()
        {
            GameEvents.RaiseLevelFailed(_stats, Session.Board.Arrows.Count);
            levelFailed.Invoke();
        }

        private IEnumerator RaiseClearedAfterDelay()
        {
            yield return new WaitForSeconds(gameConfig.ClearPopupDelay);
            levelCleared.Invoke();
        }
    }
}
