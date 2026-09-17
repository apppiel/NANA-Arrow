using System.Collections;
using NanaArrow.Core;
using NanaArrow.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace NanaArrow.UI.Game
{
    /// <summary>
    /// HUD 하트 (UI_FLOW §5, §12-3): 세션의 LivesTracker 를 따라 가득/빈 하트를 표시.
    /// 감소 → 그 하트가 흔들린 뒤 빈 하트 + 진동(설정 켜짐일 때). 회복(이어하기·치트) → 채워지는 팝 연출.
    /// </summary>
    public sealed class LivesView : MonoBehaviour
    {
        [SerializeField] private GameController gameController;
        [SerializeField, Tooltip("Heart_0 ~ Heart_2 (왼쪽부터)")] private Image[] hearts = new Image[0];
        [SerializeField] private Sprite fullSprite;
        [SerializeField] private Sprite emptySprite;

        [Header("연출")]
        [SerializeField, Min(0f), Tooltip("감소 시 흔들림 (초)")] private float shakeDuration = 0.3f;
        [SerializeField, Min(0f), Tooltip("흔들림 폭 (캔버스 px)")] private float shakeDistance = 10f;
        [SerializeField, Min(0f), Tooltip("회복 시 커졌다 돌아오는 시간 (초)")] private float refillDuration = 0.25f;
        [SerializeField, Min(1f), Tooltip("회복 시 최대 배율")] private float refillScale = 1.3f;
        [SerializeField, Tooltip("하트 감소 시 진동 (GAME_RULES §10 vibrateOnLifeLost, 설정의 진동이 켜져 있을 때만)")]
        private bool vibrateOnLifeLost = true;
        [SerializeField, Range(0f, 1f), Tooltip("빈 하트 스프라이트가 없을 때 빈 하트의 알파")] private float emptyAlpha = 0.3f;

        private LivesTracker _lives;
        private int _shown;

        private void Awake()
        {
            gameController.SessionStarted += OnSessionStarted;
        }

        private void OnDestroy()
        {
            gameController.SessionStarted -= OnSessionStarted;
            Unbind();
        }

        private void OnSessionStarted(GameSession session)
        {
            Unbind();
            StopAllCoroutines();
            _lives = session.Lives;
            _lives.LivesChanged += OnLivesChanged;
            _shown = _lives.Lives;
            for (var i = 0; i < hearts.Length; i++)
            {
                hearts[i].gameObject.SetActive(i < _lives.MaxLives);
                SetFull(i, i < _shown);
                hearts[i].rectTransform.localScale = Vector3.one;
            }
        }

        private void OnLivesChanged(int lives)
        {
            if (lives < _shown)
            {
                for (var i = lives; i < _shown; i++) StartCoroutine(Lose(i));
                if (vibrateOnLifeLost) Haptics.LifeLost();
                AudioManager.Instance?.Play(SoundId.SfxLifeLost);
            }
            else
            {
                for (var i = _shown; i < lives; i++) StartCoroutine(Refill(i));
                AudioManager.Instance?.Play(SoundId.SfxHeartRestore);
            }
            _shown = lives;
        }

        private IEnumerator Lose(int index)
        {
            if (index >= hearts.Length) yield break;
            var rect = hearts[index].rectTransform;
            var origin = rect.anchoredPosition;
            for (var t = 0f; t < shakeDuration; t += Time.deltaTime)
            {
                var k = 1f - t / shakeDuration;
                rect.anchoredPosition = origin + Vector2.right * (Mathf.Sin(t / shakeDuration * Mathf.PI * 4f) * shakeDistance * k);
                yield return null;
            }
            rect.anchoredPosition = origin;
            SetFull(index, false);
        }

        private IEnumerator Refill(int index)
        {
            if (index >= hearts.Length) yield break;
            var rect = hearts[index].rectTransform;
            SetFull(index, true);
            for (var t = 0f; t < refillDuration; t += Time.deltaTime)
            {
                var k = Mathf.Sin(t / refillDuration * Mathf.PI);
                rect.localScale = Vector3.one * Mathf.Lerp(1f, refillScale, k);
                yield return null;
            }
            rect.localScale = Vector3.one;
        }

        private void SetFull(int index, bool full)
        {
            if (index >= hearts.Length) return;
            var sprite = full ? fullSprite : emptySprite;
            if (sprite != null)
            {
                hearts[index].sprite = sprite;
                return;
            }
            var color = hearts[index].color;
            color.a = full ? 1f : emptyAlpha;
            hearts[index].color = color;
        }

        private void Unbind()
        {
            if (_lives == null) return;
            _lives.LivesChanged -= OnLivesChanged;
            _lives = null;
        }
    }
}
