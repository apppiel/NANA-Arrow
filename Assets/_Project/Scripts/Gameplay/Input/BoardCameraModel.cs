using UnityEngine;

namespace NanaArrow.Gameplay.Input
{
    /// <summary>
    /// 보드 카메라의 줌·이동 상태 (순수 C#, GAME_RULES v0.7.1 §10). 좌표는 보드 중심 기준 월드 단위.
    /// 줌은 카메라 orthographicSize 를 줄이는 방식 → 보이는 반폭 = 기본 반폭 ÷ Zoom.
    /// 클램프: 줌 1 이면 항상 중앙. 보드(+여백)가 화면보다 크면 화면이 보드+여백 안에 머물도록, 작으면 그 축은 중앙.
    /// </summary>
    public sealed class BoardCameraModel
    {
        private readonly float _zoomMin;
        private readonly float _zoomMax;
        private readonly float _doubleTapSeconds;

        private Vector2 _boardHalfSize;
        private Vector2 _viewHalfSizeAtZoom1;
        private float _panMargin;
        private float _lastEmptyTapTime = float.NegativeInfinity;

        public float Zoom { get; private set; }
        /// <summary>카메라 중심의 보드 중심 대비 오프셋.</summary>
        public Vector2 Offset { get; private set; }
        public Vector2 ViewHalfSize => _viewHalfSizeAtZoom1 / Zoom;
        public bool IsDefault => Mathf.Approximately(Zoom, _zoomMin) && Offset == Vector2.zero;

        public BoardCameraModel(float zoomMin, float zoomMax, float doubleTapSeconds)
        {
            _zoomMin = Mathf.Max(1f, zoomMin);
            _zoomMax = Mathf.Max(_zoomMin, zoomMax);
            _doubleTapSeconds = doubleTapSeconds;
            Zoom = _zoomMin;
        }

        /// <summary>레벨마다: 보드 크기·화면 크기(줌 1 기준)·여백. 상태는 리셋.</summary>
        public void Configure(Vector2 boardHalfSize, Vector2 viewHalfSizeAtZoom1, float panMargin)
        {
            _boardHalfSize = boardHalfSize;
            _viewHalfSizeAtZoom1 = viewHalfSizeAtZoom1;
            _panMargin = panMargin;
            Reset();
        }

        public void Reset()
        {
            Zoom = _zoomMin;
            Offset = Vector2.zero;
        }

        /// <summary>focus(보드 기준 월드 좌표) 아래의 점이 화면에서 움직이지 않도록 줌.</summary>
        public void SetZoom(float zoom, Vector2 focus)
        {
            var newZoom = Mathf.Clamp(zoom, _zoomMin, _zoomMax);
            if (Mathf.Approximately(newZoom, Zoom)) return;
            Offset = focus - (focus - Offset) * (Zoom / newZoom);
            Zoom = newZoom;
            Clamp();
        }

        public void ZoomBy(float factor, Vector2 focus) => SetZoom(Zoom * factor, focus);

        /// <summary>카메라를 delta 만큼 이동 (손가락 드래그의 반대 방향을 넘길 것).</summary>
        public void Pan(Vector2 delta)
        {
            Offset += delta;
            Clamp();
        }

        /// <summary>빈 곳 탭. doubleTapSeconds 안에 두 번이면 리셋하고 true.</summary>
        public bool RegisterEmptyTap(float time)
        {
            var isDouble = time - _lastEmptyTapTime <= _doubleTapSeconds;
            _lastEmptyTapTime = isDouble ? float.NegativeInfinity : time;
            if (!isDouble) return false;
            Reset();
            return true;
        }

        private void Clamp()
        {
            if (Mathf.Approximately(Zoom, _zoomMin))
            {
                Offset = Vector2.zero;
                return;
            }
            var view = ViewHalfSize;
            Offset = new Vector2(ClampAxis(Offset.x, _boardHalfSize.x, view.x), ClampAxis(Offset.y, _boardHalfSize.y, view.y));
        }

        private float ClampAxis(float offset, float boardHalf, float viewHalf)
        {
            var limit = boardHalf + _panMargin - viewHalf;
            return limit <= 0f ? 0f : Mathf.Clamp(offset, -limit, limit);
        }
    }
}
