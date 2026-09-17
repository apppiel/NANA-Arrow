using UnityEngine.SceneManagement;

namespace NanaArrow.Core
{
    /// <summary>씬 전환 (NO.2 SceneLoader 이식, 씬 enum). Main → Game 으로 넘길 레벨은 프로세스 스코프 static 으로 전달 (PlayerPrefs 아님).</summary>
    public static class SceneLoader
    {
        /// <summary>Game 씬이 시작할 때 읽어 갈 레벨. null 이면 GameController 의 인스펙터 값.</summary>
        public static int? PendingLevel { get; private set; }

        public static void Load(SceneId scene) => SceneManager.LoadScene(scene.ToString());

        public static void LoadGame(int level)
        {
            PendingLevel = level;
            Load(SceneId.Game);
        }

        public static int? ConsumePendingLevel()
        {
            var level = PendingLevel;
            PendingLevel = null;
            return level;
        }
    }
}
