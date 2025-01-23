using System.Collections.Generic;
using System.Linq;

namespace Vegraris
{
    class Cell
    {
        public Cell(in int x, in int y)
        {
            X = x;
            Y = y;
            cornerPoints = CellCornerExtensions.CellCorners.Select(cellCorner => new CellCornerPoint(this, cellCorner)).ToArray();
        }

        private readonly CellCornerPoint[] cornerPoints;

        public readonly int X;
        public readonly int Y;
        public CellCornerPoint this[CellCorner cellCorner] => cornerPoints[cellCorner.GetIndex()];
        public LockedPiece Piece;
        public Tetromino? Editing;
        public bool Clearing;

        public IEnumerable<CellCornerPoint> CornerPoints => cornerPoints;
    }
}