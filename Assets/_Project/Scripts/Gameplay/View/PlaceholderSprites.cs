using System;
using UnityEngine;

namespace NanaArrow.Gameplay.View
{
    /// <summary>아트 교체 전까지 쓰는 임시 도형. 모두 1×1 유닛, 중심 피벗.</summary>
    public static class PlaceholderSprites
    {
        private const int Size = 64;

        private static Sprite _square;
        private static Sprite _triangle;
        private static Sprite _padlock;

        public static Sprite Square => _square != null ? _square : _square = Create("Square", (x, y) => true);

        /// <summary>위(+Y)를 가리키는 삼각형.</summary>
        public static Sprite Triangle => _triangle != null ? _triangle : _triangle = Create("Triangle",
            (x, y) => Mathf.Abs(x + 0.5f - Size * 0.5f) <= (Size - y) * 0.5f);

        public static Sprite Padlock => _padlock != null ? _padlock : _padlock = Create("Padlock", IsPadlock);

        private static bool IsPadlock(int x, int y)
        {
            const float bodyTop = Size * 0.55f;
            var body = y < bodyTop && x >= Size * 0.15f && x < Size * 0.85f;
            var dx = x + 0.5f - Size * 0.5f;
            var dy = y + 0.5f - bodyTop;
            var r = Mathf.Sqrt(dx * dx + dy * dy);
            var shackle = y >= bodyTop && r <= Size * 0.3f && r >= Size * 0.18f;
            return body || shackle;
        }

        private static Sprite Create(string name, Func<int, int, bool> inside)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave,
            };
            var pixels = new Color32[Size * Size];
            for (var y = 0; y < Size; y++)
                for (var x = 0; x < Size; x++)
                    pixels[y * Size + x] = inside(x, y) ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            texture.SetPixels32(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
            sprite.name = name;
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }
    }
}
