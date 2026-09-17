using System;
using System.Linq;
using NanaArrow.Data;
using NanaArrow.Editor;
using NanaArrow.Gameplay;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class LevelValidatorTests
    {
        private GameConfig _gameConfig;
        private ArrowTypeConfig _arrowTypes;

        [SetUp]
        public void SetUp()
        {
            _gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            _arrowTypes = ScriptableObject.CreateInstance<ArrowTypeConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_gameConfig);
            UnityEngine.Object.DestroyImmediate(_arrowTypes);
        }

        private static ArrowData Arrow(string id, ArrowType type, Direction dir, params (int x, int y)[] cells) =>
            new ArrowData { Id = id, Type = type, Direction = dir, Cells = cells.Select(c => new[] { c.x, c.y }).ToArray() };

        private static ArrowData Locked(string id, Direction dir, string keyGroup, (int x, int y) cell)
        {
            var data = Arrow(id, ArrowType.Locked, dir, cell);
            data.KeyGroup = keyGroup;
            return data;
        }

        private static ArrowData Key(string id, Direction dir, string keyGroup, (int x, int y) cell)
        {
            var data = Arrow(id, ArrowType.Key, dir, cell);
            data.KeyGroup = keyGroup;
            return data;
        }

        private static ArrowData Frozen(string id, Direction dir, int? hits, (int x, int y) cell)
        {
            var data = Arrow(id, ArrowType.Frozen, dir, cell);
            data.Hits = hits;
            return data;
        }

        private static LevelData Level(int size, params ArrowData[] arrows) =>
            new LevelData { Version = 1, Id = 1, Width = size, Height = size, Arrows = arrows };

        // docs/LEVEL_FORMAT.md v0.3 예시 (a2 는 v0.6 경로형 Basic)
        private static LevelData FormatExample() => Level(5,
            Arrow("a1", ArrowType.Basic, Direction.Left, (0, 2)),
            Arrow("a2", ArrowType.Basic, Direction.Right, (1, 4), (2, 4), (3, 4)),
            Frozen("a3", Direction.Up, 2, (2, 0)),
            Locked("a4", Direction.Down, "red", (4, 4)),
            Key("a5", Direction.Up, "red", (0, 0)));

        private LevelValidationResult Validate(LevelData level) =>
            LevelValidator.Validate(level, _gameConfig, _arrowTypes);

        private static void AssertOnlyRule(LevelValidationResult result, LevelRule rule)
        {
            var report = string.Join("\n", result.Errors);
            Assert.IsFalse(result.IsValid, "expected invalid");
            Assert.IsNotEmpty(result.Errors, "expected errors");
            Assert.IsTrue(result.Errors.All(e => e.Rule == rule), $"expected only {rule}:\n{report}");
        }

        [Test]
        public void Validate_FormatExample_IsValid_WithDocumentedSolutionAndMinTaps()
        {
            var result = Validate(FormatExample());

            Assert.IsTrue(result.IsValid, string.Join("\n", result.Errors));
            CollectionAssert.AreEqual(new[] { "a1", "a5", "a4", "a2", "a3" }, result.Solution);
            Assert.AreEqual(6, result.MinTaps);
        }

        [Test]
        public void Validate_FormatV01Example_FailsSolvable()
        {
            var level = Level(5,
                Arrow("a1", ArrowType.Basic, Direction.Up, (2, 2)),
                Arrow("a2", ArrowType.Basic, Direction.Right, (0, 4), (1, 4), (2, 4)),
                Frozen("a3", Direction.Left, 2, (4, 0)),
                Locked("a4", Direction.Down, "red", (4, 4)),
                Key("a5", Direction.Up, "red", (0, 0)));

            var result = Validate(level);

            AssertOnlyRule(result, LevelRule.Solvable);
            Assert.IsEmpty(result.Solution);
            Assert.AreEqual(0, result.MinTaps);
        }

        [TestCase(2, 5)]
        [TestCase(5, 2)]
        [TestCase(11, 5)]
        [TestCase(5, 15)]
        public void Validate_BoardSizeOutOfRange_FailsSchema(int width, int height)
        {
            var level = new LevelData { Version = 1, Id = 1, Width = width, Height = height,
                Arrows = new[] { Arrow("a", ArrowType.Basic, Direction.Up, (0, 0)) } };

            AssertOnlyRule(Validate(level), LevelRule.Schema);
        }

        [Test]
        public void Validate_BoardAtMaxWidthAndHeight_IsValid()
        {
            var level = new LevelData { Version = 1, Id = 1, Width = 10, Height = 14,
                Arrows = new[] { Arrow("a", ArrowType.Basic, Direction.Up, (9, 13)) } };

            Assert.IsTrue(Validate(level).IsValid, string.Join("\n", Validate(level).Errors));
        }

        [Test]
        public void Validate_OwnBodyOnLane_FailsRule2e()
        {
            // 꼬리(2,0)가 머리(1,0)의 오른쪽 레인 위에 있는 U 자 경로
            var u = Arrow("u", ArrowType.Basic, Direction.Right, (2, 0), (2, 1), (1, 1), (0, 1), (0, 0), (1, 0));

            AssertOnlyRule(Validate(Level(5, u)), LevelRule.PathShape);
        }

        [Test]
        public void Validate_BodyBesideLane_IsFine()
        {
            // 몸통이 레인 옆 칸에만 있으면 OK
            var hook = Arrow("h", ArrowType.Basic, Direction.Up, (1, 0), (0, 0), (0, 1), (0, 2), (1, 2), (1, 3));

            Assert.IsTrue(Validate(Level(5, hook)).IsValid, string.Join("\n", Validate(Level(5, hook)).Errors));
        }

        [Test]
        public void Validate_NoArrows_FailsSchema()
        {
            AssertOnlyRule(Validate(Level(5)), LevelRule.Schema);
        }

        [Test]
        public void Validate_DuplicateId_FailsSchema()
        {
            var level = Level(5,
                Arrow("a", ArrowType.Basic, Direction.Up, (0, 0)),
                Arrow("a", ArrowType.Basic, Direction.Up, (1, 0)));

            AssertOnlyRule(Validate(level), LevelRule.Schema);
        }

        [Test]
        public void Validate_MalformedCell_FailsSchema()
        {
            var arrow = Arrow("a", ArrowType.Basic, Direction.Up, (0, 0));
            arrow.Cells = new[] { new[] { 0 } };

            AssertOnlyRule(Validate(Level(5, arrow)), LevelRule.Schema);
        }

        [Test]
        public void Validate_FrozenHitsBelowTwo_FailsSchema()
        {
            AssertOnlyRule(Validate(Level(5, Frozen("f", Direction.Up, 1, (0, 0)))), LevelRule.Schema);
        }

        [Test]
        public void Validate_CellOutOfBounds_FailsRule1()
        {
            AssertOnlyRule(Validate(Level(5, Arrow("a", ArrowType.Basic, Direction.Up, (5, 0)))), LevelRule.CellsInBoundsAndDisjoint);
        }

        [Test]
        public void Validate_OverlappingArrows_FailsRule1()
        {
            var level = Level(5,
                Arrow("a", ArrowType.Basic, Direction.Up, (2, 2)),
                Arrow("b", ArrowType.Basic, Direction.Down, (2, 2)));

            AssertOnlyRule(Validate(level), LevelRule.CellsInBoundsAndDisjoint);
        }

        [Test]
        public void Validate_PathNotAdjacent_FailsRule2()
        {
            AssertOnlyRule(Validate(Level(5, Arrow("p", ArrowType.Basic, Direction.Right, (0, 0), (2, 0), (3, 0)))), LevelRule.PathShape);
            AssertOnlyRule(Validate(Level(5, Arrow("p", ArrowType.Basic, Direction.Right, (0, 0), (1, 1), (2, 1)))), LevelRule.PathShape);
        }

        [Test]
        public void Validate_PathCrossesItself_FailsRule2()
        {
            // 한 바퀴 돌아 자기 칸을 다시 밟음
            var loop = Arrow("p", ArrowType.Basic, Direction.Up, (0, 0), (1, 0), (1, 1), (0, 1), (0, 0), (0, 1));
            AssertOnlyRule(Validate(Level(5, loop)), LevelRule.PathShape);
        }

        [Test]
        public void Validate_PathDirMismatchesLastStep_FailsRule2()
        {
            AssertOnlyRule(Validate(Level(5, Arrow("p", ArrowType.Basic, Direction.Up, (0, 0), (1, 0)))), LevelRule.PathShape);
        }

        [Test]
        public void Validate_PathTooLong_FailsRule2()
        {
            // 45칸 뱀 모양 (기본 maxArrowLength 40 초과): 9×5 를 지그재그로 채움, 머리 (0,0) 은 왼쪽을 향함
            var cells = new System.Collections.Generic.List<(int x, int y)>();
            for (var y = 0; y < 5; y++)
                for (var i = 0; i < 9; i++)
                    cells.Add((y % 2 == 0 ? i : 8 - i, y));
            cells.Reverse();
            var snake = cells.ToArray();
            Assert.AreEqual(45, snake.Length);
            var level = new LevelData { Version = 1, Id = 1, Width = 9, Height = 5, Arrows = new[] { Arrow("p", ArrowType.Basic, Direction.Left, snake) } };
            AssertOnlyRule(Validate(level), LevelRule.PathShape);
        }

        [Test]
        public void Validate_BentPath_IsValid_AndSolvable()
        {
            var level = Level(5,
                Arrow("p", ArrowType.Basic, Direction.Right, (0, 0), (0, 1), (0, 2), (1, 2)),
                Arrow("q", ArrowType.Basic, Direction.Up, (3, 0), (3, 1)));

            var result = Validate(level);

            Assert.IsTrue(result.IsValid, string.Join("\n", result.Errors));
            Assert.AreEqual(2, result.MinTaps);
        }

        [Test]
        public void Validate_SingleCellArrow_IsValid()
        {
            var result = Validate(Level(5, Arrow("s", ArrowType.Basic, Direction.Down, (2, 2))));

            Assert.IsTrue(result.IsValid, string.Join("\n", result.Errors));
        }

        [Test]
        public void Validate_PathBlockedByOtherArrowsBody_FailsSolvableUntilItMoves()
        {
            // q 의 몸통이 p 의 레인을 가로지름. q 는 나갈 수 있으니 전체는 풀림: solution = q, p
            var level = Level(5,
                Arrow("p", ArrowType.Basic, Direction.Right, (0, 2)),
                Arrow("q", ArrowType.Basic, Direction.Up, (2, 1), (2, 2), (2, 3)));

            var result = Validate(level);

            Assert.IsTrue(result.IsValid, string.Join("\n", result.Errors));
            CollectionAssert.AreEqual(new[] { "q", "p" }, result.Solution);
        }

        [Test]
        public void Validate_LockedWithoutKey_FailsRule3()
        {
            AssertOnlyRule(Validate(Level(5, Locked("l", Direction.Up, "red", (0, 0)))), LevelRule.LockedHasKey);
        }

        [Test]
        public void Validate_LockedWithoutKeyGroup_FailsRule3()
        {
            AssertOnlyRule(Validate(Level(5, Locked("l", Direction.Up, null, (0, 0)))), LevelRule.LockedHasKey);
        }

        [Test]
        public void Validate_MutualBlock_FailsRule4()
        {
            var level = Level(5,
                Arrow("a", ArrowType.Basic, Direction.Right, (0, 0)),
                Arrow("b", ArrowType.Basic, Direction.Left, (1, 0)));

            var result = Validate(level);

            AssertOnlyRule(result, LevelRule.Solvable);
            StringAssert.Contains("a", result.Errors[0].Message);
            StringAssert.Contains("b", result.Errors[0].Message);
        }

        [Test]
        public void Validate_KeyBlockedByItsOwnLock_FailsRule4()
        {
            var level = Level(5,
                Key("k", Direction.Up, "g", (0, 0)),
                Locked("l", Direction.Right, "g", (0, 1)));

            AssertOnlyRule(Validate(level), LevelRule.Solvable);
        }

        [Test]
        public void Validate_LockedUnlocksAfterAllKeysExit()
        {
            var level = Level(5,
                Locked("l", Direction.Up, "g", (2, 2)),
                Key("k1", Direction.Right, "g", (0, 0)),
                Key("k2", Direction.Right, "g", (0, 1)));

            var result = Validate(level);

            Assert.IsTrue(result.IsValid, string.Join("\n", result.Errors));
            CollectionAssert.AreEqual(new[] { "k1", "k2", "l" }, result.Solution);
        }

        [Test]
        public void Validate_FrozenHits_AddToMinTaps()
        {
            var level = Level(5,
                Arrow("a", ArrowType.Basic, Direction.Up, (0, 0)),
                Frozen("f", Direction.Up, 3, (1, 0)));

            Assert.AreEqual(4, Validate(level).MinTaps);
        }

        [Test]
        public void Validate_FrozenWithoutHits_UsesConfigDefaultForMinTaps()
        {
            var result = Validate(Level(5, Frozen("f", Direction.Up, null, (0, 0))));

            Assert.AreEqual(_arrowTypes.FrozenDefaultHits, result.MinTaps);
        }

        [Test]
        public void Validate_CollectsErrorsAcrossRules1To3()
        {
            var level = Level(5,
                Arrow("out", ArrowType.Basic, Direction.Up, (9, 9)),
                Locked("l", Direction.Up, "red", (0, 0)));

            var result = Validate(level);

            CollectionAssert.AreEquivalent(
                new[] { LevelRule.CellsInBoundsAndDisjoint, LevelRule.LockedHasKey },
                result.Errors.Select(e => e.Rule).ToArray());
        }

        [Test]
        public void Record_WritesSolutionAndMinTaps_AndKeepsOtherMetaKeys()
        {
            var level = FormatExample();
            level.Solution = new[] { "stale" };
            level.Meta = JObject.Parse(@"{ ""author"": ""desktop"", ""minTaps"": 99 }");

            LevelValidator.Record(level, Validate(level));

            CollectionAssert.AreEqual(new[] { "a1", "a5", "a4", "a2", "a3" }, level.Solution);
            Assert.AreEqual(6, (int)level.Meta["minTaps"]);
            Assert.AreEqual("desktop", (string)level.Meta["author"]);
        }

        [Test]
        public void Record_CreatesMetaWhenMissing()
        {
            var level = FormatExample();

            LevelValidator.Record(level, Validate(level));

            Assert.AreEqual(6, (int)level.Meta["minTaps"]);
        }

        [Test]
        public void Record_InvalidResult_Throws()
        {
            var level = Level(5, Locked("l", Direction.Up, "red", (0, 0)));

            Assert.Throws<InvalidOperationException>(() => LevelValidator.Record(level, Validate(level)));
        }
    }
}
