using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.UI.Tutorial
{
    /// <summary>튜토리얼 항목 목록 (UI_FLOW §7). 레벨 JSON 이 아니라 여기(Settings/TutorialConfig.asset)에 둔다.</summary>
    [CreateAssetMenu(menuName = "NanaArrow/Tutorial Config", fileName = "TutorialConfig")]
    public sealed class TutorialConfig : ScriptableObject
    {
        [SerializeField] private TutorialStep[] steps = new TutorialStep[0];
        [SerializeField, Min(0f), Tooltip("hideOn 이 None 인 말풍선의 자동 숨김 (초). 0 이면 판이 끝날 때까지 유지")]
        private float hideDelay = 3f;
        [SerializeField, Tooltip("같은 레벨을 다시 할 때도 표시 (끄면 한 번이라도 클리어한 레벨은 표시 안 함)")]
        private bool showOnReplay;

        public IReadOnlyList<TutorialStep> Steps => steps;
        public float HideDelay => hideDelay;
        public bool ShowOnReplay => showOnReplay;

        /// <summary>해당 레벨의 항목만 (등록 순서 유지).</summary>
        public List<TutorialStep> StepsFor(int level)
        {
            var result = new List<TutorialStep>();
            foreach (var step in steps)
                if (step.Level == level) result.Add(step);
            return result;
        }

        /// <summary>에디터·테스트용 일괄 설정.</summary>
        public void SetSteps(TutorialStep[] items) => steps = items ?? new TutorialStep[0];
    }
}
