using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NanaArrow.Data
{
    /// <summary>LEVEL_FORMAT v0.3 레벨 파일 스키마. JSON 필드명과 1:1 (필수 여부도 동일).</summary>
    public sealed class LevelData
    {
        [JsonProperty("version", Required = Required.Always)]
        public int Version { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("width", Required = Required.Always)]
        public int Width { get; set; }

        [JsonProperty("height", Required = Required.Always)]
        public int Height { get; set; }

        /// <summary>생략 시 GameConfig.maxLives.</summary>
        [JsonProperty("lives")]
        public int? Lives { get; set; }

        [JsonProperty("arrows", Required = Required.Always)]
        public ArrowData[] Arrows { get; set; }

        /// <summary>정답 Exit 순서 (검증기가 채움).</summary>
        [JsonProperty("solution")]
        [JsonConverter(typeof(InlineArrayConverter))]
        public string[] Solution { get; set; }

        /// <summary>자유 형식. 검증기는 "minTaps" 만 기록한다.</summary>
        [JsonProperty("meta")]
        public JObject Meta { get; set; }
    }
}
