using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>
    /// 논리 Board → ArrowView 생성과 연출 재생. 셀 배경은 없고 격자는 기본 꺼짐 (GAME_RULES v0.7.2 §9). 세로 중앙 = 이 오브젝트의 위치.
    /// 판정은 하지 않는다 (TapResult / FireResult 만 받는다).
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private ArrowViewStyle style;
        [SerializeField, Tooltip("비우면 Camera.main")] private Camera targetCamera;
        [SerializeField, Tooltip("있으면 줌 1.0 기준 카메라 크기로 셀을 계산 (줌 상태에서 재시작해도 셀 크기 고정)")]
        private NanaArrow.Gameplay.Input.BoardCameraController boardCamera;

        private readonly Dictionary<Arrow, ArrowView> _views = new Dictionary<Arrow, ArrowView>();
        private Transform _arrowsRoot;
        private LaneView _lane;
        private GridOverlay _grid;
        private ArrowView _previewView;
        private int _firing;
        private bool _gridVisible;

        public BoardLayout Layout { get; private set; }
        /// <summary>보드를 그리는 카메라 (UI 가 셀 → 화면 좌표로 바꿀 때).</summary>
        public Camera TargetCamera => targetCamera;

        /// <summary>Fire 연출 중인 Arrow 가 있는지 (allowInputDuringFire = false 일 때 입력 차단용).</summary>
        public bool IsFiring => _firing > 0;

        /// <summary>격자 표시 (GAME_RULES v0.7.2 §9). 설정값은 Core 쪽 SettingsStore 가 들고 있고 GameController 가 넘겨준다.</summary>
        public void SetGridVisible(bool visible)
        {
            _gridVisible = visible;
            if (_grid != null)
                _grid.SetVisible(visible);
        }

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        public void Build(Board board)
        {
            Clear();
            var cellSize = BoardLayout.CellSizeFor(CameraWidth(), board.Width, config.CellWidthFraction, config.MaxAreaFraction);
            Layout = new BoardLayout(board.Width, board.Height, cellSize, cellSize * config.CellGapRatio, transform.position);

            _grid = new GameObject("Grid").AddComponent<GridOverlay>();
            _grid.transform.SetParent(transform, false);
            _grid.Build(Layout, style);
            _grid.SetVisible(_gridVisible);

            _arrowsRoot = new GameObject("Arrows").transform;
            _arrowsRoot.SetParent(transform, false);

            _lane = new GameObject("Lane").AddComponent<LaneView>();
            _lane.transform.SetParent(transform, false);
            _lane.Initialize(style, Layout);

            var keyGroups = new Dictionary<string, int>();
            foreach (var arrow in board.Arrows)
            {
                var view = new GameObject($"Arrow {arrow.Id}").AddComponent<ArrowView>();
                view.transform.SetParent(_arrowsRoot, false);
                view.Initialize(arrow, Layout, style, KeyGroupIndex(arrow, keyGroups));
                _views[arrow] = view;
                StartCoroutine(PopIn(view.transform, (arrow.Head.x + arrow.Head.y) * config.CellSpawnStagger));
            }
        }

        public bool TryGetCell(Vector3 world, out Vector2Int cell)
        {
            cell = default;
            return Layout != null && Layout.TryWorldToCell(world, out cell);
        }

        /// <summary>그 셀에 (아직 Exit 안 한) Arrow 가 있는지. 탭 vs 빈 곳 탭 구분용.</summary>
        public bool HasArrowAt(Vector2Int cell)
        {
            foreach (var arrow in _views.Keys)
                for (var i = 0; i < arrow.Cells.Count; i++)
                    if (arrow.Cells[i] == cell) return true;
            return false;
        }

        /// <summary>TapResult 를 연출로 옮긴다. Exit 된 Arrow 는 목록에서 빠진다.</summary>
        public void Play(Arrow arrow, TapResult result)
        {
            if (!_views.TryGetValue(arrow, out var view))
                return;

            switch (result.Outcome)
            {
                case TapOutcome.Exit:
                    _views.Remove(arrow);
                    _firing++;
                    view.PlayFire(result.FreeCells, config.FireSpeedCellsPerSec, () => _firing--);
                    break;
                case TapOutcome.Blocked:
                    _lane.Flash(arrow, result.Lane, Layout, style.LaneFlashColor, config.LaneFlashDuration);
                    view.PlayBounce(config.BlockBounceDistance, config.BlockBounceDuration);
                    break;
                case TapOutcome.IceBroken:
                    view.PlayIceBreak(result.RemainingHits, config.IceBreakDuration);
                    break;
                case TapOutcome.Locked:
                    view.PlayShake(config.LockShakeDistance, config.LockShakeDuration);
                    break;
            }
        }

        /// <summary>길게 누르기: 레인 + 해당 Arrow 강조 (GAME_RULES v0.6 §0 레인 미리보기).</summary>
        public void ShowLanePreview(Arrow arrow, FireResult preview)
        {
            HideLanePreview();
            if (!_views.TryGetValue(arrow, out var view))
                return;
            _previewView = view;
            view.SetPreview(true);
            _lane.Show(arrow, preview.Lane, Layout, style.LanePreviewColor);
        }

        public void HideLanePreview()
        {
            if (_previewView != null)
            {
                _previewView.SetPreview(false);
                _previewView = null;
            }
            if (_lane != null)
                _lane.Hide();
        }

        /// <summary>남아 있는 Arrow 의 Marked / Locked 표시를 논리 상태에 맞춘다 (Exit 뒤에 호출).</summary>
        public void Refresh(Board board, LivesTracker lives)
        {
            foreach (var pair in _views)
            {
                pair.Value.SetMarked(lives.IsMarked(pair.Key));
                pair.Value.SetLocked(board.IsLocked(pair.Key));
            }
        }

        private void Clear()
        {
            StopAllCoroutines();
            if (_arrowsRoot != null) Destroy(_arrowsRoot.gameObject);
            if (_lane != null) Destroy(_lane.gameObject);
            if (_grid != null) Destroy(_grid.gameObject);
            _views.Clear();
            _previewView = null;
            _lane = null;
            _grid = null;
            _firing = 0;
            Layout = null;
        }

        /// <summary>카메라가 보는 화면 폭 (월드 단위). 셀 크기 규칙의 "화면폭".</summary>
        private float CameraWidth()
        {
            // 외부(캡처 툴 등)가 camera.aspect 를 덮어쓴 채 남겨 두면 셀이 찌그러진다 — 게임 뷰 기준으로 되돌린 뒤 읽는다.
            targetCamera.ResetAspect();
            var orthographicSize = boardCamera != null ? boardCamera.BaseOrthographicSize : targetCamera.orthographicSize;
            return orthographicSize * 2f * targetCamera.aspect;
        }

        private static int KeyGroupIndex(Arrow arrow, Dictionary<string, int> keyGroups)
        {
            if (string.IsNullOrEmpty(arrow.KeyGroup))
                return -1;
            if (!keyGroups.TryGetValue(arrow.KeyGroup, out var index))
                keyGroups[arrow.KeyGroup] = index = keyGroups.Count;
            return index;
        }

        private IEnumerator PopIn(Transform target, float delay)
        {
            target.localScale = Vector3.zero;
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            var duration = config.CellSpawnDuration;
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                if (target == null) yield break;
                target.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, t / duration);
                yield return null;
            }
            if (target != null)
                target.localScale = Vector3.one;
        }
    }
}
