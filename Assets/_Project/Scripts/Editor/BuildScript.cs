using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NanaArrow.Editor
{
    /// <summary>
    /// Android 빌드 (W-021). 메뉴 한 번으로 개발 빌드 APK 를 <see cref="OutputFolder"/> 에 만든다.
    /// 릴리스 빌드는 키스토어가 필요하고, 키스토어 경로·비밀번호는 git 에 올리지 않는 로컬 파일
    /// (<see cref="KeystoreConfigPath"/>) 에서 읽는다 — 팀장이 만든다. docs/BUILD.md 참고.
    /// </summary>
    public static class BuildScript
    {
        /// <summary>APK 가 떨어지는 폴더 (프로젝트 루트 기준, .gitignore 됨).</summary>
        private const string OutputFolder = "Builds";

        /// <summary>키스토어 정보 (git 제외). 없으면 릴리스 빌드는 막고 개발 빌드만 된다.</summary>
        private const string KeystoreConfigPath = "ProjectSettings/keystore.local.json";

        private const BuildTarget Target = BuildTarget.Android;
        private const BuildTargetGroup TargetGroup = BuildTargetGroup.Android;

        [MenuItem("NanaArrow/Build/개발 빌드 APK %#b", priority = 100)]
        public static void BuildDevelopment() => Build(development: true);

        [MenuItem("NanaArrow/Build/릴리스 빌드 APK", priority = 101)]
        public static void BuildRelease() => Build(development: false);

        [MenuItem("NanaArrow/Build/EDM Force Resolve", priority = 120)]
        public static void ForceResolve()
        {
            if (!TryForceResolve(out var message))
                Debug.LogWarning($"[Build] EDM Force Resolve 실패 — {message}. Assets → External Dependency Manager → Android Resolver → Force Resolve 를 직접 실행하세요.");
            else
                Debug.Log("[Build] EDM Force Resolve 완료.");
        }

        private static void Build(bool development)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[Build] Build Profiles 의 Scene List 가 비어 있습니다. Boot / Main / Game 순서로 넣어 주세요.");
                return;
            }

            if (EditorUserBuildSettings.activeBuildTarget != Target &&
                !EditorUserBuildSettings.SwitchActiveBuildTarget(TargetGroup, Target))
            {
                Debug.LogError("[Build] Android 로 플랫폼 전환에 실패했습니다 (Android 모듈이 설치돼 있는지 확인).");
                return;
            }

            if (!development && !ApplyKeystore())
                return;

            PlayerSettings.Android.useCustomKeystore = !development;

            // 광고·Firebase 의존성이 Plugins/Android 에 풀려 있어야 한다. 실패해도 빌드는 시도한다.
            if (!TryForceResolve(out var resolveMessage))
                Debug.LogWarning($"[Build] EDM Force Resolve 를 건너뜁니다 — {resolveMessage}");

            Directory.CreateDirectory(OutputFolder);
            var suffix = development ? "dev" : "release";
            var path = Path.Combine(OutputFolder,
                $"NANA-Arrow_{PlayerSettings.bundleVersion}_{suffix}_{DateTime.Now:yyyyMMdd-HHmm}.apk");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = path,
                target = Target,
                targetGroup = TargetGroup,
                options = development
                    ? BuildOptions.Development | BuildOptions.AllowDebugging
                    : BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Build] 성공 — {path} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:F0}초)");
                EditorUtility.RevealInFinder(Path.GetFullPath(path));
            }
            else
            {
                Debug.LogError($"[Build] {summary.result} — 에러 {summary.totalErrors}건. Console 위쪽의 첫 에러를 보세요.");
            }
        }

        /// <summary>로컬 키스토어 파일을 PlayerSettings 에 적용. 파일이 없으면 false.</summary>
        private static bool ApplyKeystore()
        {
            if (!File.Exists(KeystoreConfigPath))
            {
                Debug.LogError($"[Build] 릴리스 빌드에는 키스토어가 필요합니다. {KeystoreConfigPath} 를 만들어 주세요 (docs/BUILD.md). " +
                               "개발 빌드는 키스토어 없이 됩니다.");
                return false;
            }

            var config = JsonUtility.FromJson<KeystoreConfig>(File.ReadAllText(KeystoreConfigPath));
            if (config == null || string.IsNullOrEmpty(config.keystorePath) || !File.Exists(config.keystorePath))
            {
                Debug.LogError($"[Build] {KeystoreConfigPath} 의 keystorePath 가 비었거나 그 경로에 파일이 없습니다.");
                return false;
            }

            PlayerSettings.Android.keystoreName = config.keystorePath;
            PlayerSettings.Android.keystorePass = config.keystorePass;
            PlayerSettings.Android.keyaliasName = config.keyaliasName;
            PlayerSettings.Android.keyaliasPass = config.keyaliasPass;
            return true;
        }

        /// <summary>
        /// EDM(External Dependency Manager) 의 Android Resolver 를 리플렉션으로 호출한다.
        /// EDM 은 asmdef 없이 Assembly-CSharp-Editor 에 들어가서 이 어셈블리에서 직접 참조할 수 없다.
        /// </summary>
        private static bool TryForceResolve(out string message)
        {
            var resolver = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("GooglePlayServices.PlayServicesResolver"))
                .FirstOrDefault(t => t != null);

            if (resolver == null)
            {
                message = "PlayServicesResolver 를 찾지 못했습니다 (EDM 미설치 또는 아직 컴파일 전)";
                return false;
            }

            var resolve = resolver.GetMethod("MenuResolve", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                          ?? resolver.GetMethod("MenuResolve", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (resolve == null)
            {
                message = "MenuResolve 메서드를 찾지 못했습니다 (EDM 버전 차이)";
                return false;
            }

            resolve.Invoke(null, null);
            message = null;
            return true;
        }

        [Serializable]
        private sealed class KeystoreConfig
        {
            public string keystorePath;
            public string keystorePass;
            public string keyaliasName;
            public string keyaliasPass;
        }
    }
}
