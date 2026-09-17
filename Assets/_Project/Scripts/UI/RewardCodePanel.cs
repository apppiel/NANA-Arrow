using NanaArrow.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NanaArrow.UI
{
    /// <summary>
    /// 응모 코드 팝업 (UI_FLOW §6-5, NO.3 RewardCodePanel 이식). 발급은 <see cref="App.RewardCodes"/> 에 등록된 서비스가 하고 여기는 그리기만.
    /// 복사 버튼이 필수 — 캡처 방지(FLAG_SECURE) 때문에 코드를 밖으로 빼낼 유일한 수단. 프리팹에서 Closable By Back 끔.
    /// Game(응모 레벨 클리어 시) 과 Main(다시 보기) 에 각 1개.
    /// </summary>
    public sealed class RewardCodePanel : PopupBase
    {
        [SerializeField] private Strings strings;
        [SerializeField] private RewardConfig config;
        [SerializeField, Tooltip("CodeText — XXXX-XXXX 크게, 자간 넓게")] private TMP_Text codeText;
        [SerializeField, Tooltip("StatusText — 발급 진행·복사 안내 (비워도 됨)")] private TMP_Text statusText;
        [SerializeField, Tooltip("CopyButton")] private Button copyButton;
        [SerializeField, Tooltip("GoButton — 상품 받으러 가기")] private Button goButton;
        [SerializeField, Tooltip("코드가 아직 없을 때 자리값")] private string placeholder = "————";

        private const string KeyCopied = "raffle.copied";
        private const string KeyStatusPrefix = "raffle.status.";

        private string _code = "";
        private PopupBase _next;

        protected override void Awake()
        {
            base.Awake();
            App.RewardCodeIssued += OnCodeIssued;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            App.RewardCodeIssued -= OnCodeIssued;
        }

        /// <summary>이 레벨을 깨면 발급하나 (응모 레벨 이상 + 아직 미발급).</summary>
        public bool ShouldIssueFor(int level) =>
            config != null && level >= config.RewardLevel && !App.Progress.RewardCodeIssued;

        /// <summary>열고, 닫히면 <paramref name="next"/> 를 연다 (클리어 팝업보다 먼저 보여 줄 때).</summary>
        public void OpenThen(PopupBase next)
        {
            _next = next;
            Open();
        }

        protected override void OnOpened()
        {
            SetButtons();
            if (codeText != null && string.IsNullOrEmpty(_code)) codeText.text = placeholder;
            if (App.RewardCodes != null) App.RewardCodes.IssueCode();
            else if (statusText != null) statusText.text = strings.Get(KeyStatusPrefix + SnakeCase(RewardCodeStatus.Offline));
        }

        protected override void OnClosed()
        {
            var next = _next;
            _next = null;
            if (next != null) next.Open();
        }

        /// <summary>CopyButton.onClick</summary>
        public void CopyCode()
        {
            if (string.IsNullOrEmpty(_code)) return;
            GUIUtility.systemCopyBuffer = _code;
            if (statusText != null) statusText.text = strings.Get(KeyCopied);
            GameEvents.RaiseRewardCodeCopied();
        }

        /// <summary>GoButton.onClick</summary>
        public void OpenClaimPage()
        {
            if (config == null || string.IsNullOrEmpty(config.ClaimUrl)) return;
            GameEvents.RaiseRewardLinkOpened();
            Application.OpenURL(config.ClaimUrl);
        }

        private void OnCodeIssued(string code, RewardCodeStatus status)
        {
            if (!string.IsNullOrEmpty(code))
            {
                _code = code;
                if (codeText != null) codeText.text = code;
                SetButtons();
            }
            if (statusText != null) statusText.text = strings.Get(KeyStatusPrefix + SnakeCase(status));
        }

        private void SetButtons()
        {
            var has = !string.IsNullOrEmpty(_code);
            if (copyButton != null) copyButton.interactable = has;
            if (goButton != null) goButton.interactable = has;
        }

        private static string SnakeCase(RewardCodeStatus status)
        {
            switch (status)
            {
                case RewardCodeStatus.SaveFailed: return "save_failed";
                default: return status.ToString().ToLowerInvariant();
            }
        }
    }
}
