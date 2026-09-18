using NanaArrow.Core;
using TMPro;
using UnityEngine;

namespace NanaArrow.UI.Game
{
    /// <summary>
    /// HUD 상단 중앙 난이도 라벨 (GAME_RULES v0.7.2 §10). LevelData `meta.difficulty` 1~3 을
    /// `hud.difficulty.easy / normal / hard` 문구로 보여 준다. difficulty 가 없는 레벨이면 라벨을 숨긴다.
    /// </summary>
    public sealed class DifficultyLabel : MonoBehaviour
    {
        /// <summary>difficulty 값 → 문구 키. 범위 밖(0 포함)이면 null = 표시 안 함.</summary>
        public static string KeyFor(int difficulty)
        {
            switch (difficulty)
            {
                case 1: return "hud.difficulty.easy";
                case 2: return "hud.difficulty.normal";
                case 3: return "hud.difficulty.hard";
                default: return null;
            }
        }

        [SerializeField] private Strings strings;
        [SerializeField] private GameController gameController;
        [SerializeField, Tooltip("난이도 문구를 쓸 TMP (비우면 이 오브젝트의 것)")] private TMP_Text label;

        private void Awake()
        {
            if (label == null) label = GetComponent<TMP_Text>();
            gameController.SessionStarted += OnSessionStarted;
        }

        private void OnDestroy() => gameController.SessionStarted -= OnSessionStarted;

        private void OnSessionStarted(GameSession session) => Refresh();

        private void Refresh()
        {
            var key = KeyFor(gameController.CurrentLevelData != null ? gameController.CurrentLevelData.Difficulty : 0);
            if (key == null)
            {
                label.gameObject.SetActive(false);
                return;
            }
            label.gameObject.SetActive(true);
            label.text = strings != null ? strings.Get(key) : key;
        }
    }
}
