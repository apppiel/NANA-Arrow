using System;
using System.Collections.Generic;

namespace NanaArrow.Editor
{
    public sealed class LevelValidationResult
    {
        public IReadOnlyList<LevelValidationError> Errors { get; }
        public bool IsValid => Errors.Count == 0;

        /// <summary>정답 Exit 순서 (규칙 5). 유효하지 않으면 빈 목록.</summary>
        public IReadOnlyList<string> Solution { get; }

        /// <summary>Arrow 수 + Σ(Frozen.hits − 1) (규칙 5). 유효하지 않으면 0.</summary>
        public int MinTaps { get; }

        private LevelValidationResult(IReadOnlyList<LevelValidationError> errors, IReadOnlyList<string> solution, int minTaps)
        {
            Errors = errors;
            Solution = solution;
            MinTaps = minTaps;
        }

        public static LevelValidationResult Valid(IReadOnlyList<string> solution, int minTaps) =>
            new LevelValidationResult(Array.Empty<LevelValidationError>(), solution, minTaps);

        public static LevelValidationResult Invalid(IReadOnlyList<LevelValidationError> errors) =>
            new LevelValidationResult(errors, Array.Empty<string>(), 0);
    }
}
