using NanaArrow.Core;
using NanaArrow.Gameplay;
using NanaArrow.Gameplay.View;
using TMPro;
using UnityEngine;

namespace NanaArrow.UI.Tutorial
{
    /// <summary>
    /// TutorialConfig 를 읽어 GameSession 사건을 <see cref="TutorialFlow"/> 에 넘기고 말풍선·손가락을 켜고 끈다 (UI_FLOW §7, §12-4).
    /// 입력은 막지 않는다. 손가락은 UICanvas 바로 아래(SafeArea 밖)에 두고 보드 셀 → 화면 좌표로 매 프레임 따라간다 (줌·팬 대응).
    /// </summary>
    public sealed class TutorialPresenter : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private TutorialConfig config;
        [SerializeField] private Strings strings;

        [Header("씬 참조")]
        [SerializeField] private GameController gameController;
        [SerializeField] private BoardView boardView;
        [SerializeField, Tooltip("TutorialBubble (처음엔 비활성)")] private GameObject bubble;
        [SerializeField, Tooltip("TutorialBubble/Text")] private TMP_Text bubbleText;
        [SerializeField, Tooltip("UICanvas/Finger (처음엔 비활성, Raycast Target 끔)")] private RectTransform finger;

        [Header("연출")]
        [SerializeField, Min(0f), Tooltip("손가락 위아래 흔들림 폭 (캔버스 px)")] private float fingerBobAmplitude = 12f;
        [SerializeField, Min(0.01f), Tooltip("손가락 흔들림 주기 (초)")] private float fingerBobPeriod = 0.8f;

        private TutorialFlow _flow;
        private GameSession _session;
        private int _level;
        private Arrow _fingerTarget;
        private Vector2 _fingerOffsetCells;
        private FingerAnchor _fingerAnchor;
        private Canvas _fingerCanvas;

        private void Awake()
        {
            if (finger != null) _fingerCanvas = finger.GetComponentInParent<Canvas>();
            gameController.SessionStarted += OnSessionStarted;
            gameController.LanePreviewShown += OnLanePreviewShown;
            HideAll();
        }

        private void OnDestroy()
        {
            gameController.SessionStarted -= OnSessionStarted;
            gameController.LanePreviewShown -= OnLanePreviewShown;
            Unbind();
        }

        private void Update()
        {
            if (_flow != null && _flow.Tick(Time.deltaTime)) HideAll();
        }

        private void LateUpdate()
        {
            if (_fingerTarget == null || finger == null || boardView.Layout == null) return;
            finger.anchoredPosition = FingerCanvasPosition() + Vector2.up * (fingerBobAmplitude * Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI / fingerBobPeriod)));
        }

        private void OnSessionStarted(GameSession session)
        {
            Unbind();
            HideAll();
            _session = session;
            _level = session.Level.Id;
            session.Tapped += OnTapped;
            session.Cleared += OnLevelEnded;
            session.Failed += OnLevelEnded;

            if (config == null) return;
            if (!config.ShowOnReplay && App.Progress.IsCleared(_level)) return;
            var steps = config.StepsFor(_level);
            if (steps.Count == 0) return;

            _flow = new TutorialFlow(steps, config.HideDelay);
            _flow.Closed += OnStepClosed;
            Present(_flow.Start());
        }

        private void OnTapped(Arrow arrow, TapResult result)
        {
            if (result.Outcome == TapOutcome.Ignored) return;
            Fire(TutorialTrigger.FirstTap);
            switch (result.Outcome)
            {
                case TapOutcome.Exit: Fire(TutorialTrigger.FirstExit); break;
                case TapOutcome.Blocked: Fire(TutorialTrigger.FirstBlock); break;
                case TapOutcome.IceBroken: Fire(TutorialTrigger.FirstIceBreak); break;
            }
        }

        private void OnLanePreviewShown(Arrow arrow) => Fire(TutorialTrigger.FirstLongPress);

        private void OnLevelEnded()
        {
            _flow?.Abort();
            HideAll();
        }

        private void Fire(TutorialTrigger trigger)
        {
            if (_flow == null) return;
            var opened = _flow.Fire(trigger);
            if (opened.HasValue) Present(opened);
            else if (!_flow.Current.HasValue) HideAll();
        }

        private void Present(TutorialStep? step)
        {
            if (!step.HasValue) return;
            var s = step.Value;

            if (bubble != null) bubble.SetActive(s.HasBubble);
            if (bubbleText != null && s.HasBubble) bubbleText.text = strings != null ? strings.Get(s.TextKey) : s.TextKey;

            _fingerTarget = s.HasFinger ? _session.Board.GetArrow(s.TargetArrowId) : null;
            _fingerAnchor = s.FingerAnchor;
            _fingerOffsetCells = s.FingerOffsetCells;
            if (finger != null) finger.gameObject.SetActive(_fingerTarget != null);
            if (s.HasFinger && _fingerTarget == null)
                Debug.LogWarning($"[Tutorial] 레벨 {_level} 에 화살표 '{s.TargetArrowId}' 가 없습니다.");

            GameEvents.RaiseTutorialStepShown(_level, s.TextKey, s.Trigger.ToString());
        }

        private void OnStepClosed(TutorialStep step, float elapsed)
        {
            GameEvents.RaiseTutorialDone(_level, step.TextKey, elapsed);
        }

        private void HideAll()
        {
            if (bubble != null) bubble.SetActive(false);
            if (finger != null) finger.gameObject.SetActive(false);
            _fingerTarget = null;
        }

        private void Unbind()
        {
            if (_session != null)
            {
                _session.Tapped -= OnTapped;
                _session.Cleared -= OnLevelEnded;
                _session.Failed -= OnLevelEnded;
                _session = null;
            }
            if (_flow != null)
            {
                _flow.Closed -= OnStepClosed;
                _flow = null;
            }
        }

        private Vector2 AnchorCell(Arrow arrow)
        {
            switch (_fingerAnchor)
            {
                case FingerAnchor.Head: return arrow.Head;
                case FingerAnchor.Tail: return arrow.Tail;
                default: return arrow.Cells[arrow.Length / 2];
            }
        }

        private Vector2 FingerCanvasPosition()
        {
            var layout = boardView.Layout;
            var cell = AnchorCell(_fingerTarget);
            var world = layout.CellToWorld(Vector2Int.RoundToInt(cell)) + _fingerOffsetCells * layout.Pitch;
            var screen = boardView.TargetCamera.WorldToScreenPoint(world);
            var uiCamera = _fingerCanvas != null && _fingerCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? _fingerCanvas.worldCamera : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(finger.parent as RectTransform, screen, uiCamera, out var local);
            return local;
        }
    }
}
