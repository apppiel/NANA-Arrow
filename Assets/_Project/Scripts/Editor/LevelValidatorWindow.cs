using System;
using System.Collections.Generic;
using System.Linq;
using NanaArrow.Data;
using NanaArrow.Gameplay;
using UnityEditor;
using UnityEngine;

namespace NanaArrow.Editor
{
    /// <summary>
    /// Levels/ 폴더 전체를 LevelValidator 로 검증하고, 통과한 레벨에만 solution·minTaps 를 기록해 저장한다.
    /// 실패한 레벨은 저장하지 않는다 (LEVEL_FORMAT 규칙 5·6).
    /// </summary>
    public sealed class LevelValidatorWindow : EditorWindow
    {
        private GameConfig _gameConfig;
        private ArrowTypeConfig _arrowTypeConfig;
        private readonly List<LevelFileReport> _reports = new List<LevelFileReport>();
        private Vector2 _scroll;

        [MenuItem("NanaArrow/Level Validator")]
        public static void Open() => GetWindow<LevelValidatorWindow>("Level Validator");

        private void OnEnable()
        {
            if (_gameConfig == null) _gameConfig = FindAsset<GameConfig>();
            if (_arrowTypeConfig == null) _arrowTypeConfig = FindAsset<ArrowTypeConfig>();
        }

        private void OnGUI()
        {
            _gameConfig = (GameConfig)EditorGUILayout.ObjectField("Game Config", _gameConfig, typeof(GameConfig), false);
            _arrowTypeConfig = (ArrowTypeConfig)EditorGUILayout.ObjectField("Arrow Type Config", _arrowTypeConfig, typeof(ArrowTypeConfig), false);

            var configsReady = _gameConfig != null && _arrowTypeConfig != null;
            if (!configsReady)
                EditorGUILayout.HelpBox("Assets/_Project/Settings 의 GameConfig / ArrowTypeConfig 를 연결하세요.", MessageType.Warning);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!configsReady))
                {
                    if (GUILayout.Button($"검증 ({LevelFiles.Folder}/*.json)"))
                        ValidateAll();
                }

                var validCount = _reports.Count(r => r.IsValid);
                using (new EditorGUI.DisabledScope(validCount == 0))
                {
                    if (GUILayout.Button($"통과한 {validCount}개에 solution·minTaps 기록 후 저장"))
                        RecordAndSave();
                }
            }

            if (_reports.Count == 0)
            {
                EditorGUILayout.HelpBox("검증을 실행하면 결과가 여기 표시됩니다.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{_reports.Count}개 중 통과 {_reports.Count(r => r.IsValid)}개", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var report in _reports)
                DrawReport(report);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawReport(LevelFileReport report)
        {
            var fileName = System.IO.Path.GetFileName(report.Path);
            if (report.ParseError != null)
            {
                EditorGUILayout.HelpBox($"✗ {fileName}\n파싱 실패: {report.ParseError}", MessageType.Error);
                return;
            }

            if (report.IsValid)
            {
                EditorGUILayout.HelpBox(
                    $"✓ {fileName}  ({report.Level.Width}×{report.Level.Height}, Arrow {report.Level.Arrows.Length}개){MaskSummary(report.Level)}\n" +
                    $"minTaps {report.Result.MinTaps}  solution: {string.Join(" → ", report.Result.Solution)}",
                    MessageType.None);
                DrawMaskPreview(report.Level);
                return;
            }

            var errors = string.Join("\n", report.Result.Errors.Select(e => $"  {e}"));
            EditorGUILayout.HelpBox($"✗ {fileName}\n{errors}", MessageType.Error);
            DrawMaskPreview(report.Level);
        }

        private static string MaskSummary(LevelData level) =>
            level?.Mask == null || level.Mask.Length == 0
                ? ""
                : $"  ·  mask {CountUsable(level)}칸";

        private static int CountUsable(LevelData level) =>
            level.Mask.Sum(row => row == null ? 0 : row.Count(c => c == BoardMask.Included));

        /// <summary>비직사각 보드 모양을 아스키로 보여 준다 (W-027, 기획자가 파일을 안 열고 확인하도록).</summary>
        private static void DrawMaskPreview(LevelData level)
        {
            if (level?.Mask == null || level.Mask.Length == 0) return;

            EditorGUILayout.LabelField($"mask ({level.Width}×{level.Height}, 위→아래)", EditorStyles.miniBoldLabel);
            var style = new GUIStyle(EditorStyles.label) { font = EditorStyles.miniFont, richText = false };
            // 칸을 두 칸 폭으로 벌려야 정사각형처럼 보인다
            foreach (var row in level.Mask)
                EditorGUILayout.LabelField("   " + string.Join(" ", (row ?? "").ToCharArray()), style);
            EditorGUILayout.Space(4f);
        }

        private void ValidateAll()
        {
            _reports.Clear();
            foreach (var path in LevelFiles.FindAll())
            {
                try
                {
                    var level = LevelFiles.Load(path);
                    _reports.Add(new LevelFileReport(path, level, LevelValidator.Validate(level, _gameConfig, _arrowTypeConfig)));
                }
                catch (Exception e)
                {
                    _reports.Add(new LevelFileReport(path, e.Message));
                }
            }
        }

        private void RecordAndSave()
        {
            var saved = 0;
            foreach (var report in _reports.Where(r => r.IsValid))
            {
                LevelValidator.Record(report.Level, report.Result);
                LevelFiles.Save(report.Path, report.Level);
                saved++;
            }
            Debug.Log($"[LevelValidator] {saved}개 레벨에 solution·minTaps 기록 후 저장했습니다.");
        }

        private static T FindAsset<T>() where T : UnityEngine.Object
        {
            var guid = AssetDatabase.FindAssets($"t:{typeof(T).Name}").FirstOrDefault();
            return guid == null ? null : AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}
