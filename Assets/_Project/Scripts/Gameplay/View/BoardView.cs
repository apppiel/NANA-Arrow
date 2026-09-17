using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>논리 Board → 셀·ArrowView 생성과 연출 재생. 판정은 하지 않는다 (TapResult 만 받는다).</summary>
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private ArrowViewStyle style;
        [SerializeField, Tooltip("비우면 Camera.main")] private Camera targetCamera;

        [Header("배치")]
        [SerializeField, Range(0.1f, 1f), Tooltip("카메라 가로 폭 중 보드가 쓸 비율")]
        private float areaWidthFraction = 0.9f;
        [SerializeField, Range(0.1f, 1f), Tooltip("카메라 세로 높이 중 보드가 쓸 비율 (HUD 공간 제외)")]
        private float areaHeightFraction = 0.6f;

        [Header("셀")]
        [SerializeField] private Color cellColor = new Color(0.9f, 0.9f, 0.93f);
        [SerializeField, Tooltip("비우면 임시 사각형")] private Sprite cellSprite;
        [SerializeField] private int cellSortingOrder = 0;

        private readonly Dictionary<Arrow, ArrowView> _views = new Dictionary<Arrow, ArrowView>();
        private Transform _cellsRoot;
        private Transform _arrowsRoot;
        private int _firing;

        public BoardLayout Layout { get; private set; }

        /// <summary>Fire 연출 중인 Arrow 가 있는지 (allowInputDuringFire = false 일 때 입력 차단용).</summary>
        public bool IsFiring => _firing > 0;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        public void Build(Board board)
        {
            Clear();
            Layout = new BoardLayout(board.Width, board.Height, config.CellSize, config.CellGap, AvailableSize(), transform.position);

            _cellsRoot = CreateRoot("Cells");
            _arrowsRoot = CreateRoot("Arrows");

            var sprite = cellSprite != null ? cellSprite : PlaceholderSprites.Square;
            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    var renderer = new GameObject($"Cell {x},{y}").AddComponent<SpriteRenderer>();
                    renderer.transform.SetParent(_cellsRoot, false);
                    renderer.transform.position = Layout.CellToWorld(cell);
                    renderer.sprite = sprite;
                    renderer.color = cellColor;
                    renderer.sortingOrder = cellSortingOrder;
                    StartCoroutine(PopIn(renderer.transform, Vector3.one * Layout.CellSize, SpawnDelay(cell)));
                }
            }

            var keyGroups = new Dictionary<string, int>();
            foreach (var arrow in board.Arrows)
            {
                var view = new GameObject($"Arrow {arrow.Id}").AddComponent<ArrowView>();
                view.transform.SetParent(_arrowsRoot, false);
                view.Initialize(arrow, Layout, style, KeyGroupIndex(arrow, keyGroups));
                _views[arrow] = view;
                StartCoroutine(PopIn(view.transform, Vector3.one, SpawnDelay(arrow.Head)));
            }
        }

        public bool TryGetCell(Vector3 world, out Vector2Int cell)
        {
            cell = default;
            return Layout != null && Layout.TryWorldToCell(world, out cell);
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
                    view.PlayFire(result.FreeCells, config.FireDuration, () => _firing--);
                    break;
                case TapOutcome.Blocked:
                    view.PlayBounce(result.FreeCells, config.BlockBounceDistance, config.BlockBounceDuration);
                    break;
                case TapOutcome.IceBroken:
                    view.PlayIceBreak(result.RemainingHits, config.IceBreakDuration);
                    break;
                case TapOutcome.Locked:
                    view.PlayShake(config.LockShakeDistance, config.LockShakeDuration);
                    break;
            }
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
            if (_cellsRoot != null) Destroy(_cellsRoot.gameObject);
            if (_arrowsRoot != null) Destroy(_arrowsRoot.gameObject);
            _views.Clear();
            _firing = 0;
            Layout = null;
        }

        private Transform CreateRoot(string rootName)
        {
            var root = new GameObject(rootName).transform;
            root.SetParent(transform, false);
            return root;
        }

        private Vector2 AvailableSize()
        {
            var height = targetCamera.orthographicSize * 2f;
            var width = height * targetCamera.aspect;
            return new Vector2(width * areaWidthFraction, height * areaHeightFraction);
        }

        /// <summary>좌하단부터 대각선으로 퍼지는 등장 순서.</summary>
        private float SpawnDelay(Vector2Int cell) => (cell.x + cell.y) * config.CellSpawnStagger;

        private static int KeyGroupIndex(Arrow arrow, Dictionary<string, int> keyGroups)
        {
            if (string.IsNullOrEmpty(arrow.KeyGroup))
                return -1;
            if (!keyGroups.TryGetValue(arrow.KeyGroup, out var index))
                keyGroups[arrow.KeyGroup] = index = keyGroups.Count;
            return index;
        }

        private IEnumerator PopIn(Transform target, Vector3 finalScale, float delay)
        {
            target.localScale = Vector3.zero;
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            var duration = config.CellSpawnDuration;
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                if (target == null) yield break;
                target.localScale = finalScale * Mathf.SmoothStep(0f, 1f, t / duration);
                yield return null;
            }
            if (target != null)
                target.localScale = finalScale;
        }
    }
}
