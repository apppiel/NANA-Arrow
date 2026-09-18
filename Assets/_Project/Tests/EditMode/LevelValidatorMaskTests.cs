using System.Linq;
using NanaArrow.Data;
using NanaArrow.Editor;
using NanaArrow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace NanaArrow.Tests.EditMode
{
    /// <summary>검증기 규칙 1 의 mask 확장 (W-027).</summary>
    public class LevelValidatorMaskTests
    {
        private GameConfig _gameConfig;
        private ArrowTypeConfig _arrowTypeConfig;

        [SetUp]
        public void SetUp()
        {
            _gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            _arrowTypeConfig = ScriptableObject.CreateInstance<ArrowTypeConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameConfig);
            Object.DestroyImmediate(_arrowTypeConfig);
        }

        /// <summary>십자 마스크 5×5. 화살표는 전부 세로로 위를 향해 한 칸씩.</summary>
        private static LevelData CrossLevel(string[] mask, params Vector2Int[] arrowCells)
        {
            return new LevelData
            {
                Version = 1,
                Id = 99,
                Width = 5,
                Height = 5,
                Mask = mask,
                Arrows = arrowCells.Select((c, i) => new ArrowData
                {
                    Id = "a" + (i + 1),
                    Type = ArrowType.Basic,
                    Direction = Direction.Up,
                    Cells = new[] { new[] { c.x, c.y } },
                }).ToArray(),
            };
        }

        private static readonly string[] Cross =
        {
            "..#..",
            "..#..",
            "#####",
            "..#..",
            "..#..",
        };

        [Test]
        public void Validate_ArrowInsideMask_Passes()
        {
            var level = CrossLevel(Cross, new Vector2Int(2, 4));

            var result = LevelValidator.Validate(level, _gameConfig, _arrowTypeConfig);

            Assert.IsTrue(result.IsValid, string.Join("; ", result.Errors.Select(e => e.ToString())));
        }

        [Test]
        public void Validate_ArrowOutsideMask_Fails()
        {
            // (0,4) 는 사각형 안이지만 십자 마스크 밖 (맨 윗줄 "..#..")
            var level = CrossLevel(Cross, new Vector2Int(0, 4));

            var result = LevelValidator.Validate(level, _gameConfig, _arrowTypeConfig);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.Rule == LevelRule.CellsInBoundsAndDisjoint),
                "규칙 1 위반이어야 한다");
            Assert.IsTrue(result.Errors.Any(e => e.ToString().Contains("mask")),
                "메시지에 mask 가 있어야 기획자가 원인을 안다");
        }

        [Test]
        public void Validate_MaskRowCountMismatch_Fails()
        {
            var level = CrossLevel(new[] { "..#..", "..#.." }, new Vector2Int(2, 4));

            var result = LevelValidator.Validate(level, _gameConfig, _arrowTypeConfig);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.ToString().Contains("rows")));
        }

        [Test]
        public void Validate_MaskRowWidthMismatch_Fails()
        {
            var level = CrossLevel(new[] { "..#..", "..#..", "###", "..#..", "..#.." }, new Vector2Int(2, 4));

            var result = LevelValidator.Validate(level, _gameConfig, _arrowTypeConfig);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.ToString().Contains("characters")));
        }

        [Test]
        public void Validate_NoMask_StillPasses()
        {
            var level = CrossLevel(null, new Vector2Int(0, 4));

            var result = LevelValidator.Validate(level, _gameConfig, _arrowTypeConfig);

            Assert.IsTrue(result.IsValid, "마스크가 없으면 사각형 전체가 쓸 수 있는 칸");
        }
    }
}
