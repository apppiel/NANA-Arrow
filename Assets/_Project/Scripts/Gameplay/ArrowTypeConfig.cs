using System;
using UnityEngine;

namespace NanaArrow.Gameplay
{
    /// <summary>
    /// Arrow 타입별 설정 (GAME_RULES §3, LEVEL_FORMAT). 경로 길이는 GameConfig.maxArrowLength.
    /// 인스턴스는 Assets/_Project/Settings/ArrowTypeConfig.asset.
    /// </summary>
    [CreateAssetMenu(fileName = "ArrowTypeConfig", menuName = "NanaArrow/Arrow Type Config")]
    public sealed class ArrowTypeConfig : ScriptableObject
    {
        [Header("도입 레벨 (GAME_RULES §3, 초안)")]
        [SerializeField, Min(1)]
        private int basicIntroLevel = 1;

        [SerializeField, Min(1)]
        private int frozenIntroLevel = 16;

        [SerializeField, Min(1), Tooltip("Locked 와 Key 는 함께 도입")]
        private int lockedIntroLevel = 26;

        [Header("Frozen")]
        [SerializeField, Min(2), Tooltip("Exit 까지 총 탭 수 (마지막 Fire 포함, 2 이상). 레벨 JSON 의 hits 생략 시 기본값")]
        private int frozenDefaultHits = 2;

        public int FrozenDefaultHits => frozenDefaultHits;

        public int GetIntroLevel(ArrowType type)
        {
            switch (type)
            {
                case ArrowType.Basic: return basicIntroLevel;
                case ArrowType.Frozen: return frozenIntroLevel;
                case ArrowType.Locked:
                case ArrowType.Key: return lockedIntroLevel;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
