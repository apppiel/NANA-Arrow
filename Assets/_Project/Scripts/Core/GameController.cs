using System.Collections;
using NanaArrow.Data;
using NanaArrow.Gameplay;
using NanaArrow.Gameplay.Input;
using NanaArrow.Gameplay.View;
using UnityEngine;
using UnityEngine.Events;

namespace NanaArrow.Core
{
    /// <summary>Game 씬 조립 지점: 레벨 로드 → GameSession → BoardView/TapInput 연결. 광고·팝업 판단은 하지 않는다.</summary>
    public sealed class GameController : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private ArrowTypeConfig arrowTypeConfig;

        [Header("씬 참조")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private TapInput tapInput;

        [Header("레벨")]
        [SerializeField, Tooltip("시작 시 로드할 레벨 JSON (Assets/_Project/Levels/level_###.json). 비우면 로드하지 않음")]
        private TextAsset levelJson;

        [Header("이벤트 (팝업 연결용)")]
        [SerializeField, Tooltip("클리어 후 GameConfig.clearPopupDelay 뒤")] private UnityEvent levelCleared = new UnityEvent();
        [SerializeField, Tooltip("목숨 0 즉시")] private UnityEvent levelFailed = new UnityEvent();

        public GameSession Session { get; private set; }

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
            if (levelJson != null)
                LoadLevel(levelJson);
        }

        public void LoadLevel(TextAsset json) => LoadLevel(LevelLoader.Parse(json.text));

        public void LoadLevel(LevelData level)
        {
            Unsubscribe();
            StopAllCoroutines();

            Session = new GameSession(level, gameConfig, arrowTypeConfig);
            Session.Tapped += OnTapped;
            Session.Cleared += OnCleared;
            Session.Failed += OnFailed;

            boardView.Build(Session.Board);
            boardView.Refresh(Session.Board, Session.Lives);
        }

        private void Unsubscribe()
        {
            if (Session == null)
                return;
            Session.Tapped -= OnTapped;
            Session.Cleared -= OnCleared;
            Session.Failed -= OnFailed;
        }

        private void OnCellTapped(Vector2Int cell)
        {
            if (Session == null)
                return;
            if (!gameConfig.AllowInputDuringFire && boardView.IsFiring)
                return;
            Session.TapAt(cell);
        }

        private void OnLanePreviewRequested(Vector2Int cell)
        {
            var arrow = Session?.Board.GetArrowAt(cell);
            if (arrow == null)
                return;
            boardView.ShowLanePreview(arrow, Session.Preview(arrow));
        }

        private void OnLanePreviewReleased() => boardView.HideLanePreview();

        private void OnTapped(Arrow arrow, TapResult result)
        {
            boardView.Play(arrow, result);
            boardView.Refresh(Session.Board, Session.Lives);
        }

        private void OnCleared() => StartCoroutine(RaiseClearedAfterDelay());

        private void OnFailed() => levelFailed.Invoke();

        private IEnumerator RaiseClearedAfterDelay()
        {
            yield return new WaitForSeconds(gameConfig.ClearPopupDelay);
            levelCleared.Invoke();
        }
    }
}
