using UnityEngine;

namespace NanaArrow.Data
{
    /// <summary>탑재 레벨 목록 (GAME_RULES §10). 인스펙터에서 순서 편집. 인덱스 0 = 레벨 1. Resources 폴더는 쓰지 않는다.</summary>
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "NanaArrow/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField, Tooltip("level_001.json 부터 순서대로")] private TextAsset[] levels;

        public int Count => levels?.Length ?? 0;

        /// <param name="level">1부터 시작하는 레벨 번호</param>
        public bool TryGet(int level, out TextAsset asset)
        {
            asset = null;
            if (levels == null || level < 1 || level > levels.Length) return false;
            asset = levels[level - 1];
            return asset != null;
        }

        /// <summary>테스트·에디터용.</summary>
        public void SetLevels(TextAsset[] assets) => levels = assets;
    }
}
