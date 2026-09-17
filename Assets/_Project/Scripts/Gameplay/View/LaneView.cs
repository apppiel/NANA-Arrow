using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>머리 앞 레인을 굵은 반투명 선으로 표시 (Block 번쩍 / 길게 누르기 미리보기). BoardView 가 하나만 만들어 재사용한다.</summary>
    public sealed class LaneView : MonoBehaviour
    {
        private LineRenderer _line;
        private Coroutine _flash;

        public void Initialize(ArrowViewStyle style, BoardLayout layout)
        {
            _line = gameObject.AddComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.alignment = LineAlignment.TransformZ;
            _line.material = style.LineMaterial;
            _line.numCapVertices = style.CapVertices;
            _line.startWidth = _line.endWidth = layout.CellSize * style.LaneWidthCellRatio;
            _line.sortingOrder = style.LaneOrder;
            _line.positionCount = 0;
            _line.enabled = false;
        }

        /// <summary>머리 중심에서 레인 끝까지. 레인이 비어 있으면 머리 칸 가장자리까지 짧게.</summary>
        public void Show(Arrow arrow, IReadOnlyList<Vector2Int> lane, BoardLayout layout, Color color)
        {
            StopFlash();
            var from = (Vector3)layout.CellToWorld(arrow.Head);
            var to = lane.Count > 0
                ? (Vector3)layout.CellToWorld(lane[lane.Count - 1])
                : from + (Vector3)(Vector2)arrow.Direction.ToOffset() * (layout.CellSize * 0.5f);

            _line.positionCount = 2;
            _line.SetPosition(0, from);
            _line.SetPosition(1, to);
            _line.startColor = _line.endColor = color;
            _line.enabled = true;
        }

        public void Hide()
        {
            StopFlash();
            _line.enabled = false;
        }

        /// <summary>Block: 표시 후 duration 동안 페이드아웃.</summary>
        public void Flash(Arrow arrow, IReadOnlyList<Vector2Int> lane, BoardLayout layout, Color color, float duration)
        {
            Show(arrow, lane, layout, color);
            _flash = StartCoroutine(FadeOut(color, duration));
        }

        private IEnumerator FadeOut(Color color, float duration)
        {
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                var c = color;
                c.a = color.a * (1f - t / duration);
                _line.startColor = _line.endColor = c;
                yield return null;
            }
            _line.enabled = false;
            _flash = null;
        }

        private void StopFlash()
        {
            if (_flash == null) return;
            StopCoroutine(_flash);
            _flash = null;
        }
    }
}
