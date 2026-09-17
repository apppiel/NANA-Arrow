using System.IO;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>앱 전역 서비스 로케이터 (CLAUDE.md Core). 씬을 넘나드는 순수 C# 객체만 둔다. MonoBehaviour 싱글턴은 각자 Instance.</summary>
    public static class App
    {
        private const string SaveFileName = "save.json";

        private static PlayerProgress _progress;

        public static PlayerProgress Progress =>
            _progress ??= new PlayerProgress(new SaveService(Path.Combine(Application.persistentDataPath, SaveFileName)));

        /// <summary>테스트·치트용 교체.</summary>
        public static void SetProgress(PlayerProgress progress) => _progress = progress;
    }
}
