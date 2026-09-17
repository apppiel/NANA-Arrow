using System;
using System.Collections;
using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>Arrow 하나의 표시와 연출. BoardView 가 코드로 생성한다 (프리팹 없음). 판정은 하지 않는다.</summary>
    public sealed class ArrowView : MonoBehaviour
    {
        /// <summary>잠긴 Locked 흔들림 왕복 횟수.</summary>
        private const int ShakeCycles = 3;
        /// <summary>얼음 깨질 때 순간 확대 비율.</summary>
        private const float IcePunchScale = 1.15f;

        private Arrow _arrow;
        private BoardLayout _layout;
        private ArrowViewStyle _style;
        private SpriteRenderer _body;
        private SpriteRenderer _head;
        private SpriteRenderer _ice;
        private SpriteRenderer _lock;
        private Color _bodyColor;
        private Vector3 _restPosition;
        private Vector3 _iceScale;
        private float _bodyLength;
        private Coroutine _motion;

        public Arrow Arrow => _arrow;

        public void Initialize(Arrow arrow, BoardLayout layout, ArrowViewStyle style, int keyGroupIndex)
        {
            _arrow = arrow;
            _layout = layout;
            _style = style;

            var center = Vector2.zero;
            foreach (var cell in arrow.Cells)
                center += layout.CellToWorld(cell);
            center /= arrow.Cells.Count;

            var angle = AngleOf(arrow.Direction);
            transform.position = center;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            _restPosition = transform.position;

            var cells = arrow.Cells.Count;
            var inset = layout.CellSize * style.BodyInset;
            var fullLength = cells * layout.CellSize + (cells - 1) * layout.CellGap;
            _bodyLength = fullLength - inset * 2f;
            var thickness = layout.CellSize - inset * 2f;

            _bodyColor = ColorFor(arrow, style, keyGroupIndex);
            _body = CreateChild("Body", style.BodySprite, _bodyColor, style.BodyOrder);
            _body.transform.localScale = new Vector3(thickness, _bodyLength, 1f);

            var headLength = layout.CellSize * style.HeadLength;
            _head = CreateChild("Head", style.HeadSprite, _bodyColor * style.HeadTint, style.HeadOrder);
            _head.transform.localScale = new Vector3(layout.CellSize * style.HeadWidth, headLength, 1f);
            _head.transform.localPosition = new Vector3(0f, (_bodyLength - headLength) * 0.5f, 0f);

            _iceScale = new Vector3(layout.CellSize, fullLength, 1f);
            _ice = CreateChild("Ice", style.IceSprite, style.IceColor, style.OverlayOrder);
            _ice.transform.localScale = _iceScale;
            _ice.enabled = arrow.Type == ArrowType.Frozen;

            _lock = CreateChild("Lock", style.LockSprite, style.LockColor, style.OverlayOrder);
            _lock.transform.localScale = Vector3.one * (layout.CellSize * style.LockSize);
            _lock.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            _lock.enabled = false;
        }

        public void SetMarked(bool marked)
        {
            var color = marked ? _style.MarkedColor : _bodyColor;
            _body.color = color;
            _head.color = color * _style.HeadTint;
        }

        public void SetLocked(bool locked) => _lock.enabled = locked;

        /// <summary>Exit: 꼬리까지 보드 밖으로 날아간 뒤 파괴.</summary>
        public void PlayFire(int freeCells, float duration, Action onComplete)
        {
            var distance = freeCells * _layout.Pitch + _bodyLength + _layout.CellSize;
            StartMotion(FireRoutine(transform.up * distance, duration, onComplete));
        }

        /// <summary>Block: 막은 Arrow 직전까지 밀렸다가 (+bounceDistance) 원위치 (GAME_RULES §2-3).</summary>
        public void PlayBounce(int freeCells, float bounceDistanceCells, float legDuration)
        {
            var distance = freeCells * _layout.Pitch + bounceDistanceCells * _layout.CellSize;
            StartMotion(BounceRoutine(transform.up * distance, legDuration));
        }

        /// <summary>잠긴 Locked 탭: 진행 방향에 수직으로 흔들림.</summary>
        public void PlayShake(float distanceCells, float duration)
        {
            StartMotion(ShakeRoutine(transform.right * (distanceCells * _layout.CellSize), duration));
        }

        /// <summary>Frozen 얼음 깨기: 남은 얼음만큼 알파를 줄이고 순간 확대.</summary>
        public void PlayIceBreak(int remainingHits, float duration)
        {
            var totalIceTaps = Mathf.Max(1, _arrow.Hits - Arrow.DefaultHits);
            var iceLeft = Mathf.Clamp01((remainingHits - Arrow.DefaultHits) / (float)totalIceTaps);
            var color = _style.IceColor;
            color.a *= iceLeft;
            _ice.color = color;
            StartCoroutine(PunchRoutine(_ice.transform, _iceScale, duration));
        }

        private void StartMotion(IEnumerator routine)
        {
            if (_motion != null)
                StopCoroutine(_motion);
            transform.position = _restPosition;
            _motion = StartCoroutine(routine);
        }

        private IEnumerator FireRoutine(Vector3 delta, float duration, Action onComplete)
        {
            yield return MoveBy(delta, duration);
            onComplete?.Invoke();
            Destroy(gameObject);
        }

        private IEnumerator BounceRoutine(Vector3 delta, float legDuration)
        {
            yield return MoveBy(delta, legDuration);
            yield return MoveBy(-delta, legDuration);
            _motion = null;
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

        private IEnumerator MoveBy(Vector3 delta, float duration)
        {
            var from = transform.position;
            var to = from + delta;
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            transform.position = to;
        }

        private static IEnumerator PunchRoutine(Transform target, Vector3 baseScale, float duration)
        {
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                var punch = Mathf.Lerp(IcePunchScale, 1f, t / duration);
                target.localScale = baseScale * punch;
                yield return null;
            }
            target.localScale = baseScale;
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

        private static Color ColorFor(Arrow arrow, ArrowViewStyle style, int keyGroupIndex)
        {
            switch (arrow.Type)
            {
                case ArrowType.Frozen: return style.FrozenColor;
                case ArrowType.Key:
                case ArrowType.Locked: return style.GetKeyGroupColor(keyGroupIndex);
                default: return style.BasicColor;
            }
        }

        /// <summary>로컬 +Y 가 Direction 을 향하도록 하는 Z 회전.</summary>
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
