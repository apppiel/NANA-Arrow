using System;
using UnityEngine;

namespace NanaArrow.UI
{
    /// <summary>문구 한 줄 (UI_FLOW §9 키 → 문구).</summary>
    [Serializable]
    public struct StringEntry
    {
        [SerializeField] private string key;
        [SerializeField, TextArea(1, 3)] private string text;

        public StringEntry(string key, string text)
        {
            this.key = key;
            this.text = text;
        }

        public string Key => key;
        public string Text => text;
    }
}
