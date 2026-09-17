using System;
using NanaArrow.Gameplay.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NanaArrow.Gameplay.Input
{
    /// <summary>Input System 탭 → 월드 좌표 → 셀. 탭만 쓴다 (GAME_RULES §9). 판정은 구독자(GameSession) 몫.</summary>
    public sealed class TapInput : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        [SerializeField, Tooltip("비우면 Camera.main")] private Camera targetCamera;

        public event Action<Vector2Int> CellTapped;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
                return;

            var world = targetCamera.ScreenToWorldPoint(pointer.position.ReadValue());
            if (boardView.TryGetCell(world, out var cell))
                CellTapped?.Invoke(cell);
        }
    }
}
