using System.Windows;
using System.Windows.Media;

namespace Vegraris.Wpf
{
    public class DrawingContext : Drawing.DrawingContext<StreamGeometry, Point>
    {
        public DrawingContext()
        {
            geometry = new StreamGeometry();
            context = geometry.Open();
        }

        private readonly StreamGeometry geometry;
        private readonly StreamGeometryContext context;

        public override void Dispose()
        {
            (context as IDisposable)?.Dispose();
            geometry.Freeze();
            GC.SuppressFinalize(this);
        }

        protected override void OnBeginPath(in Point startPoint) => context.BeginFigure(startPoint, true, true);

        protected override void OnClosePath() { }

        protected override Point OnConvertPoint(in double x, in double y) => new(x, y);

        protected override void OnDrawCubicBezier(in Point controlPoint1, in Point controlPoint2, in Point endPoint)
            => context.BezierTo(controlPoint1, controlPoint2, endPoint, true, true);

        protected override void OnDrawLine(in Point endPoint)
            => context.LineTo(endPoint, true, false);

        protected override StreamGeometry OnGetDrawing() => geometry;
    }
}
