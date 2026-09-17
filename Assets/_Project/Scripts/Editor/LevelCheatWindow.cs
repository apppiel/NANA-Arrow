using System;
using NanaArrow.Core;
using UnityEditor;
using UnityEngine;

namespace NanaArrow.Editor
{
    /// <summary>플레이 모드 치트: Game 씬의 GameController 에 Levels/level_###.json 을 바로 로드한다.</summary>
    public sealed class LevelCheatWindow : EditorWindow
    {
        private int _levelId = 1;

        [MenuItem("NanaArrow/Cheat/Level Jump")]
        public static void Open() => GetWindow<LevelCheatWindow>("Level Jump");

        private void OnGUI()
        {
            var controller = Application.isPlaying ? FindFirstObjectByType<GameController>() : null;
            if (controller == null)
            {
                EditorGUILayout.HelpBox("플레이 모드에서 Game 씬(GameController)이 있어야 합니다.", MessageType.Info);
                return;
            }

            var current = controller.Session?.Level.Id;
            EditorGUILayout.LabelField("현재 레벨", current.HasValue ? current.Value.ToString() : "-");

            using (new EditorGUILayout.HorizontalScope())
            {
                _levelId = Mathf.Max(1, EditorGUILayout.IntField("레벨 번호", _levelId));
                if (GUILayout.Button("이동", GUILayout.Width(60)))
                    Jump(controller, _levelId);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!current.HasValue || current.Value <= 1))
                {
                    if (GUILayout.Button("◀ 이전"))
                        Jump(controller, current.Value - 1);
                }
                using (new EditorGUI.DisabledScope(!current.HasValue))
                {
                    if (GUILayout.Button("다시하기"))
                        Jump(controller, current.Value);
                    if (GUILayout.Button("다음 ▶"))
                        Jump(controller, current.Value + 1);
                }
            }
        }

        private void Jump(GameController controller, int id)
        {
            var path = LevelFiles.PathFor(id);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null)
            {
                Debug.LogWarning($"[LevelCheat] {path} 가 없습니다.");
                return;
            }
            try
            {
                controller.LoadLevel(asset);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LevelCheat] {path} 로드 실패: {e.Message}");
                return;
            }
            _levelId = id;
            Repaint();
        }
    }
}
