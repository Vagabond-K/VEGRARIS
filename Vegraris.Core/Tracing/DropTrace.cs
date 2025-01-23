using System;
using System.Collections.Generic;
using System.Linq;

namespace Vegraris.Tracing
{
    public class DropTrace : ITrace
    {
        internal DropTrace(Piece piece, int dropHeight, in TimeSpan timeStamp)
        {
            Tetromino = piece.Tetromino;
            Path = piece.Path;
            MovingPath = Path.Where(points => points.Count() != 4 || points.Any(point => point.IsConvex != false)).Select(points => GetMovingPoints(points, dropHeight));
            DropHeight = dropHeight;
            TimeStamp = timeStamp;
        }

        private static IEnumerable<PiecePoint> GetMovingPoints(IEnumerable<PiecePoint> points, int dropHeight)
        {
            var left = points.Min(point => point.CellX);
            var right = points.Max(point => point.CellX);

            var leftTop = points.Where(p => p.IsConvex == true && p.CellCorner == CellCorner.LeftTop && p.CellX == left).OrderBy(p => p.CellY).First();
            var topRight = points.Where(p => p.IsConvex == true && p.CellCorner == CellCorner.TopRight && p.CellX == right).OrderBy(p => p.CellY).First();
            var rightBottom = points.Where(p => p.IsConvex == true && p.CellCorner == CellCorner.RightBottom && p.CellX == right).OrderByDescending(p => p.CellY).First();
            var bottomLeft = points.Where(p => p.IsConvex == true && p.CellCorner == CellCorner.BottomLeft && p.CellX == left).OrderByDescending(p => p.CellY).First();

            return FindPath(points, leftTop, topRight).Concat(FindPath(points, rightBottom, bottomLeft).Select(point => point.Offset(0, dropHeight)));
        }

        private static IEnumerable<PiecePoint> FindPath(IEnumerable<PiecePoint> points, PiecePoint start, PiecePoint end)
        {
            bool started = false;
            foreach (var point in points.Concat(points))
                if (started)
                {
                    yield return point;
                    if (point == end)
                        break;
                }
                else if (point == start)
                {
                    started = true;
                    yield return point;
                }
        }

        public Tetromino Tetromino { get; }
        public IEnumerable<IEnumerable<PiecePoint>> Path { get; }
        public IEnumerable<IEnumerable<PiecePoint>> MovingPath { get; }
        public int DropHeight { get; }
        public TimeSpan TimeStamp { get; }

        public void Complete() { }
    }
}
