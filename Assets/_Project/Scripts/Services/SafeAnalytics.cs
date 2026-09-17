using Firebase.Analytics;
using UnityEngine;

namespace NanaArrow.Services
{
    /// <summary>
    /// Firebase Analytics 안전 래퍼 (NO.3 SafeAnalytics 이식). 네이티브가 없는 환경(에디터)에서 LogEvent 를 직접 부르면 DllNotFoundException 이
    /// 호출 스택을 끊어 뒤따르는 게임 로직이 안 돈다 — 여기서 삼키고 경고만 남긴다. FirebaseAnalytics 는 반드시 이 클래스를 경유할 것.
    /// </summary>
    public static class SafeAnalytics
    {
        public static void LogEvent(string name, params Parameter[] parameters)
        {
            try { FirebaseAnalytics.LogEvent(name, parameters); }
            catch (System.Exception e) { Debug.LogWarning($"[Analytics] {name} 건너뜀: {e.Message}"); }
        }

        public static void SetUserProperty(string name, string value)
        {
            try { FirebaseAnalytics.SetUserProperty(name, value); }
            catch (System.Exception e) { Debug.LogWarning($"[Analytics] property {name} 건너뜀: {e.Message}"); }
        }
    }
}
