using System;
using UnityEngine;

namespace NanaArrow.UI.Tutorial
{
    /// <summary>튜토리얼 항목 하나 (UI_FLOW §7-1). 필드는 인스펙터에서 편집 (§7-2 표).</summary>
    [Serializable]
    public struct TutorialStep
    {
        [SerializeField, Min(1), Tooltip("레벨 번호")] private int level;
        [SerializeField, Tooltip("언제 표시하나")] private TutorialTrigger trigger;
        [SerializeField, Tooltip("§9 문구 키. 비우면 말풍선 숨김")] private string textKey;
        [SerializeField, Tooltip("손가락이 가리킬 화살표 id. 비우면 손가락 없음")] private string targetArrowId;
        [SerializeField, Tooltip("화살표 경로의 어느 칸을 가리키나")] private FingerAnchor fingerAnchor;
        [SerializeField, Tooltip("셀 단위 추가 오프셋")] private Vector2 fingerOffsetCells;
        [SerializeField, Tooltip("언제 숨기나 (None = hideDelay 만 사용)")] private TutorialTrigger hideOn;

        public TutorialStep(int level, TutorialTrigger trigger, string textKey, string targetArrowId,
            FingerAnchor fingerAnchor, Vector2 fingerOffsetCells, TutorialTrigger hideOn)
        {
            this.level = level;
            this.trigger = trigger;
            this.textKey = textKey;
            this.targetArrowId = targetArrowId;
            this.fingerAnchor = fingerAnchor;
            this.fingerOffsetCells = fingerOffsetCells;
            this.hideOn = hideOn;
        }

        public int Level => level;
        public TutorialTrigger Trigger => trigger;
        public string TextKey => textKey;
        public string TargetArrowId => targetArrowId;
        public FingerAnchor FingerAnchor => fingerAnchor;
        public Vector2 FingerOffsetCells => fingerOffsetCells;
        public TutorialTrigger HideOn => hideOn;

        public bool HasBubble => !string.IsNullOrEmpty(textKey);
        public bool HasFinger => !string.IsNullOrEmpty(targetArrowId);
    }
}
