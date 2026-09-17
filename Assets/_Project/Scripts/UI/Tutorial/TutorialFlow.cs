using System.Collections.Generic;

namespace NanaArrow.UI.Tutorial
{
    /// <summary>
    /// 한 판의 튜토리얼 진행 (순수 C#, 연출 없음): 트리거가 처음 일어날 때 항목을 열고, hideOn 트리거나 hideDelay 로 닫는다.
    /// 같은 트리거는 판마다 한 번만 인정한다 (First*). 항목은 한 번에 하나만 보인다 — 새 항목이 열리면 앞 항목을 닫는다.
    /// </summary>
    public sealed class TutorialFlow
    {
        private readonly List<TutorialStep> _pending;
        private readonly float _hideDelay;
        private readonly HashSet<TutorialTrigger> _fired = new HashSet<TutorialTrigger>();
        private float _shownFor;

        public TutorialStep? Current { get; private set; }
        /// <summary>현재 항목이 보인 뒤 지난 시간 (초). 애널리틱스 tutorial_done 의 elapsed.</summary>
        public float Elapsed => _shownFor;
        public bool IsDone => Current == null && _pending.Count == 0;

        public TutorialFlow(IEnumerable<TutorialStep> steps, float hideDelay)
        {
            _pending = new List<TutorialStep>(steps);
            _hideDelay = hideDelay;
        }

        /// <summary>판 시작. LevelStart 항목이 있으면 그것을 연다.</summary>
        public TutorialStep? Start() => Fire(TutorialTrigger.LevelStart);

        /// <summary>
        /// 세션 사건 통지. 처음 온 트리거만 인정한다. 현재 항목의 hideOn 이면 닫고, 대기 중 항목의 trigger 이면 연다.
        /// 반환값은 새로 열린 항목 (없으면 null). 닫힘은 <see cref="Closed"/> 로 알린다.
        /// </summary>
        public TutorialStep? Fire(TutorialTrigger trigger)
        {
            if (trigger == TutorialTrigger.None || !_fired.Add(trigger)) return null;

            if (Current.HasValue && Current.Value.HideOn == trigger)
                Close();

            for (var i = 0; i < _pending.Count; i++)
            {
                if (_pending[i].Trigger != trigger) continue;
                var step = _pending[i];
                _pending.RemoveAt(i);
                Show(step);
                return step;
            }
            return null;
        }

        /// <summary>시간 경과. hideOn 이 None 인 항목은 hideDelay 뒤 닫힌다 (0 이면 유지). 닫혔으면 true.</summary>
        public bool Tick(float deltaTime)
        {
            if (!Current.HasValue) return false;
            _shownFor += deltaTime;
            if (Current.Value.HideOn != TutorialTrigger.None || _hideDelay <= 0f || _shownFor < _hideDelay) return false;
            Close();
            return true;
        }

        /// <summary>현재 항목과 그 표시 시간. 닫힐 때 호출자에게 전달된다.</summary>
        public event System.Action<TutorialStep, float> Closed;

        /// <summary>판이 끝나거나 다시 시작할 때.</summary>
        public void Abort()
        {
            if (Current.HasValue) Close();
            _pending.Clear();
        }

        private void Show(TutorialStep step)
        {
            if (Current.HasValue) Close();
            Current = step;
            _shownFor = 0f;
        }

        private void Close()
        {
            var step = Current.Value;
            Current = null;
            Closed?.Invoke(step, _shownFor);
        }
    }
}
