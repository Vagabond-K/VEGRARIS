using System.Text;
using Vegraris.Drawing;

namespace Vegraris.Wasm
{
    class DrawingContext : DrawingContext<string, Point>
    {
        private const char space = ' ';
        private StringBuilder? stringBuilder = new();
        protected override Point OnConvertPoint(in double x, in double y) => new Point(x, y);

        public override void Dispose()
        {
            stringBuilder = null;
            GC.SuppressFinalize(this);
        }

        protected override void OnBeginPath(in Point startPoint)
            => stringBuilder?.Append('M')
            .Append(space).Append(startPoint.X)
            .Append(space).Append(startPoint.Y);

        protected override void OnClosePath() => stringBuilder?.Append('Z');

        protected override void OnDrawCubicBezier(in Point controlPoint1, in Point controlPoint2, in Point endPoint)
            => stringBuilder?.Append('C')
            .Append(space).Append(controlPoint1.X)
            .Append(space).Append(controlPoint1.Y)
            .Append(space).Append(controlPoint2.X)
            .Append(space).Append(controlPoint2.Y)
            .Append(space).Append(endPoint.X)
            .Append(space).Append(endPoint.Y);

        protected override void OnDrawLine(in Point endPoint)
            => stringBuilder?.Append('L')
            .Append(space).Append(endPoint.X)
            .Append(space).Append(endPoint.Y);

        protected override string OnGetDrawing() => stringBuilder?.ToString() ?? string.Empty;
    }
}
