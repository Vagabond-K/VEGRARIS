using System;
using System.Collections.Generic;
using System.Linq;

namespace Vegraris.Drawing
{
    public class PieceDrawingFactory<TDrawing, TContext> : RoundedDrawingFactory<TDrawing>
        where TContext : DrawingContext<TDrawing>, new()
    {
        private readonly static double sqrt = Math.Sqrt(2);
        private readonly static double bezierRatio = 4 * (sqrt - 1) / 3;
        private readonly static double reversedBezierRatio = 1 - bezierRatio;

        protected override TDrawing OnCreate(IEnumerable<IEnumerable<PiecePoint>> path)
        {
            var body = new TContext();
            foreach (var points in path)
            {
                bool isHall = IsHall(points);
                bool started = false;

                void Draw(PiecePoint corner1, PiecePoint corner2)
                {
                    var point1 = ToPoint(corner1);
                    var point2 = ToPoint(corner2);

                    var direction = ToDirection(corner1, corner2);

                    var radius1 = GetCornerRadius(corner1, isHall);
                    var radius2 = GetCornerRadius(corner2, isHall);
                    var beginPoint = point1 + direction * radius1;

                    var roundStart = direction * radius2;

                    if (!started)
                    {
                        body.BeginPath(beginPoint);
                        started = true;
                    }
                    body.DrawLine(point2 - roundStart);
                    if (ActualCornerRadius > 0)
                    {
                        var roundEnd = corner2.IsConvex == true ? new Vector(-roundStart.Y, roundStart.X) : new Vector(roundStart.Y, -roundStart.X);
                        body.DrawCubicBezier(point2 - roundStart * reversedBezierRatio, point2 + roundEnd * reversedBezierRatio, point2 + roundEnd);
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

                body.Close();
            }

            var result = body.GetDrawing();
            body.Dispose();

            return result;
        }
    }
}
