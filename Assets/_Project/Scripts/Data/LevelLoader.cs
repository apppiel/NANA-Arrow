using System;
using NanaArrow.Gameplay;
using Newtonsoft.Json;
using UnityEngine;

namespace NanaArrow.Data
{
    /// <summary>레벨 JSON → LevelData → Board. 파일 I/O 는 호출자 몫 (에디터: File, 런타임: TextAsset).</summary>
    public static class LevelLoader
    {
        /// <summary>LEVEL_FORMAT 스키마 버전. 올릴 때 마이그레이션을 여기 추가한다.</summary>
        public const int SupportedVersion = 1;

        private static readonly JsonSerializerSettings WriteSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static LevelData Parse(string json)
        {
            var level = JsonConvert.DeserializeObject<LevelData>(json);
            if (level == null)
                throw new FormatException("Level JSON is empty.");
            if (level.Version != SupportedVersion)
                throw new NotSupportedException($"Level schema version {level.Version} is not supported (expected {SupportedVersion}).");
            return level;
        }

        /// <summary>LevelData → 파일에 쓸 JSON. 생략된 선택 필드는 쓰지 않는다.</summary>
        public static string ToJson(LevelData level) => JsonConvert.SerializeObject(level, WriteSettings);

        /// <param name="frozenDefaultHits">ArrowTypeConfig.frozenDefaultHits. Frozen 의 hits 생략 시 사용.</param>
        public static Board CreateBoard(LevelData level, int frozenDefaultHits)
        {
            var board = new Board(level.Width, level.Height);
            foreach (var data in level.Arrows)
                board.Place(CreateArrow(data, frozenDefaultHits));
            return board;
        }

        /// <summary>cells 는 꼬리 → 머리 순서. dir 이 경로의 마지막 두 칸과 다르면 FormatException.</summary>
        public static Arrow CreateArrow(ArrowData data, int frozenDefaultHits)
        {
            var hits = data.Type == ArrowType.Frozen ? data.Hits ?? frozenDefaultHits : Arrow.DefaultHits;
            var keyGroup = data.Type == ArrowType.Locked || data.Type == ArrowType.Key ? data.KeyGroup : null;
            try
            {
                return new Arrow(data.Id, data.Type, data.Direction, ToCells(data), hits, keyGroup);
            }
            catch (ArgumentException e)
            {
                throw new FormatException(e.Message, e);
            }
        }

        /// <summary>[[x,y], ...] → Vector2Int[]. 좌표 쌍이 아니면 FormatException.</summary>
        public static Vector2Int[] ToCells(ArrowData data)
        {
            if (data.Cells == null)
                throw new FormatException($"Arrow '{data.Id}' has no cells.");

            var cells = new Vector2Int[data.Cells.Length];
            for (var i = 0; i < cells.Length; i++)
            {
                var pair = data.Cells[i];
                if (pair == null || pair.Length != 2)
                    throw new FormatException($"Arrow '{data.Id}' cell #{i} must be [x, y].");
                cells[i] = new Vector2Int(pair[0], pair[1]);
            }
            return cells;
        }
    }
}
