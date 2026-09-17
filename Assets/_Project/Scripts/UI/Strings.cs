using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.UI
{
    /// <summary>
    /// 문구 표 (UI_FLOW §9). 코드는 키만 참조하고 문구는 이 에셋(Settings/Strings_ko.asset)에 둔다 (§2, 영어 추가 대비).
    /// 없는 키는 키 문자열 그대로 돌려줘서 빠진 문구가 화면에 보이게 한다.
    /// </summary>
    [CreateAssetMenu(menuName = "NanaArrow/Strings", fileName = "Strings_ko")]
    public sealed class Strings : ScriptableObject
    {
        [SerializeField] private StringEntry[] entries = new StringEntry[0];

        private Dictionary<string, string> _map;

        public int Count => entries.Length;

        public string Get(string key)
        {
            if (_map == null || _map.Count != entries.Length) Rebuild();
            return key != null && _map.TryGetValue(key, out var text) ? text : key;
        }

        /// <summary>`{0}` 자리표시자 치환 (main.level_sub, clear.level).</summary>
        public string Format(string key, params object[] args) => string.Format(Get(key), args);

        /// <summary>에디터·테스트용 일괄 설정.</summary>
        public void SetEntries(StringEntry[] items)
        {
            entries = items ?? new StringEntry[0];
            _map = null;
        }

        private void OnValidate() => _map = null;

        private void Rebuild()
        {
            _map = new Dictionary<string, string>(entries.Length);
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Key)) continue;
                _map[entry.Key] = entry.Text;
            }
        }
    }
}
