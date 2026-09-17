using System;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>
    /// 저장 파일의 내용물 (NO.3 SaveData 이식). 저장하는 값은 <b>최고 클리어 레벨 · 응모 코드 발급 여부</b> 둘뿐.
    /// 사운드·진동 설정은 재화가 아니라 PlayerPrefs (<see cref="SettingsStore"/>). 레벨 중간 상태는 저장하지 않는다 (GAME_RULES §10).
    /// 구조체 + [SerializeField] private: JsonUtility 가 값 타입을 직접 다루고 공개 필드 금지를 지킨다. 없는 필드는 기본값으로 읽히므로 필드 추가에 마이그레이션이 필요 없다.
    /// </summary>
    [Serializable]
    public struct SaveData
    {
        /// <summary>스키마 버전. 호환 안 되는 구조 변경 시에만 올린다.</summary>
        public const int CurrentVersion = 1;

        [SerializeField] private int v;
        [SerializeField] private int highestClearedLevel;
        [SerializeField] private bool rewardCodeIssued;

        public int Version => v;
        /// <summary>클리어한 가장 높은 레벨 번호. 0 = 아직 없음.</summary>
        public int HighestClearedLevel => highestClearedLevel;
        /// <summary>100단계 응모 코드를 발급받았는지 (코드 값은 RewardCodeService 가 따로 보관).</summary>
        public bool RewardCodeIssued => rewardCodeIssued;

        public SaveData(int highestClearedLevel, bool rewardCodeIssued)
        {
            v = CurrentVersion;
            this.highestClearedLevel = highestClearedLevel;
            this.rewardCodeIssued = rewardCodeIssued;
        }

        /// <summary>첫 설치 상태.</summary>
        public static SaveData Fresh() => new SaveData(0, false);

        public SaveData WithHighestClearedLevel(int level) => new SaveData(level, rewardCodeIssued);
        public SaveData WithRewardCodeIssued(bool issued) => new SaveData(highestClearedLevel, issued);
    }
}
