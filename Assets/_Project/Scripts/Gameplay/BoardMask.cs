using System.Text;
using UnityEngine;

namespace NanaArrow.Gameplay
{
    /// <summary>
    /// 비직사각 보드 마스크 (LEVEL_FORMAT v0.7 `mask`, W-027). 문자열 배열로 쓰고 `#` 은 쓰는 칸, `.` 은 뺀 칸이다.
    /// <para>
    /// <b>행 순서가 뒤집혀 있다</b>: JSON 의 `mask[0]` 은 <b>맨 윗줄</b>이고 보드 셀 (0,0) 은 <b>좌하단</b>이라
    /// <c>rows[r]</c> ↔ <c>y = Height - 1 - r</c> 로 대응한다. 사람이 파일에서 모양을 눈으로 보려면 위→아래가 자연스럽기 때문.
    /// </para>
    /// 마스크는 <b>표시와 검증에만</b> 쓴다. 레인 판정·Fire 는 사각형 경계(<see cref="Board.IsInside"/>)를 그대로 쓰고,
    /// 마스크 밖 칸은 그냥 빈 칸으로 취급한다 (W-027).
    /// </summary>
    public sealed class BoardMask
    {
        /// <summary>쓰는 칸.</summary>
        public const char Included = '#';
        /// <summary>빼는 칸.</summary>
        public const char Excluded = '.';

        private readonly bool[] _cells;

        public int Width { get; }
        public int Height { get; }
        /// <summary>`#` 칸 수.</summary>
        public int IncludedCount { get; }

        private BoardMask(int width, int height, bool[] cells, int includedCount)
        {
            Width = width;
            Height = height;
            _cells = cells;
            IncludedCount = includedCount;
        }

        public bool Contains(int x, int y) =>
            x >= 0 && x < Width && y >= 0 && y < Height && _cells[y * Width + x];

        public bool Contains(Vector2Int cell) => Contains(cell.x, cell.y);

        /// <summary>
        /// `mask` 문자열 배열을 해석한다. <paramref name="rows"/> 가 null 이거나 비면 마스크 없음(전체 사각형)으로 보고
        /// <paramref name="mask"/> = null, true 를 돌려준다. 크기가 안 맞거나 모르는 문자가 있으면 false + <paramref name="error"/>.
        /// </summary>
        public static bool TryParse(string[] rows, int width, int height, out BoardMask mask, out string error)
        {
            mask = null;
            error = null;
            if (rows == null || rows.Length == 0)
                return true;

            if (rows.Length != height)
            {
                error = $"mask has {rows.Length} rows but the board height is {height}.";
                return false;
            }

            var cells = new bool[width * height];
            var included = 0;
            for (var r = 0; r < height; r++)
            {
                var row = rows[r];
                if (row == null || row.Length != width)
                {
                    error = $"mask row {r} has {(row == null ? 0 : row.Length)} characters but the board width is {width}.";
                    return false;
                }

                var y = height - 1 - r;   // mask[0] = 맨 윗줄
                for (var x = 0; x < width; x++)
                {
                    var c = row[x];
                    if (c == Included)
                    {
                        cells[y * width + x] = true;
                        included++;
                    }
                    else if (c != Excluded)
                    {
                        error = $"mask row {r} has an unexpected character '{c}' at column {x} (use '{Included}' or '{Excluded}').";
                        return false;
                    }
                }
            }

            mask = new BoardMask(width, height, cells, included);
            return true;
        }

        /// <summary>다시 문자열 배열로 (위→아래). 검증기 창의 아스키 미리보기용.</summary>
        public string[] ToRows()
        {
            var rows = new string[Height];
            var sb = new StringBuilder(Width);
            for (var r = 0; r < Height; r++)
            {
                sb.Clear();
                var y = Height - 1 - r;
                for (var x = 0; x < Width; x++)
                    sb.Append(_cells[y * Width + x] ? Included : Excluded);
                rows[r] = sb.ToString();
            }
            return rows;
        }
    }
}
