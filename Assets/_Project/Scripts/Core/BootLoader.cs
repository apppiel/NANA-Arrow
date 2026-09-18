using System.Collections;
using NanaArrow.Gameplay;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>Boot 씬 진입점 (NO.3 BootLoader 이식, Addressables 제거): 프레임 설정 → 저장 로드 → 설정 적용 → 최소 표시 시간 뒤 다음 씬. 광고 SDK 는 스스로 초기화한다 (완료를 기다리지 않음).</summary>
    public sealed class BootLoader : MonoBehaviour
    {
        [SerializeField] private SceneId nextScene = SceneId.Main;
        [SerializeField, Min(0f), Tooltip("로고를 최소 이 시간만큼 보여준 뒤 이동 (UI_FLOW §3 bootMinDuration)")]
        private float minDuration = 1f;
        [SerializeField, Tooltip("targetFrameRate 를 읽어올 곳 (W-025 3). 비우면 프레임을 건드리지 않는다")]
        private GameConfig gameConfig;

        private void Awake()
        {
            if (gameConfig == null) return;
            // 모바일은 targetFrameRate 가 -1 이면 30 으로 잡힌다. vSync 가 켜져 있으면 targetFrameRate 가 무시되므로 함께 끈다.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = gameConfig.TargetFrameRate;
        }

        private IEnumerator Start()
        {
            var startedAt = Time.realtimeSinceStartup;

            var progress = App.Progress;
            Debug.Log($"[Boot] 저장 로드: 최고 클리어 레벨 {progress.HighestClearedLevel}, 응모 코드 {(progress.RewardCodeIssued ? "발급됨" : "없음")}");
            if (AudioManager.Instance != null)
                AudioManager.Instance.ApplySettings();

            var remaining = minDuration - (Time.realtimeSinceStartup - startedAt);
            if (remaining > 0f)
                yield return new WaitForSecondsRealtime(remaining);

            SceneLoader.Load(nextScene);
        }
    }
}
