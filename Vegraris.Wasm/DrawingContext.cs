using System.Globalization;
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
            .Append(space).AppendDouble(startPoint.X)
            .Append(space).AppendDouble(startPoint.Y);

        protected override void OnClosePath() => stringBuilder?.Append('Z');

        protected override void OnDrawCubicBezier(in Point controlPoint1, in Point controlPoint2, in Point endPoint)
            => stringBuilder?.Append('C')
            .Append(space).AppendDouble(controlPoint1.X)
            .Append(space).AppendDouble(controlPoint1.Y)
            .Append(space).AppendDouble(controlPoint2.X)
            .Append(space).AppendDouble(controlPoint2.Y)
            .Append(space).AppendDouble(endPoint.X)
            .Append(space).AppendDouble(endPoint.Y);

        protected override void OnDrawLine(in Point endPoint)
            => stringBuilder?.Append('L')
            .Append(space).AppendDouble(endPoint.X)
            .Append(space).AppendDouble(endPoint.Y);

        protected override string OnGetDrawing() => stringBuilder?.ToString() ?? string.Empty;
    }

    static class StringBuilderExtensions
    {
        private const string doubleFormat = "{0}";

        public static StringBuilder AppendDouble(this StringBuilder stringBuilder, in double value)
            => stringBuilder.AppendFormat(CultureInfo.InvariantCulture, doubleFormat, value);
    }
}
