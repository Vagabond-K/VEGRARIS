using System;
using System.Collections.Generic;
using System.Linq;

namespace Vegraris.Drawing
{
    public class BevelDrawingFactory<TDrawing, TContext> : RoundedDrawingFactory<BevelDrawingFactory<TDrawing, TContext>.BevelDrawings>
        where TContext : DrawingContext<TDrawing>, new()
    {
        private readonly static double sqrt = Math.Sqrt(2);
        private readonly static double halfSqrt = sqrt / 2;
        private readonly static double reversedSqrt = 1 - sqrt;
        private readonly static double reversedHarfSqrt = 1 - halfSqrt;
        private readonly static double bezierRatio = 4 * (sqrt - 1) / 3;
        private readonly static double halfBezierRatio = bezierRatio / 2;
        private readonly static double reversedHalfBezierRatio = 1 - halfBezierRatio;
        private readonly static double halfBezierRatioSlope = halfBezierRatio / sqrt;

        private double bevel = 1d / 7 * 4;

        private double bevelThickness;
        private double bevelRounding;

        public double Bevel { get => bevel; set => SetParameter(ref bevel, value); }

        protected override void OnParameterUpdated(string propertyName)
        {
            base.OnParameterUpdated(propertyName);
            bevelThickness = (CellSize - ActualStrokeThickness) / 2 * bevel;
            bevelRounding = bevelThickness / sqrt;
            bevelRounding += (bevelThickness - bevelRounding) * (1 - Math.Max(0, Math.Min(CornerRadius, 1)));
        }

        public readonly struct BevelDrawings
        {
            internal BevelDrawings(TDrawing left, TDrawing top, TDrawing right, TDrawing bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public TDrawing Left { get; }
            public TDrawing Top { get; }
            public TDrawing Right { get; }
            public TDrawing Bottom { get; }
        }

        protected override BevelDrawings OnCreate(IEnumerable<IEnumerable<PiecePoint>> path)
        {
            var left = new TContext();
            var top = new TContext();
            var right = new TContext();
            var bottom = new TContext();

            foreach (var points in path)
            {
                bool isHall = IsHall(points);

                void Draw(PiecePoint corner1, PiecePoint corner2)
                {
                    var point1 = ToPoint(corner1);
                    var point2 = ToPoint(corner2);

                    var direction = ToDirection(corner1, corner2);

                    var radius1 = GetCornerRadius(corner1, isHall);
                    var radius2 = GetCornerRadius(corner2, isHall);
                    var beginPoint = point1 + direction * radius1;

                    var roundStart1 = -direction * radius1;
                    var roundStart2 = direction * radius2;

                    TContext bevelContext = null;
                    if (direction.X > 0) bevelContext = top;
                    else if (direction.X < 0) bevelContext = bottom;
                    else if (direction.Y < 0) bevelContext = left;
                    else if (direction.Y > 0) bevelContext = right;

                    if (bevelContext != null)
                    {
                        var bevelPoint1 = point1 - new Vector(corner1.CellCorner.GetOffsetX(), corner1.CellCorner.GetOffsetY()) * bevelThickness;
                        var bevelPoint2 = point2 - new Vector(corner2.CellCorner.GetOffsetX(), corner2.CellCorner.GetOffsetY()) * bevelThickness;
                        var cornerVector1 = ToVector(corner1);
                        var cornerVector2 = ToVector(corner2);
                        var roundPoint1 = point1 + cornerVector1 * (corner1.IsConvex == true ? -1 : 1) * radius1 * reversedHarfSqrt;
                        var roundPoint2 = point2 + cornerVector2 * (corner2.IsConvex == true ? -1 : 1) * radius2 * reversedHarfSqrt;
                        var bevelBezierDirection1 = new Vector(-corner1.CellCorner.GetOffsetY(), corner1.CellCorner.GetOffsetX());
                        var bevelBezierDirection2 = new Vector(corner2.CellCorner.GetOffsetY(), -corner2.CellCorner.GetOffsetX());
                        var bezierPoint1 = roundPoint1 - cornerVector1 * bevelRounding;
                        var bezierPoint2 = roundPoint2 - cornerVector2 * bevelRounding;
                        var bevelRadius1 = -new Vector(bevelPoint1.X - bezierPoint1.X, bevelPoint1.Y - bezierPoint1.Y).Length / reversedSqrt;
                        var bevelRadius2 = -new Vector(bevelPoint2.X - bezierPoint2.X, bevelPoint2.Y - bezierPoint2.Y).Length / reversedSqrt;

                        bevelContext.BeginPath(beginPoint);
                        bevelContext.DrawLine(point2 - roundStart2);
                        bevelContext.DrawCubicBezier(
                            point2 - roundStart2 * reversedHalfBezierRatio,
                            roundPoint2 + bevelBezierDirection2 * radius2 * halfBezierRatioSlope,
                            roundPoint2);

                        if (bevelRadius2 <= 0)
                            bevelContext.DrawLine(bevelPoint2);
                        else
                        {
                            bevelContext.DrawLine(bezierPoint2);
                            bevelContext.DrawCubicBezier(bezierPoint2 + bevelBezierDirection2 * (bevelRadius2 / sqrt) * halfBezierRatio,
                            bevelPoint2 - direction * bevelRadius2 * reversedHalfBezierRatio,
                            bevelPoint2 - direction * bevelRadius2);
                        }

                        if (bevelRadius1 <= 0)
                            bevelContext.DrawLine(bezierPoint1);
                        else
                        {
                            bevelContext.DrawLine(bevelPoint1 + direction * bevelRadius1);
                            bevelContext.DrawCubicBezier(bevelPoint1 + direction * bevelRadius1 * reversedHalfBezierRatio,
                            bezierPoint1 + bevelBezierDirection1 * (bevelRadius1 / sqrt) * halfBezierRatio,
                            bezierPoint1);
                        }

                        bevelContext.DrawLine(roundPoint1);
                        bevelContext.DrawCubicBezier(
                            roundPoint1 + bevelBezierDirection1 * radius1 * halfBezierRatioSlope,
                            point1 - roundStart1 * reversedHalfBezierRatio,
                            beginPoint);
                        bevelContext.Close();
                    }
                }

                PiecePoint? first = null;
                PiecePoint? last = null;

                foreach (var corner in points.Where(point => point.IsConvex != null))
                {
                    if (first == null)
                        first = corner;
                    if (last != null)
                        Draw(last.Value, corner);
                    last = corner;
                }
                if (first != null && last != null)
                    Draw(last.Value, first.Value);
            }

            var result = new BevelDrawings(left.GetDrawing(), top.GetDrawing(), right.GetDrawing(), bottom.GetDrawing());
            left?.Dispose();
            top?.Dispose();
            right?.Dispose();
            bottom?.Dispose();

            return result;
        }
    }
}
