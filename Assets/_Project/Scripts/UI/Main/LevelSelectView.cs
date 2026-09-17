using System.Collections.Generic;
using NanaArrow.Core;
using NanaArrow.Data;
using UnityEngine;
using UnityEngine.UI;

namespace NanaArrow.UI.Main
{
    /// <summary>
    /// 레벨 선택 패널 (UI_FLOW §4, §12-6): 카탈로그 개수만큼 LevelCell 을 만들고 상태를 표시. 열 때마다 상태를 다시 계산.
    /// 열리는 칸 = 클리어한 레벨 + 다음 1개. 뒤로가기(Android) 는 패널 닫기.
    /// </summary>
    public sealed class LevelSelectView : MonoBehaviour
    {
        [SerializeField] private LevelCatalog catalog;
        [SerializeField, Tooltip("Prefabs/UI/LevelCell")] private LevelCell cellPrefab;
        [SerializeField, Tooltip("Scroll/Viewport/Content (Grid Layout Group)")] private Transform content;
        [SerializeField, Tooltip("있으면 열 때 '다음 도전' 칸이 보이게 스크롤")] private ScrollRect scrollRect;
        [SerializeField, Min(1), Tooltip("Grid Layout Group 의 열 수 (스크롤 위치 계산용)")] private int columns = 5;

        private readonly List<LevelCell> _cells = new List<LevelCell>();

        public bool IsOpen => gameObject.activeSelf;

        private void Awake() => BackButton.Pressed += OnBackPressed;
        private void OnDestroy() => BackButton.Pressed -= OnBackPressed;

        /// <summary>칸 상태 규칙: 클리어 / 바로 다음 하나 / 잠김.</summary>
        public static LevelCellState StateFor(int level, int highestCleared)
        {
            if (level <= highestCleared) return LevelCellState.Cleared;
            return level == highestCleared + 1 ? LevelCellState.Next : LevelCellState.Locked;
        }

        /// <summary>LevelSelectButton.onClick</summary>
        public void Open()
        {
            var highest = App.Progress.HighestClearedLevel;
            EnsureCells();
            for (var i = 0; i < _cells.Count; i++)
                _cells[i].Bind(i + 1, StateFor(i + 1, highest), Play);
            gameObject.SetActive(true);
            ScrollTo(highest + 1);
            GameEvents.RaiseLevelSelectOpened(highest);
        }

        /// <summary>Header/BackButton.onClick</summary>
        public void Close() => gameObject.SetActive(false);

        private void Play(int level)
        {
            AudioManager.Instance?.Play(SoundId.SfxButton);
            SceneLoader.LoadGame(level);
        }

        private void EnsureCells()
        {
            var count = catalog != null ? catalog.Count : 0;
            while (_cells.Count < count)
                _cells.Add(Instantiate(cellPrefab, content));
            for (var i = 0; i < _cells.Count; i++)
                _cells[i].gameObject.SetActive(i < count);
        }

        private void ScrollTo(int level)
        {
            if (scrollRect == null || _cells.Count == 0) return;
            var rows = Mathf.CeilToInt(_cells.Count / (float)columns);
            var row = Mathf.Clamp((level - 1) / columns, 0, rows - 1);
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = rows > 1 ? 1f - row / (float)(rows - 1) : 1f;
        }

        private void OnBackPressed()
        {
            if (IsOpen && !PopupBase.ConsumedBack) Close();
        }
    }
}
