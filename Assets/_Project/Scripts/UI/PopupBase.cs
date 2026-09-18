using UnityEngine;
using UnityEngine.Events;

namespace NanaArrow.UI
{
    /// <summary>
    /// 팝업 공통 (UI_FLOW §12-5): 열고 닫기, 한 번에 하나만 (<see cref="Current"/>), Dim 탭·뒤로가기로 닫을 수 있는지 옵션.
    /// 문구·버튼 배선은 파생/프리팹 몫. 열림 사운드는 AudioManager 가 있을 때만.
    /// <para>
    /// 닫힘은 <b>오브젝트 비활성 + CanvasGroup 숨김</b> 두 가지를 모두 쓴다 (W-025 4).
    /// 비활성만으로는 에디터에서 씬을 활성 상태로 저장했을 때 팝업이 겹쳐 보이고,
    /// CanvasGroup 만으로는 안 보이는 팝업이 계속 레이캐스트·업데이트를 먹는다.
    /// 씬에 어떤 상태로 저장돼 있든 <see cref="Open"/>/<see cref="Close"/> 가 상태를 확정한다.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PopupBase : MonoBehaviour
    {
        /// <summary>지금 열려 있는 팝업. 없으면 null.</summary>
        public static PopupBase Current { get; private set; }

        /// <summary>
        /// 팝업이 열린 채로 뒤로가기를 받은 프레임 (닫히지 않는 팝업 포함). 화면 쪽 핸들러가 같은 프레임에
        /// 이중 처리(팝업 닫힘 → 종료 확인 열림)하지 않도록 <see cref="ConsumedBack"/> 로 확인한다.
        /// </summary>
        private static int _backConsumedFrame = -1;

        /// <summary>이번 프레임 뒤로가기를 팝업이 처리했거나 팝업이 열려 있으면 true.</summary>
        public static bool ConsumedBack => Current != null || _backConsumedFrame == Time.frameCount;

        /// <summary>
        /// 씬의 모든 팝업을 닫힌 상태로 확정한다 (W-025 4). 씬을 팝업이 활성인 채로 저장했더라도
        /// 화면 진입 순간 겹쳐 보이지 않게 한다. 각 화면의 Start 에서 한 번 부른다.
        /// </summary>
        public static void CloseAll()
        {
            foreach (var popup in FindObjectsByType<PopupBase>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                popup.CloseImmediate();
        }

        [SerializeField, Tooltip("Dim 탭 또는 Android 뒤로가기로 닫기 허용 (실패·응모 코드 팝업은 끔)")]
        private bool closableByBack = true;
        [SerializeField] private UnityEvent opened = new UnityEvent();
        [SerializeField] private UnityEvent closed = new UnityEvent();

        private CanvasGroup _group;

        public bool IsOpen { get; private set; }
        public bool ClosableByBack => closableByBack;

        protected virtual void Awake()
        {
            EnsureGroup();
            SetVisible(false);
        }

        /// <summary>
        /// 뒤로가기는 <b>열려 있는 동안만</b> 구독한다. 닫힌 팝업은 비활성이라 Awake/OnDestroy 에 걸어 두면
        /// 아예 구독되지 않는다 (W-025 4·5 의 원인).
        /// </summary>
        protected virtual void OnEnable() => Core.BackButton.Pressed += OnBackPressed;

        protected virtual void OnDisable() => Core.BackButton.Pressed -= OnBackPressed;

        protected virtual void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        /// <summary>다른 팝업이 열려 있으면 먼저 닫는다 (UI_FLOW §2: 한 번에 하나).</summary>
        public void Open()
        {
            if (Current != null && Current != this) Current.Close();
            Current = this;
            IsOpen = true;
            // 씬에 비활성으로 저장돼 있어도 여기서 살아난다. SetActive 는 동기라 이 줄에서 Awake 가 끝난다.
            gameObject.SetActive(true);
            EnsureGroup();
            SetVisible(true);
            transform.SetAsLastSibling();
            Core.AudioManager.Instance?.Play(Core.SoundId.SfxPopup);
            OnOpened();
            opened.Invoke();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                // 씬에 활성으로 저장된 팝업을 처음부터 확실히 숨기는 경로 (GameController.Start 가 호출).
                CloseImmediate();
                return;
            }
            IsOpen = false;
            SetVisible(false);
            if (Current == this) Current = null;
            OnClosed();
            closed.Invoke();
            gameObject.SetActive(false);
        }

        /// <summary>열린 적 없는 팝업을 상태 변화 없이 숨긴다 (씬 저장 상태 보정용).</summary>
        public void CloseImmediate()
        {
            if (Current == this) Current = null;
            IsOpen = false;
            if (gameObject.activeSelf)
            {
                EnsureGroup();
                SetVisible(false);
                gameObject.SetActive(false);
            }
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
            if (!IsOpen || Current != this) return;
            _backConsumedFrame = Time.frameCount;
            CloseIfAllowed();
        }

        private void EnsureGroup()
        {
            if (_group == null) _group = GetComponent<CanvasGroup>();
        }

        private void SetVisible(bool visible)
        {
            EnsureGroup();
            _group.alpha = visible ? 1f : 0f;
            _group.interactable = visible;
            _group.blocksRaycasts = visible;
        }
    }
}
