using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>
    /// 셀 경계를 옅은 선으로 그리는 격자 (GAME_RULES v0.7.2 §9 — 기본 꺼짐, 우하단 `#` 버튼으로 토글).
    /// BoardView 가 레벨마다 하나 만들고, 보이기/숨기기만 바뀐다. 판정에는 관여하지 않는 순수 표시용.
    /// </summary>
    public sealed class GridOverlay : MonoBehaviour
    {
        /// <summary>세로 (Width + 1) + 가로 (Height + 1) 개의 선을 만든다.</summary>
        public void Build(BoardLayout layout, ArrowViewStyle style)
        {
            var min = layout.GridMin;
            var max = layout.GridMax;
            var width = layout.CellSize * style.GridLineWidthCellRatio;

            for (var x = 0; x <= layout.Width; x++)
            {
                var lineX = min.x + x * layout.Pitch;
                AddLine(style, width, new Vector3(lineX, min.y), new Vector3(lineX, max.y));
            }

            for (var y = 0; y <= layout.Height; y++)
            {
                var lineY = min.y + y * layout.Pitch;
                AddLine(style, width, new Vector3(min.x, lineY), new Vector3(max.x, lineY));
            }
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        private void AddLine(ArrowViewStyle style, float width, Vector3 from, Vector3 to)
        {
            var line = new GameObject("GridLine").AddComponent<LineRenderer>();
            line.transform.SetParent(transform, false);
            line.useWorldSpace = true;
            line.alignment = LineAlignment.TransformZ;
            line.material = style.LineMaterial;
            line.numCapVertices = 0;
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = style.GridColor;
            line.sortingOrder = style.GridOrder;
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
        }
    }
}
