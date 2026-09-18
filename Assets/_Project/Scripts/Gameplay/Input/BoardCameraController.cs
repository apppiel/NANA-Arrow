using NanaArrow.Gameplay.View;
using UnityEngine;

namespace NanaArrow.Gameplay.Input
{
    /// <summary>
    /// Main Camera 에 붙여 핀치 줌·드래그 이동·더블 탭 리셋을 orthographicSize/position 으로 적용 (GAME_RULES v0.7.1 §10).
    /// 상태·클램프는 <see cref="BoardCameraModel"/>. 줌 1.0 의 크기 = 이 카메라의 시작 orthographicSize.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class BoardCameraController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private TapInput tapInput;
        [SerializeField] private BoardView boardView;

        private Camera _camera;
        private BoardCameraModel _model;
        private float _baseOrthographicSize;
        private Vector3 _basePosition;
        private Vector2 _boardCenter;

        public BoardCameraModel Model => _model;
        /// <summary>줌 1.0 기준 카메라 세로 반높이. BoardView 의 셀 크기 계산은 이 값을 쓴다.</summary>
        public float BaseOrthographicSize => _baseOrthographicSize;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _baseOrthographicSize = _camera.orthographicSize;
            _basePosition = transform.position;
            _model = new BoardCameraModel(config.ZoomMin, config.ZoomDefault, config.ZoomMax, config.DoubleTapSeconds);

            tapInput.Pan += OnPan;
            tapInput.Pinch += OnPinch;
            tapInput.Scroll += OnScroll;
            tapInput.EmptyTapped += OnEmptyTapped;
        }

        private void OnDestroy()
        {
            tapInput.Pan -= OnPan;
            tapInput.Pinch -= OnPinch;
            tapInput.Scroll -= OnScroll;
            tapInput.EmptyTapped -= OnEmptyTapped;
        }

        /// <summary>레벨(보드) 이 바뀔 때. 줌 리셋 포함.</summary>
        public void Attach(BoardLayout layout)
        {
            _boardCenter = layout.Center;
            var viewHalf = new Vector2(_baseOrthographicSize * _camera.aspect, _baseOrthographicSize);
            _model.Configure(layout.BoardSize * 0.5f, viewHalf, config.PanMarginCells * layout.CellSize);
            Apply();
        }

        /// <summary>레벨 시작·클리어·실패 시.</summary>
        public void ResetZoom()
        {
            _model.Reset();
            Apply();
        }

        private void OnPan(Vector2 fingerDelta)
        {
            _model.Pan(-fingerDelta);
            Apply();
        }

        private void OnPinch(float factor, Vector2 focusWorld) => ZoomAt(factor, focusWorld);

        private void OnScroll(float factor, Vector2 focusWorld) => ZoomAt(factor, focusWorld);

        private void ZoomAt(float factor, Vector2 focusWorld)
        {
            _model.ZoomBy(factor, focusWorld - _boardCenter);
            Apply();
        }

        private void OnEmptyTapped()
        {
            if (_model.RegisterEmptyTap(Time.unscaledTime))
                Apply();
        }

        private void Apply()
        {
            _camera.orthographicSize = _baseOrthographicSize / _model.Zoom;
            var target = _boardCenter + _model.Offset;
            transform.position = new Vector3(target.x, target.y, _basePosition.z);
        }
    }
}
