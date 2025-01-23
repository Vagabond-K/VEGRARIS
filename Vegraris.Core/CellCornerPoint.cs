namespace Vegraris
{
    class CellCornerPoint
    {
        public CellCornerPoint(Cell cell, in CellCorner cellCorner)
        {
            Cell = cell;
            CellCorner = cellCorner;
        }

        public readonly Cell Cell;
        public readonly CellCorner CellCorner;
        public CellCorner NextCorner => CellCorner.Next();
        public CellCorner PrevCorner => CellCorner.Prev();

        public CellCornerPoint NextPoint;
    }
}