using NanaArrow.Data;

namespace NanaArrow.Tests.EditMode
{
    /// <summary>테스트 공용 레벨 픽스처.</summary>
    public static class TestLevels
    {
        // docs/LEVEL_FORMAT.md v0.3 예시 그대로
        public const string FormatExampleJson = @"{
          ""version"": 1, ""id"": 1, ""width"": 5, ""height"": 5, ""lives"": 3,
          ""arrows"": [
            { ""id"": ""a1"", ""type"": ""Basic"",  ""dir"": ""Left"",  ""cells"": [[0,2]] },
            { ""id"": ""a2"", ""type"": ""Long"",   ""dir"": ""Right"", ""cells"": [[1,4],[2,4],[3,4]] },
            { ""id"": ""a3"", ""type"": ""Frozen"", ""dir"": ""Up"",    ""cells"": [[2,0]], ""hits"": 2 },
            { ""id"": ""a4"", ""type"": ""Locked"", ""dir"": ""Down"",  ""cells"": [[4,4]], ""keyGroup"": ""red"" },
            { ""id"": ""a5"", ""type"": ""Key"",    ""dir"": ""Up"",    ""cells"": [[0,0]], ""keyGroup"": ""red"" }
          ],
          ""solution"": [""a1"", ""a5"", ""a4"", ""a2"", ""a3""],
          ""meta"": { ""author"": ""desktop"", ""difficulty"": 1, ""minTaps"": 6, ""note"": """" }
        }";

        public static LevelData FormatExample() => LevelLoader.Parse(FormatExampleJson);
    }
}
