using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine.UI;
#endif

namespace NanaArrow.Services
{
    /// <summary>
    /// 캡처 방지 (NO.3 ScreenCaptureProtection 이식). 첫 씬 로드 전에 스스로 생성.
    /// Android: FLAG_SECURE — 스크린샷·녹화·화면 공유 차단 (개발 빌드에도 걸림 → 폰 테스트 화면은 다른 기기로 촬영).
    /// iOS: 스크린샷은 못 막고 녹화·미러링만 검은 오버레이 (Plugins/iOS 브릿지 필요, v1 대상 아님).
    /// 에디터: 아무것도 하지 않음.
    /// </summary>
    public sealed class ScreenCaptureProtection : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject(nameof(ScreenCaptureProtection));
            DontDestroyOnLoad(go);
            go.AddComponent<ScreenCaptureProtection>();
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool _IsScreenBeingCaptured();

        private Canvas _overlayCanvas;
        private bool _overlayVisible;
#endif

        private void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            SetAndroidSecureFlag();
#endif
#if UNITY_IOS && !UNITY_EDITOR
            BuildOverlay();
            UpdateOverlay(_IsScreenBeingCaptured());
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        private void Update()
        {
            var captured = _IsScreenBeingCaptured();
            if (captured != _overlayVisible) UpdateOverlay(captured);
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        private void SetAndroidSecureFlag()
        {
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                // Window 조작은 UI 스레드에서. 람다 안에서 activity 를 다시 얻는다 (바깥 것은 반환 시 dispose).
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    try
                    {
                        using var innerPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                        using var innerActivity = innerPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                        using var window = innerActivity.Call<AndroidJavaObject>("getWindow");
                        const int FLAG_SECURE = 0x2000; // WindowManager.LayoutParams.FLAG_SECURE
                        window.Call("setFlags", FLAG_SECURE, FLAG_SECURE);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("[ScreenCaptureProtection] FLAG_SECURE 적용 실패: " + e.Message);
                    }
                }));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ScreenCaptureProtection] Activity 접근 실패: " + e.Message);
            }
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        private void BuildOverlay()
        {
            var canvasGo = new GameObject("CaptureOverlayCanvas");
            canvasGo.transform.SetParent(transform);
            _overlayCanvas = canvasGo.AddComponent<Canvas>();
            _overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _overlayCanvas.sortingOrder = 32767;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var imageGo = new GameObject("BlackFill");
            imageGo.transform.SetParent(canvasGo.transform, false);
            var img = imageGo.AddComponent<Image>();
            img.color = Color.black;
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _overlayCanvas.gameObject.SetActive(false);
        }

        private void UpdateOverlay(bool show)
        {
            _overlayVisible = show;
            if (_overlayCanvas != null) _overlayCanvas.gameObject.SetActive(show);
        }
#endif
    }
}
