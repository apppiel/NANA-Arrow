using System;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>사운드·진동 설정 (GAME_RULES §10). 재화가 아니라 PlayerPrefs. 기본 둘 다 켜짐.</summary>
    public static class SettingsStore
    {
        private const string SoundKey = "sound_on";
        private const string VibrationKey = "vibration_on";

        public static event Action<bool> SoundChanged;
        public static event Action<bool> VibrationChanged;

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
    }
}
