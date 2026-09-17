using System.Collections.Generic;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>
    /// BGM 1채널 + 효과음 (NO.2 AudioManager 뼈대). Boot 씬 오브젝트에 붙이고 DontDestroyOnLoad. 클립은 인스펙터에서 SoundId 별로 연결 (비우면 무음).
    /// 사운드 토글 하나로 BGM·효과음 동시 on/off (UI_FLOW §10).
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField, Tooltip("UI_FLOW §10 의 13종. 없는 항목은 무음")] private SoundClip[] clips;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.6f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

        private readonly Dictionary<SoundId, SoundClip> _byId = new Dictionary<SoundId, SoundClip>();
        private AudioSource _bgm;
        private AudioSource _sfx;
        private SoundId? _currentBgm;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _bgm = gameObject.AddComponent<AudioSource>();
            _bgm.loop = true;
            _bgm.playOnAwake = false;
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;

            if (clips != null)
                foreach (var clip in clips)
                    _byId[clip.Id] = clip;

            SettingsStore.SoundChanged += OnSoundChanged;
            ApplySettings();
        }

        private void OnDestroy()
        {
            SettingsStore.SoundChanged -= OnSoundChanged;
            if (Instance == this) Instance = null;
        }

        public void Play(SoundId id)
        {
            if (!SettingsStore.SoundOn || !_byId.TryGetValue(id, out var sound) || sound.Clip == null) return;
            _sfx.PlayOneShot(sound.Clip, sound.Volume * sfxVolume);
        }

        /// <summary>같은 BGM 이 이미 나오면 유지.</summary>
        public void PlayBgm(SoundId id)
        {
            if (_currentBgm == id && _bgm.isPlaying) return;
            _currentBgm = id;
            if (!_byId.TryGetValue(id, out var sound) || sound.Clip == null)
            {
                _bgm.Stop();
                return;
            }
            _bgm.clip = sound.Clip;
            _bgm.volume = sound.Volume * bgmVolume;
            _bgm.mute = !SettingsStore.SoundOn;
            _bgm.Play();
        }

        public void StopBgm()
        {
            _currentBgm = null;
            _bgm.Stop();
        }

        /// <summary>광고 재생 중 BGM 일시정지 (UI_FLOW §8).</summary>
        public void PauseBgm(bool pause)
        {
            if (pause) _bgm.Pause();
            else _bgm.UnPause();
        }

        public void ApplySettings() => OnSoundChanged(SettingsStore.SoundOn);

        private void OnSoundChanged(bool on) => _bgm.mute = !on;
    }
}
