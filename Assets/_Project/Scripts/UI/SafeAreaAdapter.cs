using UnityEngine;

namespace NanaArrow.UI
{
    /// <summary>Canvas 안의 패널을 Screen.safeArea 에 맞춘다 (NO.2 SafeAreaAdapter 이식). 노치·홈 인디케이터를 피한다.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaAdapter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _applied;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_rect != null && Screen.safeArea != _applied) Apply();
        }

        private void Apply()
        {
            var safe = Screen.safeArea;
            _applied = safe;

            var anchorMin = safe.position;
            var anchorMax = safe.position + safe.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
