using System;

namespace Vegraris.Drawing
{
    public abstract class DrawingContext<TDrawing> : IDisposable
    {
        public abstract void BeginPath(in Point startPoint);
        public abstract void DrawLine(Point endPoint);
        public abstract void DrawCubicBezier(Point control1, Point control2, Point end);
        public abstract void Close();
        public abstract TDrawing GetDrawing();
        public abstract void Dispose();
    }

    public abstract class DrawingContext<TDrawing, TRendererPoint> : DrawingContext<TDrawing>
    {
        public override void BeginPath(in Point startPoint) => OnBeginPath(ConvertPoint(startPoint));
        public override void DrawLine(Point endPoint) => OnDrawLine(ConvertPoint(endPoint));
        public override void DrawCubicBezier(Point control1, Point control2, Point end)
            => OnDrawCubicBezier(ConvertPoint(control1), ConvertPoint(control2), ConvertPoint(end));
        public override void Close() => OnClosePath();
        public override TDrawing GetDrawing() => OnGetDrawing();

        TRendererPoint ConvertPoint(in Point point) => OnConvertPoint(point.X, point.Y);

        protected internal abstract void OnBeginPath(in TRendererPoint startPoint);
        protected internal abstract void OnDrawLine(in TRendererPoint endPoint);
        protected internal abstract void OnDrawCubicBezier(in TRendererPoint controlPoint1, in TRendererPoint controlPoint2, in TRendererPoint endPoint);
        protected internal abstract void OnClosePath();
        protected internal abstract TRendererPoint OnConvertPoint(in double x, in double y);
        protected internal abstract TDrawing OnGetDrawing();
    }
}