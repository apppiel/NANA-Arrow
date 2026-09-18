using System;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>사운드·진동·레인 가이드 설정 (GAME_RULES §9·§10). 재화가 아니라 PlayerPrefs. 사운드·진동은 기본 켜짐, 레인 가이드는 기본 꺼짐.</summary>
    public static class SettingsStore
    {
        private const string SoundKey = "sound_on";
        private const string VibrationKey = "vibration_on";
        private const string LaneGuideKey = "lane_guide_on";

        public static event Action<bool> SoundChanged;
        public static event Action<bool> VibrationChanged;
        public static event Action<bool> LaneGuideChanged;

        public static bool SoundOn
        {
            get => PlayerPrefs.GetInt(SoundKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(SoundKey, value ? 1 : 0);
                PlayerPrefs.Save();
                SoundChanged?.Invoke(value);
            }
        }

        /// <summary>하트 감소 시에만 진동 (vibrateOnLifeLost 는 LivesView 쪽 SerializeField).</summary>
        public static bool VibrationOn
        {
            get => PlayerPrefs.GetInt(VibrationKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(VibrationKey, value ? 1 : 0);
                PlayerPrefs.Save();
                VibrationChanged?.Invoke(value);
            }
        }

        /// <summary>레인 가이드 표시 (GAME_RULES v0.7.3 §9, 게임 화면 우하단 `#` 버튼). 기본 꺼짐.</summary>
        public static bool LaneGuideOn
        {
            get => PlayerPrefs.GetInt(LaneGuideKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(LaneGuideKey, value ? 1 : 0);
                PlayerPrefs.Save();
                LaneGuideChanged?.Invoke(value);
            }
        }
    }
}
