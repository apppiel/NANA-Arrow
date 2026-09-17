using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NanaArrow.Core
{
    /// <summary>
    /// Android 뒤로가기 (NO.2 BackButtonHandler 의 입력 부분만). Input System 은 뒤로가기를 Keyboard.escapeKey 로 매핑한다.
    /// 무엇을 할지는 구독자(UI) 가 정한다 (UI_FLOW §2 표). 첫 씬 로드 전에 스스로 생성.
    /// </summary>
    public sealed class BackButton : MonoBehaviour
    {
        public static event Action Pressed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject(nameof(BackButton));
            DontDestroyOnLoad(go);
            go.AddComponent<BackButton>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                Pressed?.Invoke();
        }
    }
}
