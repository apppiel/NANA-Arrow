using System;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>AudioManager 인스펙터용 (SoundId → AudioClip).</summary>
    [Serializable]
    public sealed class SoundClip
    {
        [SerializeField] private SoundId id;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        public SoundId Id => id;
        public AudioClip Clip => clip;
        public float Volume => volume;
    }
}
