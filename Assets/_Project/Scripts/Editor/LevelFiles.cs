using System;
using System.IO;
using System.Linq;
using NanaArrow.Data;
using UnityEditor;

namespace NanaArrow.Editor
{
    /// <summary>Assets/_Project/Levels/level_###.json 파일 접근 (에디터 전용).</summary>
    public static class LevelFiles
    {
        public const string Folder = "Assets/_Project/Levels";

        public static string PathFor(int id) => $"{Folder}/level_{id:000}.json";

        /// <summary>폴더의 모든 레벨 JSON 경로, 이름 순.</summary>
        public static string[] FindAll()
        {
            if (!Directory.Exists(Folder))
                return Array.Empty<string>();
            return Directory.GetFiles(Folder, "*.json")
                .Select(p => p.Replace('\\', '/'))
                .OrderBy(p => p, StringComparer.Ordinal)
                .ToArray();
        }

        public static LevelData Load(string path) => LevelLoader.Parse(File.ReadAllText(path));

        public static void Save(string path, LevelData level)
        {
            File.WriteAllText(path, LevelLoader.ToJson(level) + "\n");
            AssetDatabase.ImportAsset(path);
        }
    }
}
