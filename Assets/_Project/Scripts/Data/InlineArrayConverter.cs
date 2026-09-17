using System;
using Newtonsoft.Json;

namespace NanaArrow.Data
{
    /// <summary>
    /// 들여쓰기 저장 시 cells / solution 같은 짧은 배열을 한 줄로 쓴다 (LEVEL_FORMAT 예시와 같은 모양).
    /// 읽기는 기본 동작.
    /// </summary>
    public sealed class InlineArrayConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType) => objectType.IsArray;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(JsonConvert.SerializeObject(value, Formatting.None));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
