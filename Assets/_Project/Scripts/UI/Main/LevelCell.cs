using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NanaArrow.UI.Main
{
    /// <summary>레벨 선택 칸 프리팹 (UI_FLOW §12-6). 잠긴 칸은 탭하면 흔들리기만 한다.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class LevelCell : MonoBehaviour
    {
        [SerializeField] private TMP_Text number;
        [SerializeField] private GameObject check;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject highlight;

        [Header("연출")]
        [SerializeField, Min(0f), Tooltip("잠김 탭 시 흔들림 (초)")] private float shakeDuration = 0.25f;
        [SerializeField, Min(0f), Tooltip("흔들림 폭 (캔버스 px)")] private float shakeDistance = 8f;

        private int _level;
        private LevelCellState _state;
        private Action<int> _onTap;
        private Coroutine _shake;

        public int Level => _level;
        public LevelCellState State => _state;

        private void Awake() => GetComponent<Button>().onClick.AddListener(OnClick);

        public void Bind(int level, LevelCellState state, Action<int> onTap)
        {
            _level = level;
            _state = state;
            _onTap = onTap;
            if (number != null) number.text = level.ToString();
            if (check != null) check.SetActive(state == LevelCellState.Cleared);
            if (lockIcon != null) lockIcon.SetActive(state == LevelCellState.Locked);
            if (highlight != null) highlight.SetActive(state == LevelCellState.Next);
        }

        private void OnClick()
        {
            if (_state == LevelCellState.Locked)
            {
                if (_shake == null) _shake = StartCoroutine(Shake());
                return;
            }
            _onTap?.Invoke(_level);
        }

        private IEnumerator Shake()
        {
            var rect = (RectTransform)transform;
            var origin = rect.anchoredPosition;
            for (var t = 0f; t < shakeDuration; t += Time.deltaTime)
            {
                var k = 1f - t / shakeDuration;
                rect.anchoredPosition = origin + Vector2.right * (Mathf.Sin(t / shakeDuration * Mathf.PI * 4f) * shakeDistance * k);
                yield return null;
            }
            rect.anchoredPosition = origin;
            _shake = null;
        }
    }
}
