using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>
    /// 경로형 Arrow 하나의 표시와 연출 (GAME_RULES v0.6 §0): 경로 폴리라인(LineRenderer, 둥근 꺾임) + 머리 화살촉 + 머리 위 타입 아이콘.
    /// Fire 는 머리가 레인을 직진하고 몸통이 경로를 따라 뱀처럼 따라간다. BoardView 가 코드로 생성한다 (프리팹 없음). 판정은 하지 않는다.
    /// </summary>
    public sealed class ArrowView : MonoBehaviour
    {
        /// <summary>잠긴 Locked 흔들림 왕복 횟수.</summary>
        private const int ShakeCycles = 3;
        /// <summary>얼음 깨질 때 순간 확대 비율.</summary>
        private const float IcePunchScale = 1.2f;

        private readonly List<Vector3> _points = new List<Vector3>();

        private Arrow _arrow;
        private BoardLayout _layout;
        private ArrowViewStyle _style;
        private LineRenderer _line;
        private SpriteRenderer _head;
        private SpriteRenderer _icon;
        private Color _baseColor;
        private Vector3 _headDir;
        private float _headLength;
        private Vector3 _restPosition;
        private Vector3 _iconScale;
        /// <summary>경로 셀 중심 + 머리 앞으로 직진하는 연장 (월드 좌표). 뱀 이동은 이 폴리라인 위의 창(window)이다.</summary>
        private Vector3[] _extended;
        private bool _marked;
        private bool _preview;
        private Coroutine _motion;
        /// <summary>Bounce 중 현재 창 오프셋 (셀 단위). 정지 상태는 0.</summary>
        private float _currentOffset;

        public Arrow Arrow => _arrow;

        public void Initialize(Arrow arrow, BoardLayout layout, ArrowViewStyle style, int keyGroupIndex)
        {
            _arrow = arrow;
            _layout = layout;
            _style = style;
            _headDir = (Vector2)arrow.Direction.ToOffset();
            _headLength = layout.CellSize * style.ArrowHeadLengthCellRatio;
            _baseColor = style.LineColor;

            // 루트는 경로 중심 (등장 스케일의 기준점). 선·머리는 루트 기준 로컬 좌표.
            var center = Vector2.zero;
            foreach (var cell in arrow.Cells)
                center += layout.CellToWorld(cell);
            transform.position = center / arrow.Length;
            transform.rotation = Quaternion.identity;
            _restPosition = transform.position;

            // 보드 어느 위치에서 쏴도 꼬리까지 밖으로 나갈 만큼 연장
            var extra = Mathf.Max(layout.Width, layout.Height) + arrow.Length + 1;
            _extended = new Vector3[arrow.Length + extra];
            for (var i = 0; i < arrow.Length; i++)
                _extended[i] = layout.CellToWorld(arrow.Cells[i]);
            for (var k = 1; k <= extra; k++)
                _extended[arrow.Length - 1 + k] = _extended[arrow.Length - 1] + _headDir * (layout.Pitch * k);

            _line = gameObject.AddComponent<LineRenderer>();
            _line.useWorldSpace = false;
            _line.alignment = LineAlignment.TransformZ;
            _line.material = style.LineMaterial;
            _line.textureMode = LineTextureMode.Stretch;
            _line.startWidth = _line.endWidth = layout.CellSize * style.LineWidthCellRatio;
            _line.numCornerVertices = style.CornerVertices;
            _line.numCapVertices = style.CapVertices;
            _line.sortingOrder = style.LineOrder;

            _head = CreateChild("Head", style.HeadSprite, _baseColor, style.HeadOrder);
            _head.transform.localScale = new Vector3(layout.CellSize * style.ArrowHeadWidthCellRatio, _headLength, 1f);
            _head.transform.localRotation = Quaternion.Euler(0f, 0f, AngleOf(arrow.Direction));

            _iconScale = Vector3.one * (layout.CellSize * style.IconSizeCellRatio);
            _icon = CreateIcon(arrow, style, keyGroupIndex);

            ApplyColor();
            SetSnake(0f);
        }

        public void SetMarked(bool marked)
        {
            _marked = marked;
            ApplyColor();
        }

        /// <summary>길게 누르기 미리보기 중 선 색 강조.</summary>
        public void SetPreview(bool preview)
        {
            _preview = preview;
            ApplyColor();
        }

        public void SetLocked(bool locked)
        {
            if (_arrow.Type == ArrowType.Locked && _icon != null)
                _icon.enabled = locked;
        }

        /// <summary>
        /// Exit: 머리는 레인을 직진, 몸통은 경로를 따라. <paramref name="travelCells"/> 는 머리가 화면 밖으로
        /// 나가는 데 필요한 칸 수 (BoardView 가 카메라 기준으로 계산, W-025 1)이고, 여기에 꼬리까지 빠질
        /// 길이를 더해 달린 뒤 파괴한다.
        /// </summary>
        public void PlayFire(int travelCells, float cellsPerSecond, Action onComplete)
        {
            StartMotion(FireRoutine(travelCells + _arrow.Length + 1, cellsPerSecond, onComplete));
        }

        /// <summary>Block: 머리가 앞으로 살짝 밀렸다가 제자리 (GAME_RULES §2-3). 레인 번쩍은 LaneView 가 한다.</summary>
        public void PlayBounce(float distanceCells, float legDuration)
        {
            StartMotion(BounceRoutine(distanceCells, legDuration));
        }

        /// <summary>잠긴 Locked 탭: 진행 방향에 수직으로 흔들림.</summary>
        public void PlayShake(float distanceCells, float duration)
        {
            var side = new Vector3(-_headDir.y, _headDir.x, 0f);
            StartMotion(ShakeRoutine(side * (distanceCells * _layout.CellSize), duration));
        }

        /// <summary>Frozen 얼음 깨기: 남은 얼음만큼 알파를 줄이고 순간 확대.</summary>
        public void PlayIceBreak(int remainingHits, float duration)
        {
            if (_icon == null) return;
            var totalIceTaps = Mathf.Max(1, _arrow.Hits - Arrow.DefaultHits);
            var iceLeft = Mathf.Clamp01((remainingHits - Arrow.DefaultHits) / (float)totalIceTaps);
            var color = _style.IceColor;
            color.a *= iceLeft;
            _icon.color = color;
            StartCoroutine(PunchRoutine(_icon.transform, _iconScale, duration));
        }

        // ---- 뱀 폴리라인 ----

        /// <summary>_extended 위에서 [offset, offset + Length-1] 창을 잘라 선·머리를 놓는다 (셀 단위).</summary>
        private void SetSnake(float offset)
        {
            var start = offset;
            var end = offset + (_arrow.Length - 1);
            var headCenter = Sample(end);

            _points.Clear();
            _points.Add(_arrow.Length == 1 ? headCenter - _headDir * (_layout.CellSize * 0.5f) : Sample(start));
            for (var k = Mathf.FloorToInt(start) + 1; k < end; k++)
                _points.Add(Sample(k));   // 배열 끝을 넘어가면 Sample 이 직선으로 외삽한다
            _points.Add(headCenter - _headDir * (_headLength * 0.5f));

            _line.positionCount = _points.Count;
            for (var i = 0; i < _points.Count; i++)
                _line.SetPosition(i, transform.InverseTransformPoint(_points[i]));

            var headLocal = transform.InverseTransformPoint(headCenter);
            _head.transform.localPosition = headLocal;
            if (_icon != null)
                _icon.transform.localPosition = headLocal;
        }

        /// <summary>
        /// _extended 위의 위치 (셀 단위). 배열 끝을 넘어가면 머리 방향으로 <b>직선 외삽</b>한다 —
        /// Exit 연출이 화면 밖까지 가야 해서(W-025 1) 줌 아웃 상태에서는 배열보다 멀리 나갈 수 있다.
        /// </summary>
        private Vector3 Sample(float u)
        {
            var last = _extended.Length - 1;
            var index = Mathf.FloorToInt(u);
            if (index >= last)
                return _extended[last] + _headDir * ((u - last) * _layout.Pitch);
            if (index < 0)
                return _extended[0];
            return Vector3.Lerp(_extended[index], _extended[index + 1], u - index);
        }

        // ---- 연출 코루틴 ----

        private void StartMotion(IEnumerator routine)
        {
            if (_motion != null)
                StopCoroutine(_motion);
            transform.position = _restPosition;
            _currentOffset = 0f;
            SetSnake(0f);
            _motion = StartCoroutine(routine);
        }

        private IEnumerator FireRoutine(float travelCells, float cellsPerSecond, Action onComplete)
        {
            for (var offset = 0f; offset < travelCells; offset += cellsPerSecond * Time.deltaTime)
            {
                SetSnake(offset);
                yield return null;
            }
            onComplete?.Invoke();
            Destroy(gameObject);
        }

        private IEnumerator BounceRoutine(float distanceCells, float legDuration)
        {
            yield return SlideTo(distanceCells, legDuration);
            yield return SlideTo(0f, legDuration);
            _motion = null;
        }

        private IEnumerator SlideTo(float targetOffset, float duration)
        {
            var from = _currentOffset;
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                _currentOffset = Mathf.Lerp(from, targetOffset, Mathf.SmoothStep(0f, 1f, t / duration));
                SetSnake(_currentOffset);
                yield return null;
            }
            _currentOffset = targetOffset;
            SetSnake(targetOffset);
        }

        private IEnumerator ShakeRoutine(Vector3 amplitude, float duration)
        {
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                var progress = t / duration;
                var wave = Mathf.Sin(progress * Mathf.PI * 2f * ShakeCycles) * (1f - progress);
                transform.position = _restPosition + amplitude * wave;
                yield return null;
            }
            transform.position = _restPosition;
            _motion = null;
        }

        private static IEnumerator PunchRoutine(Transform target, Vector3 baseScale, float duration)
        {
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                target.localScale = baseScale * Mathf.Lerp(IcePunchScale, 1f, t / duration);
                yield return null;
            }
            target.localScale = baseScale;
        }

        // ---- 생성 헬퍼 ----

        private void ApplyColor()
        {
            var color = _preview ? _style.PreviewLineColor : _marked ? _style.MarkedColor : _baseColor;
            _line.startColor = _line.endColor = color;
            _head.color = color;
        }

        private SpriteRenderer CreateIcon(Arrow arrow, ArrowViewStyle style, int keyGroupIndex)
        {
            SpriteRenderer icon;
            switch (arrow.Type)
            {
                case ArrowType.Frozen:
                    icon = CreateChild("Ice", style.IceSprite, style.IceColor, style.IconOrder);
                    break;
                case ArrowType.Locked:
                    icon = CreateChild("Lock", style.LockSprite, style.GetKeyGroupColor(keyGroupIndex), style.IconOrder);
                    break;
                case ArrowType.Key:
                    icon = CreateChild("Key", style.KeySprite, style.GetKeyGroupColor(keyGroupIndex), style.IconOrder);
                    break;
                default:
                    return null;
            }
            icon.transform.localScale = _iconScale;
            return icon;
        }

        private SpriteRenderer CreateChild(string childName, Sprite sprite, Color color, int sortingOrder)
        {
            var child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        /// <summary>스프라이트의 +Y 가 Direction 을 향하도록 하는 Z 회전.</summary>
        private static float AngleOf(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return 0f;
                case Direction.Left: return 90f;
                case Direction.Down: return 180f;
                case Direction.Right: return 270f;
                default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
