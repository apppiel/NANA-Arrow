using System;
using System.Collections.Generic;
using System.Linq;
using NanaArrow.Data;
using NanaArrow.Gameplay;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NanaArrow.Editor
{
    /// <summary>LEVEL_FORMAT §검증 규칙 1~6. 레벨 저장 전에 반드시 통과해야 한다.</summary>
    public static class LevelValidator
    {
        /// <summary>meta 에 기록하는 최소 탭 수 키 (LEVEL_FORMAT meta.minTaps).</summary>
        public const string MinTapsKey = "minTaps";

        /// <summary>Frozen hits 최소값 = 얼음 깨기 1 + Fire 1 (LEVEL_FORMAT hits "2 이상").</summary>
        private const int FrozenMinHits = 2;

        public static LevelValidationResult Validate(LevelData level, GameConfig gameConfig, ArrowTypeConfig arrowTypeConfig)
        {
            var errors = new List<LevelValidationError>();

            CheckSchema(level, gameConfig, errors);
            if (errors.Count > 0)
                return LevelValidationResult.Invalid(errors);

            CheckCellsInBoundsAndDisjoint(level, errors);
            CheckLongShape(level, arrowTypeConfig, errors);
            CheckLockedHasKey(level, errors);
            if (errors.Count > 0)
                return LevelValidationResult.Invalid(errors);

            var board = LevelLoader.CreateBoard(level, arrowTypeConfig.FrozenDefaultHits);
            var minTaps = board.Arrows.Sum(arrow => arrow.Hits);
            var solution = Simulate(board, level, errors);
            if (errors.Count > 0)
                return LevelValidationResult.Invalid(errors);

            return LevelValidationResult.Valid(solution, minTaps);
        }

        /// <summary>규칙 5·6: 통과한 결과의 solution 과 meta.minTaps 를 레벨에 기록한다 (기존 값은 덮어씀).</summary>
        public static void Record(LevelData level, LevelValidationResult result)
        {
            if (!result.IsValid)
                throw new InvalidOperationException("Cannot record an invalid validation result.");

            level.Solution = result.Solution.ToArray();
            level.Meta ??= new JObject();
            level.Meta[MinTapsKey] = result.MinTaps;
        }

        private static void CheckSchema(LevelData level, GameConfig config, List<LevelValidationError> errors)
        {
            if (!IsInRange(level.Width, config) || !IsInRange(level.Height, config))
                errors.Add(new LevelValidationError(LevelRule.Schema,
                    $"Board {level.Width}x{level.Height} must be {config.MinBoardSize}~{config.MaxBoardSize}."));

            if (level.Arrows == null || level.Arrows.Length == 0)
            {
                errors.Add(new LevelValidationError(LevelRule.Schema, "Level has no arrows."));
                return;
            }

            var ids = new HashSet<string>();
            foreach (var arrow in level.Arrows)
            {
                if (string.IsNullOrEmpty(arrow.Id))
                    errors.Add(new LevelValidationError(LevelRule.Schema, "Arrow id is required."));
                else if (!ids.Add(arrow.Id))
                    errors.Add(new LevelValidationError(LevelRule.Schema, $"Duplicate arrow id '{arrow.Id}'."));

                if (arrow.Cells == null || arrow.Cells.Length == 0)
                    errors.Add(new LevelValidationError(LevelRule.Schema, $"Arrow '{arrow.Id}' has no cells."));
                else
                    for (var i = 0; i < arrow.Cells.Length; i++)
                        if (arrow.Cells[i] == null || arrow.Cells[i].Length != 2)
                            errors.Add(new LevelValidationError(LevelRule.Schema, $"Arrow '{arrow.Id}' cell #{i} must be [x, y]."));

                if (arrow.Type == ArrowType.Frozen && arrow.Hits.HasValue && arrow.Hits.Value < FrozenMinHits)
                    errors.Add(new LevelValidationError(LevelRule.Schema, $"Frozen '{arrow.Id}' hits must be {FrozenMinHits} or more."));
            }
        }

        private static bool IsInRange(int size, GameConfig config) =>
            size >= config.MinBoardSize && size <= config.MaxBoardSize;

        // 규칙 1
        private static void CheckCellsInBoundsAndDisjoint(LevelData level, List<LevelValidationError> errors)
        {
            var occupied = new Dictionary<Vector2Int, string>();
            foreach (var arrow in level.Arrows)
            {
                foreach (var cell in LevelLoader.ToCells(arrow))
                {
                    if (cell.x < 0 || cell.x >= level.Width || cell.y < 0 || cell.y >= level.Height)
                    {
                        errors.Add(new LevelValidationError(LevelRule.CellsInBoundsAndDisjoint,
                            $"Arrow '{arrow.Id}' cell {cell} is outside the {level.Width}x{level.Height} board."));
                        continue;
                    }

                    if (occupied.TryGetValue(cell, out var otherId))
                        errors.Add(new LevelValidationError(LevelRule.CellsInBoundsAndDisjoint,
                            $"Arrow '{arrow.Id}' overlaps '{otherId}' at {cell}."));
                    else
                        occupied[cell] = arrow.Id;
                }
            }
        }

        // 규칙 2
        private static void CheckLongShape(LevelData level, ArrowTypeConfig config, List<LevelValidationError> errors)
        {
            foreach (var arrow in level.Arrows)
            {
                var cells = LevelLoader.ToCells(arrow);

                if (arrow.Type != ArrowType.Long)
                {
                    if (cells.Length != 1)
                        errors.Add(new LevelValidationError(LevelRule.LongShape,
                            $"{arrow.Type} '{arrow.Id}' must occupy exactly one cell, has {cells.Length}."));
                    continue;
                }

                if (cells.Length < config.LongMinLength || cells.Length > config.LongMaxLength)
                {
                    errors.Add(new LevelValidationError(LevelRule.LongShape,
                        $"Long '{arrow.Id}' must have {config.LongMinLength}~{config.LongMaxLength} cells, has {cells.Length}."));
                    continue;
                }

                var horizontal = cells.All(c => c.y == cells[0].y);
                var vertical = cells.All(c => c.x == cells[0].x);
                if (!horizontal && !vertical)
                {
                    errors.Add(new LevelValidationError(LevelRule.LongShape,
                        $"Long '{arrow.Id}' cells must be in a single row or column."));
                    continue;
                }

                var coords = cells.Select(c => horizontal ? c.x : c.y).OrderBy(v => v).ToArray();
                for (var i = 1; i < coords.Length; i++)
                {
                    if (coords[i] == coords[i - 1] + 1) continue;
                    errors.Add(new LevelValidationError(LevelRule.LongShape,
                        $"Long '{arrow.Id}' cells must be contiguous."));
                    break;
                }

                var dirHorizontal = arrow.Direction == Direction.Left || arrow.Direction == Direction.Right;
                if (horizontal != dirHorizontal)
                    errors.Add(new LevelValidationError(LevelRule.LongShape,
                        $"Long '{arrow.Id}' dir {arrow.Direction} must follow its cell axis."));
            }
        }

        // 규칙 3
        private static void CheckLockedHasKey(LevelData level, List<LevelValidationError> errors)
        {
            var keyGroups = new HashSet<string>(level.Arrows
                .Where(a => a.Type == ArrowType.Key && !string.IsNullOrEmpty(a.KeyGroup))
                .Select(a => a.KeyGroup));

            foreach (var arrow in level.Arrows)
            {
                if (arrow.Type != ArrowType.Locked && arrow.Type != ArrowType.Key)
                    continue;

                if (string.IsNullOrEmpty(arrow.KeyGroup))
                {
                    errors.Add(new LevelValidationError(LevelRule.LockedHasKey,
                        $"{arrow.Type} '{arrow.Id}' needs a keyGroup."));
                    continue;
                }

                if (arrow.Type == ArrowType.Locked && !keyGroups.Contains(arrow.KeyGroup))
                    errors.Add(new LevelValidationError(LevelRule.LockedHasKey,
                        $"Locked '{arrow.Id}' has no Key for keyGroup '{arrow.KeyGroup}'."));
            }
        }

        // 규칙 4: 탐욕 시뮬레이션. Exit 는 상황을 나쁘게 만들지 않으므로 백트래킹 불필요.
        private static List<string> Simulate(Board board, LevelData level, List<LevelValidationError> errors)
        {
            var pending = level.Arrows.Select(a => board.GetArrow(a.Id)).ToList();
            var solution = new List<string>();
            var progressed = true;

            while (pending.Count > 0 && progressed)
            {
                progressed = false;
                for (var i = 0; i < pending.Count;)
                {
                    var arrow = pending[i];
                    if (!CanExit(board, arrow))
                    {
                        i++;
                        continue;
                    }

                    board.Remove(arrow);
                    solution.Add(arrow.Id);
                    pending.RemoveAt(i);
                    progressed = true;
                }
            }

            if (pending.Count > 0)
                errors.Add(new LevelValidationError(LevelRule.Solvable,
                    $"Unsolvable: {string.Join(", ", pending.Select(a => a.Id))} can never exit."));

            return solution;
        }

        /// <summary>Frozen 의 얼음 깨기 탭은 경로 무관이므로 시뮬레이션에서는 경로와 잠금만 본다.</summary>
        private static bool CanExit(Board board, Arrow arrow) =>
            !board.IsLocked(arrow) && FireResolver.Resolve(board, arrow).IsExit;
    }
}
