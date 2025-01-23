namespace Vegraris
{
    public readonly struct PiecePoint
    {
        public PiecePoint(in int cellX, in int cellY, in CellCorner cellCorner, in bool? isConvex = null)
        {
            CellX = cellX;
            CellY = cellY;
            CellCorner = cellCorner;
            IsConvex = isConvex;
        }

        public int CellX { get; }
        public int CellY { get; }
        public CellCorner CellCorner { get; }

        public bool? IsConvex { get; }

        public PiecePoint Offset(in int offsetX, in int offsetY)
            => new PiecePoint(CellX + offsetX, CellY + offsetY, CellCorner, IsConvex);

        public static bool operator ==(PiecePoint point1, PiecePoint point2)
            => point1.CellX == point2.CellX
            && point1.CellY == point2.CellY
            && point1.CellCorner == point2.CellCorner
            && point1.IsConvex == point2.IsConvex;

        public static bool operator !=(PiecePoint point1, PiecePoint point2) => !(point1 == point2);

        public override bool Equals(object obj) => obj is PiecePoint point && point == this;

        public override int GetHashCode()
        {
            int hashCode = -1789691315;
            hashCode = hashCode * -1521134295 + CellX.GetHashCode();
            hashCode = hashCode * -1521134295 + CellY.GetHashCode();
            hashCode = hashCode * -1521134295 + CellCorner.GetHashCode();
            hashCode = hashCode * -1521134295 + IsConvex.GetHashCode();
            return hashCode;
        }
    }
}