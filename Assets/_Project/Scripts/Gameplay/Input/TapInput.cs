using System;
using NanaArrow.Gameplay.View;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace NanaArrow.Gameplay.Input
{
    /// <summary>
    /// 보드 위 손가락 제스처 분류 (GAME_RULES §2-7, §10). 판정은 구독자 몫.
    /// 한 손가락: 누른 뒤 (a) dragThresholdCells 이상 움직이면 드래그(Pan) 확정 — 탭·미리보기 아님 (b) longPressSeconds 경과 → 미리보기 (c) 그 전에 놓으면 탭.
    /// 두 손가락: 즉시 핀치(Pinch) 모드, 진행 중이던 탭·미리보기 취소. 손가락이 모두 떨어질 때까지 탭 후보를 새로 만들지 않는다.
    /// 에디터·PC: 마우스 드래그 = Pan, 마우스 휠 = Scroll.
    /// </summary>
    public sealed class TapInput : MonoBehaviour
    {
        /// <summary>마우스 휠 한 눈금당 줌 배율 변화 (에디터·PC 전용).</summary>
        private const float WheelZoomStep = 0.1f;

        [SerializeField] private GameConfig config;
        [SerializeField] private BoardView boardView;
        [SerializeField, Tooltip("비우면 Camera.main")] private Camera targetCamera;

        private bool _pressing;
        private bool _longPressed;
        private bool _dragging;
        private bool _pinching;
        private bool _blockedUntilRelease;
        private float _pressedAt;
        private Vector2 _pressedScreen;
        private Vector2 _lastScreen;
        private Vector2Int _pressedCell;
        private bool _pressedOnBoard;
        private float _lastPinchDistance;

        /// <summary>Arrow 가 있는 셀을 탭.</summary>
        public event Action<Vector2Int> CellTapped;
        /// <summary>보드 밖 또는 빈 셀을 탭 (더블 탭 줌 리셋용).</summary>
        public event Action EmptyTapped;
        public event Action<Vector2Int> LanePreviewRequested;
        public event Action LanePreviewReleased;
        /// <summary>드래그 이동량 (월드 단위, 손가락 기준).</summary>
        public event Action<Vector2> Pan;
        /// <summary>핀치 배율(이전 프레임 대비)과 중심점(월드).</summary>
        public event Action<float, Vector2> Pinch;
        /// <summary>마우스 휠 배율과 커서 위치(월드).</summary>
        public event Action<float, Vector2> Scroll;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void Update()
        {
            if (HandlePinch())
                return;

            HandleScroll();

            var pointer = Pointer.current;
            if (pointer == null)
                return;

            if (pointer.press.wasPressedThisFrame)
                BeginPress(pointer.position.ReadValue());

            if (!_pressing)
                return;

            var screen = pointer.position.ReadValue();

            if (!_dragging && (screen - _pressedScreen).magnitude >= DragThresholdPixels())
            {
                _dragging = true;
                if (_longPressed) LanePreviewReleased?.Invoke();
                _lastScreen = screen;
            }

            if (_dragging)
            {
                var delta = ToWorld(screen) - ToWorld(_lastScreen);
                _lastScreen = screen;
                if (delta != Vector2.zero) Pan?.Invoke(delta);
            }
            else if (!_longPressed && Time.unscaledTime - _pressedAt >= config.LongPressSeconds)
            {
                _longPressed = true;
                if (_pressedOnBoard) LanePreviewRequested?.Invoke(_pressedCell);
            }

            if (pointer.press.wasReleasedThisFrame)
                EndPress();
        }

        private void BeginPress(Vector2 screen)
        {
            if (_blockedUntilRelease) return;
            // HUD·팝업 위의 터치는 보드 입력이 아니다
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                _pressing = false;
                return;
            }
            _pressing = true;
            _longPressed = false;
            _dragging = false;
            _pressedAt = Time.unscaledTime;
            _pressedScreen = screen;
            _lastScreen = screen;
            _pressedOnBoard = boardView.TryGetCell(ToWorld(screen), out _pressedCell);
        }

        private void EndPress()
        {
            _pressing = false;
            if (_dragging) return;
            if (_longPressed)
            {
                if (_pressedOnBoard) LanePreviewReleased?.Invoke();
                return;
            }
            if (_pressedOnBoard && boardView.HasArrowAt(_pressedCell))
                CellTapped?.Invoke(_pressedCell);
            else
                EmptyTapped?.Invoke();
        }

        /// <summary>두 손가락이면 핀치 모드. true 를 돌려주면 이번 프레임의 한 손가락 처리는 건너뛴다.</summary>
        private bool HandlePinch()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return false;

            var touches = touchscreen.touches;
            Vector2 a = default, b = default;
            var count = 0;
            for (var i = 0; i < touches.Count && count < 2; i++)
            {
                if (!touches[i].press.isPressed) continue;
                if (count == 0) a = touches[i].position.ReadValue();
                else b = touches[i].position.ReadValue();
                count++;
            }

            if (count >= 2)
            {
                if (!_pinching)
                {
                    _pinching = true;
                    _blockedUntilRelease = true;
                    CancelPending();
                    _lastPinchDistance = (a - b).magnitude;
                    return true;
                }
                var distance = (a - b).magnitude;
                if (_lastPinchDistance > 0f && distance > 0f)
                    Pinch?.Invoke(distance / _lastPinchDistance, ToWorld((a + b) * 0.5f));
                _lastPinchDistance = distance;
                return true;
            }

            _pinching = false;
            if (count == 0) _blockedUntilRelease = false;
            return _blockedUntilRelease;
        }

        private void HandleScroll()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            var wheel = mouse.scroll.ReadValue().y;
            if (Mathf.Approximately(wheel, 0f)) return;
            var factor = 1f + Mathf.Sign(wheel) * WheelZoomStep;
            Scroll?.Invoke(factor, ToWorld(mouse.position.ReadValue()));
        }

        private void CancelPending()
        {
            if (_pressing && _longPressed && _pressedOnBoard) LanePreviewReleased?.Invoke();
            _pressing = false;
            _longPressed = false;
            _dragging = false;
        }

        private float DragThresholdPixels()
        {
            var cell = boardView.Layout?.CellSize ?? 1f;
            var pixelsPerUnit = Screen.height / (targetCamera.orthographicSize * 2f);
            return config.DragThresholdCells * cell * pixelsPerUnit;
        }

        private Vector2 ToWorld(Vector2 screen) => targetCamera.ScreenToWorldPoint(screen);
    }
}
