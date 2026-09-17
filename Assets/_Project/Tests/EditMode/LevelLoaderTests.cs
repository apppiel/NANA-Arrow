using System;
using System.Linq;
using NanaArrow.Data;
using NanaArrow.Gameplay;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    public class LevelLoaderTests
    {
        private const int FrozenDefaultHits = 2;

        private const string ExampleJson = TestLevels.FormatExampleJson;

        private static ArrowData Basic(string id, int x, int y) =>
            new ArrowData { Id = id, Type = ArrowType.Basic, Direction = Direction.Up, Cells = new[] { new[] { x, y } } };

        [Test]
        public void Parse_FormatExample_MapsAllFields()
        {
            var level = LevelLoader.Parse(ExampleJson);

            Assert.AreEqual(1, level.Version);
            Assert.AreEqual(1, level.Id);
            Assert.AreEqual(5, level.Width);
            Assert.AreEqual(5, level.Height);
            Assert.AreEqual(3, level.Lives);
            Assert.AreEqual(5, level.Arrows.Length);
            CollectionAssert.AreEqual(new[] { "a1", "a5", "a4", "a2", "a3" }, level.Solution);
            Assert.AreEqual(6, (int)level.Meta["minTaps"]);
            Assert.AreEqual("desktop", (string)level.Meta["author"]);

            var a2 = level.Arrows[1];
            Assert.AreEqual("a2", a2.Id);
            Assert.AreEqual(ArrowType.Long, a2.Type);
            Assert.AreEqual(Direction.Right, a2.Direction);
            CollectionAssert.AreEqual(new[] { 1, 4 }, a2.Cells[0]);
            CollectionAssert.AreEqual(new[] { 3, 4 }, a2.Cells[2]);
            Assert.IsNull(a2.Hits);
            Assert.IsNull(a2.KeyGroup);

            Assert.AreEqual(2, level.Arrows[2].Hits);
            Assert.AreEqual("red", level.Arrows[3].KeyGroup);
            Assert.AreEqual(ArrowType.Key, level.Arrows[4].Type);
        }

        [Test]
        public void Parse_OptionalFieldsOmitted_AreNull()
        {
            var level = LevelLoader.Parse(@"{ ""version"": 1, ""id"": 2, ""width"": 3, ""height"": 3,
                ""arrows"": [ { ""id"": ""a"", ""type"": ""Basic"", ""dir"": ""Up"", ""cells"": [[0,0]] } ] }");

            Assert.IsNull(level.Lives);
            Assert.IsNull(level.Solution);
            Assert.IsNull(level.Meta);
        }

        [Test]
        public void Parse_MissingRequiredField_Throws()
        {
            const string noDir = @"{ ""version"": 1, ""id"": 1, ""width"": 3, ""height"": 3,
                ""arrows"": [ { ""id"": ""a"", ""type"": ""Basic"", ""cells"": [[0,0]] } ] }";

            Assert.Catch<JsonException>(() => LevelLoader.Parse(noDir));
        }

        [Test]
        public void Parse_UnknownArrowType_Throws()
        {
            const string bomb = @"{ ""version"": 1, ""id"": 1, ""width"": 3, ""height"": 3,
                ""arrows"": [ { ""id"": ""a"", ""type"": ""Bomb"", ""dir"": ""Up"", ""cells"": [[0,0]] } ] }";

            Assert.Catch<JsonException>(() => LevelLoader.Parse(bomb));
        }

        [Test]
        public void Parse_UnsupportedVersion_Throws()
        {
            const string v2 = @"{ ""version"": 2, ""id"": 1, ""width"": 3, ""height"": 3, ""arrows"": [] }";

            Assert.Throws<NotSupportedException>(() => LevelLoader.Parse(v2));
        }

        [Test]
        public void CreateBoard_FormatExample_PlacesEveryArrowOnItsCells()
        {
            var board = LevelLoader.CreateBoard(LevelLoader.Parse(ExampleJson), FrozenDefaultHits);

            Assert.AreEqual(5, board.Width);
            Assert.AreEqual(5, board.Arrows.Count);
            Assert.AreEqual("a1", board.GetArrowAt(new Vector2Int(0, 2)).Id);
            Assert.AreEqual("a2", board.GetArrowAt(new Vector2Int(1, 4)).Id);
            Assert.AreEqual("a2", board.GetArrowAt(new Vector2Int(3, 4)).Id);
            Assert.AreEqual(new Vector2Int(3, 4), board.GetArrow("a2").Head);
            Assert.AreEqual(2, board.GetArrow("a3").Hits);
            Assert.AreEqual("red", board.GetArrow("a4").KeyGroup);
            Assert.AreEqual("red", board.GetArrow("a5").KeyGroup);
        }

        [Test]
        public void CreateArrow_FrozenWithoutHits_UsesDefault()
        {
            var data = new ArrowData { Id = "f", Type = ArrowType.Frozen, Direction = Direction.Up, Cells = new[] { new[] { 0, 0 } } };

            Assert.AreEqual(FrozenDefaultHits, LevelLoader.CreateArrow(data, FrozenDefaultHits).Hits);
        }

        [Test]
        public void CreateArrow_FrozenWithHits_UsesGiven()
        {
            var data = new ArrowData { Id = "f", Type = ArrowType.Frozen, Direction = Direction.Up, Cells = new[] { new[] { 0, 0 } }, Hits = 3 };

            Assert.AreEqual(3, LevelLoader.CreateArrow(data, FrozenDefaultHits).Hits);
        }

        [Test]
        public void CreateArrow_NonFrozen_IgnoresHits()
        {
            var data = Basic("b", 0, 0);
            data.Hits = 3;

            Assert.AreEqual(Arrow.DefaultHits, LevelLoader.CreateArrow(data, FrozenDefaultHits).Hits);
        }

        [Test]
        public void CreateArrow_KeyGroup_OnlyKeptForLockedAndKey()
        {
            var basic = Basic("b", 0, 0);
            basic.KeyGroup = "red";
            var locked = new ArrowData { Id = "l", Type = ArrowType.Locked, Direction = Direction.Up, Cells = new[] { new[] { 0, 0 } }, KeyGroup = "red" };
            var key = new ArrowData { Id = "k", Type = ArrowType.Key, Direction = Direction.Up, Cells = new[] { new[] { 0, 0 } }, KeyGroup = "red" };

            Assert.IsNull(LevelLoader.CreateArrow(basic, FrozenDefaultHits).KeyGroup);
            Assert.AreEqual("red", LevelLoader.CreateArrow(locked, FrozenDefaultHits).KeyGroup);
            Assert.AreEqual("red", LevelLoader.CreateArrow(key, FrozenDefaultHits).KeyGroup);
        }

        [Test]
        public void ToJson_RoundTrips_FormatExample()
        {
            var original = LevelLoader.Parse(ExampleJson);

            var reparsed = LevelLoader.Parse(LevelLoader.ToJson(original));

            Assert.AreEqual(original.Id, reparsed.Id);
            Assert.AreEqual(original.Lives, reparsed.Lives);
            Assert.AreEqual(original.Arrows.Length, reparsed.Arrows.Length);
            for (var i = 0; i < original.Arrows.Length; i++)
            {
                Assert.AreEqual(original.Arrows[i].Id, reparsed.Arrows[i].Id);
                Assert.AreEqual(original.Arrows[i].Type, reparsed.Arrows[i].Type);
                Assert.AreEqual(original.Arrows[i].Direction, reparsed.Arrows[i].Direction);
                Assert.AreEqual(original.Arrows[i].Hits, reparsed.Arrows[i].Hits);
                Assert.AreEqual(original.Arrows[i].KeyGroup, reparsed.Arrows[i].KeyGroup);
                CollectionAssert.AreEqual(LevelLoader.ToCells(original.Arrows[i]).ToList(), LevelLoader.ToCells(reparsed.Arrows[i]).ToList());
            }
            CollectionAssert.AreEqual(original.Solution, reparsed.Solution);
            Assert.AreEqual((int)original.Meta["minTaps"], (int)reparsed.Meta["minTaps"]);
        }

        [Test]
        public void ToJson_WritesEnumsAsNames_ArraysInline_AndOmitsNulls()
        {
            var json = LevelLoader.ToJson(LevelLoader.Parse(ExampleJson));

            StringAssert.Contains("\"type\": \"Long\"", json);
            StringAssert.Contains("\"dir\": \"Right\"", json);
            StringAssert.Contains("\"cells\": [[1,4],[2,4],[3,4]]", json);
            StringAssert.Contains("\"solution\": [\"a1\",\"a5\",\"a4\",\"a2\",\"a3\"]", json);
            StringAssert.DoesNotContain("\"hits\": null", json);
            StringAssert.DoesNotContain("\"keyGroup\": null", json);
        }

        [Test]
        public void ToJson_OptionalFieldsOmitted_AreNotWritten()
        {
            var level = LevelLoader.Parse(@"{ ""version"": 1, ""id"": 2, ""width"": 3, ""height"": 3,
                ""arrows"": [ { ""id"": ""a"", ""type"": ""Basic"", ""dir"": ""Up"", ""cells"": [[0,0]] } ] }");

            var json = LevelLoader.ToJson(level);

            StringAssert.DoesNotContain("lives", json);
            StringAssert.DoesNotContain("solution", json);
            StringAssert.DoesNotContain("meta", json);
        }

        [Test]
        public void ToCells_MalformedPair_Throws()
        {
            var data = Basic("b", 0, 0);
            data.Cells = new[] { new[] { 1 } };

            Assert.Throws<FormatException>(() => LevelLoader.ToCells(data));
        }

        [Test]
        public void ToCells_ConvertsEveryPair()
        {
            var data = Basic("b", 0, 0);
            data.Cells = new[] { new[] { 1, 2 }, new[] { 3, 4 } };

            CollectionAssert.AreEqual(new[] { new Vector2Int(1, 2), new Vector2Int(3, 4) }, LevelLoader.ToCells(data).ToList());
        }
    }
}
