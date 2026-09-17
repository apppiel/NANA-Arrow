using TMPro;
using UnityEngine;

namespace NanaArrow.UI
{
    /// <summary>TMP 텍스트에 문구 키를 붙여 두면 활성화될 때 Strings 에서 채운다 (고정 문구용. 인자가 있는 문구는 각 화면 스크립트가 Format).</summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private Strings strings;
        [SerializeField, Tooltip("UI_FLOW §9 키 (예: main.start)")] private string key;

        private void OnEnable() => Apply();

        public void SetKey(string newKey)
        {
            key = newKey;
            Apply();
        }

        private void Apply()
        {
            if (strings == null || string.IsNullOrEmpty(key)) return;
            GetComponent<TMP_Text>().text = strings.Get(key);
        }
    }
}
