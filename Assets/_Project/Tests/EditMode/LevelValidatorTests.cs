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

        // docs/LEVEL_FORMAT.md v0.3 예시
        private static LevelData FormatExample() => Level(5,
            Arrow("a1", ArrowType.Basic, Direction.Left, (0, 2)),
            Arrow("a2", ArrowType.Long, Direction.Right, (1, 4), (2, 4), (3, 4)),
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
                Arrow("a2", ArrowType.Long, Direction.Right, (0, 4), (1, 4), (2, 4)),
                Frozen("a3", Direction.Left, 2, (4, 0)),
                Locked("a4", Direction.Down, "red", (4, 4)),
                Key("a5", Direction.Up, "red", (0, 0)));

            var result = Validate(level);

            AssertOnlyRule(result, LevelRule.Solvable);
            Assert.IsEmpty(result.Solution);
            Assert.AreEqual(0, result.MinTaps);
        }

        [TestCase(2)]
        [TestCase(11)]
        public void Validate_BoardSizeOutOfRange_FailsSchema(int size)
        {
            AssertOnlyRule(Validate(Level(size, Arrow("a", ArrowType.Basic, Direction.Up, (0, 0)))), LevelRule.Schema);
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
        public void Validate_LongNotStraight_FailsRule2()
        {
            AssertOnlyRule(Validate(Level(5, Arrow("l", ArrowType.Long, Direction.Right, (0, 0), (1, 0), (1, 1)))), LevelRule.LongShape);
        }

        [Test]
        public void Validate_LongNotContiguous_FailsRule2()
        {
            AssertOnlyRule(Validate(Level(5, Arrow("l", ArrowType.Long, Direction.Right, (0, 0), (2, 0)))), LevelRule.LongShape);
        }

        [Test]
        public void Validate_LongAxisMismatchesDir_FailsRule2()
        {
            AssertOnlyRule(Validate(Level(5, Arrow("l", ArrowType.Long, Direction.Up, (0, 0), (1, 0)))), LevelRule.LongShape);
        }

        [Test]
        public void Validate_LongTooLong_FailsRule2()
        {
            AssertOnlyRule(Validate(Level(5, Arrow("l", ArrowType.Long, Direction.Right, (0, 0), (1, 0), (2, 0), (3, 0)))), LevelRule.LongShape);
        }

        [Test]
        public void Validate_BasicWithTwoCells_FailsRule2()
        {
            AssertOnlyRule(Validate(Level(5, Arrow("b", ArrowType.Basic, Direction.Right, (0, 0), (1, 0)))), LevelRule.LongShape);
        }

        [Test]
        public void Validate_VerticalLong_IsValid()
        {
            var result = Validate(Level(5, Arrow("l", ArrowType.Long, Direction.Down, (2, 1), (2, 2), (2, 3))));

            Assert.IsTrue(result.IsValid, string.Join("\n", result.Errors));
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
