using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>
    /// 짧은 진동 (NO.2 Haptics 이식, Android). Handheld.Vibrate 는 500ms 라 거칠어서 Vibrator 를 직접 부른다.
    /// 재생 직전에 <see cref="SettingsStore.VibrationOn"/> 을 확인하므로 호출부는 설정을 신경 쓰지 않는다. 실패해도 조용히 넘어간다.
    /// iOS 는 v1 대상이 아니라 Handheld.Vibrate 로 대체 (네이티브 브릿지 없음).
    /// </summary>
    public static class Haptics
    {
        /// <summary>하트 감소용 짧은 진동 (ms) 과 세기 (1~255).</summary>
        private const long LifeLostDurationMs = 30;
        private const int LifeLostAmplitude = 160;
        private const int AmplitudeControlSdk = 26;

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject _vibrator;
        private static bool _resolved;
        private static int _sdkInt;
#endif

        /// <summary>하트가 줄 때 (GAME_RULES §10 vibrateOnLifeLost).</summary>
        public static void LifeLost()
        {
            if (!SettingsStore.VibrationOn) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            var vib = ResolveVibrator();
            if (vib == null) return;
            try
            {
                if (_sdkInt >= AmplitudeControlSdk)
                {
                    using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    using (var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", LifeLostDurationMs, LifeLostAmplitude))
                        vib.Call("vibrate", effect);
                }
                else
                {
                    vib.Call("vibrate", LifeLostDurationMs);
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[Haptics] Android 진동 실패: " + e.Message); }
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject ResolveVibrator()
        {
            if (_resolved) return _vibrator;
            _resolved = true;
            try
            {
                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    _sdkInt = version.GetStatic<int>("SDK_INT");

                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    var vib = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    if (vib != null && vib.Call<bool>("hasVibrator")) _vibrator = vib;
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[Haptics] Vibrator 서비스 해결 실패: " + e.Message); }
            return _vibrator;
        }
#endif
    }
}
