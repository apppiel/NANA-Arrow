using UnityEngine;
using UnityEngine.Events;

namespace NanaArrow.UI
{
    /// <summary>
    /// 팝업 공통 (UI_FLOW §12-5): CanvasGroup 으로 열고 닫기, 한 번에 하나만 (<see cref="Current"/>), Dim 탭·뒤로가기로 닫을 수 있는지 옵션.
    /// 문구·버튼 배선은 파생/프리팹 몫. 열림 사운드는 AudioManager 가 있을 때만.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PopupBase : MonoBehaviour
    {
        /// <summary>지금 열려 있는 팝업. 없으면 null.</summary>
        public static PopupBase Current { get; private set; }

        [SerializeField, Tooltip("Dim 탭 또는 Android 뒤로가기로 닫기 허용 (실패·응모 코드 팝업은 끔)")]
        private bool closableByBack = true;
        [SerializeField] private UnityEvent opened = new UnityEvent();
        [SerializeField] private UnityEvent closed = new UnityEvent();

        private CanvasGroup _group;

        public bool IsOpen { get; private set; }
        public bool ClosableByBack => closableByBack;

        protected virtual void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            Core.BackButton.Pressed += OnBackPressed;
            SetVisible(false);
        }

        protected virtual void OnDestroy()
        {
            Core.BackButton.Pressed -= OnBackPressed;
            if (Current == this) Current = null;
        }

        /// <summary>다른 팝업이 열려 있으면 먼저 닫는다 (UI_FLOW §2: 한 번에 하나).</summary>
        public void Open()
        {
            if (Current != null && Current != this) Current.Close();
            Current = this;
            IsOpen = true;
            SetVisible(true);
            transform.SetAsLastSibling();
            Core.AudioManager.Instance?.Play(Core.SoundId.SfxPopup);
            OnOpened();
            opened.Invoke();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            SetVisible(false);
            if (Current == this) Current = null;
            OnClosed();
            closed.Invoke();
        }

        /// <summary>Dim 클릭용 (Button.onClick 에 연결).</summary>
        public void CloseIfAllowed()
        {
            if (closableByBack) Close();
        }

        protected virtual void OnOpened() { }
        protected virtual void OnClosed() { }

        private void OnBackPressed()
        {
            if (IsOpen && Current == this) CloseIfAllowed();
        }

        private void SetVisible(bool visible)
        {
            _group.alpha = visible ? 1f : 0f;
            _group.interactable = visible;
            _group.blocksRaycasts = visible;
        }
    }
}
