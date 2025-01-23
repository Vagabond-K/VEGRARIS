using System;

namespace Vegraris.Drawing
{
    public abstract class RoundedDrawingFactory<TDrawing> : DrawingFactory<TDrawing>
    {
        private double cornerRadius = 0.2;
        private double actualCornerRadius;

        private double harfStrokeThickness;

        public double CornerRadius { get => cornerRadius; set => SetParameter(ref cornerRadius, value); }
        public double ActualCornerRadius { get => actualCornerRadius; set => SetProperty(ref actualCornerRadius, value); }

        protected override void OnParameterUpdated(string propertyName)
        {
            base.OnParameterUpdated(propertyName);
            ActualCornerRadius = (CellSize - ActualStrokeThickness) / 2 * Math.Max(0, Math.Min(cornerRadius, 1));
            harfStrokeThickness = ActualStrokeThickness / 2;
        }

        protected double GetCornerRadius(in PiecePoint point, bool isHall)
        {
            var rounding = actualCornerRadius > 0 ? (point.IsConvex == true ? actualCornerRadius : actualCornerRadius + ActualStrokeThickness) : 0;
            return isHall ? Math.Min(rounding, harfStrokeThickness) : rounding;
        }
    }
}
