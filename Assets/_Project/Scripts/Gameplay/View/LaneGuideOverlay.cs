using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>
    /// 레인 가이드 (GAME_RULES v0.7.3 §9, 우하단 `#` 토글). 남아 있는 Arrow 마다 머리가 있는 행(가로 Arrow) 또는
    /// 열(세로 Arrow)에 화면 끝에서 끝까지 옅은 선을 긋는다. 표 격자가 아니다 — Arrow 가 없는 행·열엔 선이 없다.
    /// Arrow 가 Exit 하면 <see cref="Rebuild"/> 로 그 선도 사라진다. 선은 월드 공간이라 줌·팬에 보드와 함께 움직인다.
    /// </summary>
    public sealed class LaneGuideOverlay : MonoBehaviour
    {
        private readonly List<LineRenderer> _lines = new List<LineRenderer>();

        private BoardLayout _layout;
        private ArrowViewStyle _style;
        private float _halfSpan;
        private bool _visible;

        /// <summary>선 하나의 절반 길이 (월드). 줌 아웃·팬을 해도 화면을 덮도록 넉넉히 잡는다.</summary>
        public void Initialize(BoardLayout layout, ArrowViewStyle style, float halfSpan)
        {
            _layout = layout;
            _style = style;
            _halfSpan = halfSpan;
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            gameObject.SetActive(visible);
        }

        /// <summary>남아 있는 Arrow 기준으로 선을 다시 만든다.</summary>
        public void Rebuild(IEnumerable<Arrow> arrows)
        {
            if (_layout == null) return;

            var wanted = LaneGuides.For(arrows);
            while (_lines.Count < wanted.Count) _lines.Add(CreateLine());

            for (var i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                if (i >= wanted.Count)
                {
                    line.enabled = false;
                    continue;
                }
                var guide = wanted[i];
                // 선이 지나는 한 점: 그 행·열의 셀 중심
                var through = guide.Horizontal
                    ? _layout.CellToWorld(new Vector2Int(0, guide.Index))
                    : _layout.CellToWorld(new Vector2Int(guide.Index, 0));
                var axis = guide.Horizontal ? Vector2.right : Vector2.up;
                var center = guide.Horizontal
                    ? new Vector2(_layout.Center.x, through.y)
                    : new Vector2(through.x, _layout.Center.y);

                line.startWidth = line.endWidth = _layout.CellSize * _style.LaneGuideWidthCellRatio;
                line.startColor = line.endColor = _style.LaneGuideColor;
                line.sortingOrder = _style.GuideOrder;
                line.SetPosition(0, center - axis * _halfSpan);
                line.SetPosition(1, center + axis * _halfSpan);
                line.enabled = true;
            }
            gameObject.SetActive(_visible);
        }

        private LineRenderer CreateLine()
        {
            var go = new GameObject("LaneGuide");
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.alignment = LineAlignment.TransformZ;
            line.material = _style.LineMaterial;
            line.numCapVertices = 0;
            line.numCornerVertices = 0;
            line.positionCount = 2;
            return line;
        }
    }
}
