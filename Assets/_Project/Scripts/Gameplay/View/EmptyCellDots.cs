using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>
    /// 보드 사각형 안의 빈 칸마다 찍는 옅은 점 (GAME_RULES v0.7.3 §9). 레인 가이드 토글과 무관하게 <b>항상</b> 보인다.
    /// Arrow 가 Exit 해서 칸이 비면 그 칸에도 점이 생기므로 <see cref="Rebuild"/> 를 다시 부른다. 보드 바깥엔 찍지 않는다.
    /// </summary>
    public sealed class EmptyCellDots : MonoBehaviour
    {
        private readonly List<SpriteRenderer> _dots = new List<SpriteRenderer>();

        private BoardLayout _layout;
        private ArrowViewStyle _style;

        public void Initialize(BoardLayout layout, ArrowViewStyle style)
        {
            _layout = layout;
            _style = style;
            gameObject.SetActive(style.ShowEmptyCellDots);
        }

        public void Rebuild(IEnumerable<Arrow> arrows)
        {
            if (_layout == null || !_style.ShowEmptyCellDots) return;

            var cells = LaneGuides.EmptyCells(_layout.Width, _layout.Height, arrows);
            while (_dots.Count < cells.Count) _dots.Add(CreateDot());

            var diameter = _layout.CellSize * _style.EmptyCellDotRadiusCellRatio * 2f;
            for (var i = 0; i < _dots.Count; i++)
            {
                var dot = _dots[i];
                if (i >= cells.Count)
                {
                    dot.enabled = false;
                    continue;
                }
                dot.transform.position = _layout.CellToWorld(cells[i]);
                dot.transform.localScale = Vector3.one * diameter;
                dot.color = _style.EmptyCellDotColor;
                dot.sortingOrder = _style.GuideOrder;
                dot.enabled = true;
            }
        }

        private SpriteRenderer CreateDot()
        {
            var go = new GameObject("Dot");
            go.transform.SetParent(transform, false);
            var dot = go.AddComponent<SpriteRenderer>();
            dot.sprite = PlaceholderSprites.Circle;
            return dot;
        }
    }
}
