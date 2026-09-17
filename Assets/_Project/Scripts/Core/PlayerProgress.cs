using System;
using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>진행도 (최고 클리어 레벨 · 응모 코드 발급) 의 메모리 사본 + 즉시 저장. 레벨별 시도 횟수는 애널리틱스용이라 PlayerPrefs.</summary>
    public sealed class PlayerProgress
    {
        private const string AttemptsKeyPrefix = "attempts_";

        private readonly SaveService _save;
        private SaveData _data;

        public int HighestClearedLevel => _data.HighestClearedLevel;
        public bool RewardCodeIssued => _data.RewardCodeIssued;

        /// <summary>아직 안 깬 가장 낮은 레벨. 탑재 레벨 수로 자르는 건 호출자 몫.</summary>
        public int NextLevel => HighestClearedLevel + 1;

        public event Action Changed;

        public PlayerProgress(SaveService save)
        {
            _save = save;
            _data = save.Load() ?? SaveData.Fresh();
        }

        public bool IsCleared(int level) => level >= 1 && level <= HighestClearedLevel;

        public void MarkCleared(int level)
        {
            if (level <= HighestClearedLevel) return;
            _data = _data.WithHighestClearedLevel(level);
            Persist();
        }

        public void MarkRewardCodeIssued()
        {
            if (RewardCodeIssued) return;
            _data = _data.WithRewardCodeIssued(true);
            Persist();
        }

        /// <summary>치트·테스트: 첫 설치 상태로.</summary>
        public void Reset()
        {
            _data = SaveData.Fresh();
            _save.Delete();
            Changed?.Invoke();
        }

        public int GetAttempts(int level) => PlayerPrefs.GetInt(AttemptsKeyPrefix + level, 0);

        /// <summary>레벨 시작마다 +1 (ANALYTICS attempt).</summary>
        public int IncrementAttempts(int level)
        {
            var attempts = GetAttempts(level) + 1;
            PlayerPrefs.SetInt(AttemptsKeyPrefix + level, attempts);
            PlayerPrefs.Save();
            return attempts;
        }

        private void Persist()
        {
            if (!_save.Save(_data))
                Debug.LogWarning("[PlayerProgress] 저장 실패 — 진행은 계속하지만 다음 실행 때 되돌아갈 수 있음");
            Changed?.Invoke();
        }
    }
}
