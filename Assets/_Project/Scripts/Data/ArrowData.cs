using NanaArrow.Gameplay;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NanaArrow.Data
{
    /// <summary>LEVEL_FORMAT arrows[] 항목. 셀은 [[x,y], ...] 원본 그대로 보관한다.</summary>
    public sealed class ArrowData
    {
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ArrowType Type { get; set; }

        [JsonProperty("dir", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Direction Direction { get; set; }

        [JsonProperty("cells", Required = Required.Always)]
        public int[][] Cells { get; set; }

        /// <summary>Frozen 전용. 생략 시 ArrowTypeConfig.frozenDefaultHits.</summary>
        [JsonProperty("hits")]
        public int? Hits { get; set; }

        /// <summary>Locked/Key 전용.</summary>
        [JsonProperty("keyGroup")]
        public string KeyGroup { get; set; }
    }
}
