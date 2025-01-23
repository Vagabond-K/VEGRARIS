using System;
using System.Collections.Generic;
using System.Linq;

namespace Vegraris
{
    public enum CellCorner : byte
    {
        //index_directionBits
        LeftTop = 0b0000_0011,
        TopRight = 0b0001_0110,
        RightBottom = 0b0010_1100,
        BottomLeft = 0b0011_1001
    }

    public static class CellCornerExtensions
    {
        private const sbyte negative = -1;
        private const sbyte positive = 1;
        private const sbyte zero = 0;

        public readonly static IReadOnlyList<CellCorner> CellCorners
            = Enum.GetValues(typeof(CellCorner)).Cast<CellCorner>().OrderBy(value => value).ToArray();

        public static int GetIndex(this CellCorner cellCorner) => (int)cellCorner >> 4;
        public static CellCorner Next(this CellCorner cellCorner) => CellCorners[(cellCorner.GetIndex() + 1) % 4];
        public static CellCorner Prev(this CellCorner cellCorner) => CellCorners[(cellCorner.GetIndex() + 3) % 4];
        public static sbyte GetOffsetX(this CellCorner cellCorner) => ((byte)cellCorner & 0b0001) != 0 ? negative : ((byte)cellCorner & 0b0100) != 0 ? positive : zero;
        public static sbyte GetOffsetY(this CellCorner cellCorner) => ((byte)cellCorner & 0b0010) != 0 ? negative : ((byte)cellCorner & 0b1000) != 0 ? positive : zero;
    }
}
