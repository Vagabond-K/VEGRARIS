using System.Collections.Generic;

namespace Vegraris
{
    public class StaticPiece : Piece
    {
        public StaticPiece(IEnumerable<IEnumerable<PiecePoint>> path, in Tetromino tetromino) : base(tetromino)
        {
            Path = path;

            int? minX = null;
            int? maxX = null;
            int? minY = null;
            int? maxY = null;
            foreach (var points in path)
                foreach (var point in points)
                {
                    if (minX == null || minX > point.CellX)
                        minX = point.CellX;
                    if (maxX == null || maxX < point.CellX)
                        maxX = point.CellX;
                    if (minY == null || minY > point.CellY)
                        minY = point.CellY;
                    if (maxY == null || maxY < point.CellY)
                        maxY = point.CellY;
                }
            if (minX != null)
            {
                Width = maxX.Value - minX.Value + 1;
                Height = maxY.Value - minY.Value + 1;
            }
        }

        public int Width { get; }
        public int Height { get; }
    }
}
