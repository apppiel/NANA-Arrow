using System;
using NanaArrow.Core;
using NanaArrow.Data;
using UnityEditor;
using UnityEngine;

namespace NanaArrow.Editor
{
    /// <summary>플레이 모드 치트 (GAME_RULES §10): 레벨 점프 · 즉시 클리어 · 하트 채우기. Game 씬의 GameController 를 통해 동작.</summary>
    public sealed class LevelCheatWindow : EditorWindow
    {
        private int _levelId = 1;

        [MenuItem("NanaArrow/Cheat/Level Jump")]
        public static void Open() => GetWindow<LevelCheatWindow>("Cheat");

        private void OnGUI()
        {
            var controller = Application.isPlaying ? FindFirstObjectByType<GameController>() : null;
            if (controller == null)
            {
                EditorGUILayout.HelpBox("플레이 모드에서 Game 씬(GameController)이 있어야 합니다.", MessageType.Info);
                return;
            }

            var current = controller.Session != null ? controller.CurrentLevel : (int?)null;
            EditorGUILayout.LabelField("현재 레벨", current.HasValue ? current.Value.ToString() : "-");
            EditorGUILayout.LabelField("카탈로그", controller.Catalog != null ? $"{controller.Catalog.Count}개" : "없음 → Levels/ 파일 직접 로드");

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
                    if (GUILayout.Button("◀ 이전")) Jump(controller, current.Value - 1);
                }
                using (new EditorGUI.DisabledScope(!current.HasValue))
                {
                    if (GUILayout.Button("다시하기")) controller.Restart();
                    if (GUILayout.Button("다음 ▶")) Jump(controller, current.Value + 1);
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(controller.Session == null))
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("즉시 클리어")) controller.CheatClear();
                if (GUILayout.Button("하트 채우기")) controller.CheatRefillLives();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("저장 초기화 (최고 레벨 0)"))
                App.Progress.Reset();
            EditorGUILayout.LabelField("최고 클리어 레벨", App.Progress.HighestClearedLevel.ToString());
        }

        private void Jump(GameController controller, int id)
        {
            if (controller.Catalog != null)
            {
                if (controller.LoadLevel(id)) _levelId = id;
                Repaint();
                return;
            }

            // 카탈로그 미연결: Levels/ 파일을 직접 읽는다
            var path = LevelFiles.PathFor(id);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null)
            {
                Debug.LogWarning($"[LevelCheat] {path} 가 없습니다.");
                return;
            }
            try
            {
                controller.LoadLevel(LevelLoader.Parse(asset.text));
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
