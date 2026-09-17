using System;
using NanaArrow.Gameplay.View;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace NanaArrow.Gameplay.Input
{
    /// <summary>
    /// Input System 포인터 → 셀. 짧게 누르면 탭(놓을 때 확정), GameConfig.longPressSeconds 이상 누르면 레인 미리보기 (GAME_RULES v0.6 §0).
    /// 미리보기가 시작된 누름은 탭으로 세지 않는다 (목숨 차감 없음). 판정은 구독자(GameSession) 몫.
    /// </summary>
    public sealed class TapInput : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private BoardView boardView;
        [SerializeField, Tooltip("비우면 Camera.main")] private Camera targetCamera;

        private bool _pressing;
        private bool _longPressed;
        private float _pressedAt;
        private Vector2Int _pressedCell;

        public event Action<Vector2Int> CellTapped;
        public event Action<Vector2Int> LanePreviewRequested;
        public event Action LanePreviewReleased;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null)
                return;

            if (pointer.press.wasPressedThisFrame)
            {
                // HUD·팝업 위의 터치는 보드 입력이 아니다
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    _pressing = false;
                    return;
                }
                var world = targetCamera.ScreenToWorldPoint(pointer.position.ReadValue());
                _pressing = boardView.TryGetCell(world, out _pressedCell);
                _longPressed = false;
                _pressedAt = Time.unscaledTime;
            }

            if (!_pressing)
                return;

            if (!_longPressed && Time.unscaledTime - _pressedAt >= config.LongPressSeconds)
            {
                _longPressed = true;
                LanePreviewRequested?.Invoke(_pressedCell);
            }

            if (pointer.press.wasReleasedThisFrame)
            {
                _pressing = false;
                if (_longPressed)
                    LanePreviewReleased?.Invoke();
                else
                    CellTapped?.Invoke(_pressedCell);
            }
        }
    }
}
