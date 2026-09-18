using System;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>사운드·진동·격자 설정 (GAME_RULES §9·§10). 재화가 아니라 PlayerPrefs. 사운드·진동은 기본 켜짐, 격자는 기본 꺼짐.</summary>
    public static class SettingsStore
    {
        private const string SoundKey = "sound_on";
        private const string VibrationKey = "vibration_on";
        private const string GridKey = "grid_on";

        public static event Action<bool> SoundChanged;
        public static event Action<bool> VibrationChanged;
        public static event Action<bool> GridChanged;

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

        /// <summary>보드 격자 표시 (GAME_RULES §9, 게임 화면 우하단 `#` 버튼). 기본 꺼짐.</summary>
        public static bool GridOn
        {
            get => PlayerPrefs.GetInt(GridKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(GridKey, value ? 1 : 0);
                PlayerPrefs.Save();
                GridChanged?.Invoke(value);
            }
        }
    }
}
