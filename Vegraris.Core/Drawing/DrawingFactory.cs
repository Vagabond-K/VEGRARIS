using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Vegraris.Drawing
{
    public abstract class DrawingFactory<TDrawing> : INotifyPropertyChanged
    {
        protected DrawingFactory() => OnParameterUpdated(string.Empty);

        private double cellSize = 20;
        private double strokeThickness = 2;
        private bool containsHole = true;
        private bool splitCell;
        private double actualStrokeThickness;

        private double halfCellSize;
        private double halfInsideSize;

        public double CellSize { get => cellSize; set => SetParameter(ref cellSize, value); }
        public double StrokeThickness { get => strokeThickness; set => SetParameter(ref strokeThickness, value); }
        public bool ContainsHole { get => containsHole; set => SetParameter(ref containsHole, value); }
        public bool SplitCell { get => splitCell; set => SetParameter(ref splitCell, value); }
        public double ActualStrokeThickness { get => actualStrokeThickness; private set => SetProperty(ref actualStrokeThickness, value); }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T target, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(target, value))
            {
                target = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        protected bool SetParameter<T>(ref T target, T value, [CallerMemberName] string propertyName = null)
        {
            var result = SetProperty(ref target, value, propertyName);
            if (result)
                OnParameterUpdated(propertyName);
            return result;
        }

        protected virtual void OnParameterUpdated(string propertyName)
        {
            ActualStrokeThickness = Math.Min(cellSize, strokeThickness);
            halfCellSize = cellSize / 2;
            halfInsideSize = (cellSize - actualStrokeThickness) / 2;
        }

        protected Point ToPoint(in PiecePoint point) => new Point(
            point.CellX * cellSize + halfCellSize + point.CellCorner.GetOffsetX() * halfInsideSize,
            point.CellY * cellSize + halfCellSize + point.CellCorner.GetOffsetY() * halfInsideSize);

        protected static Vector ToVector(in PiecePoint point) => new Vector(point.CellCorner.GetOffsetX(), point.CellCorner.GetOffsetY());

        protected static Vector ToDirection(in PiecePoint point1, in PiecePoint point2) => new Vector(
            Math.Max(-1, Math.Min(point2.CellX * 3 + point2.CellCorner.GetOffsetX() - point1.CellX * 3 - point1.CellCorner.GetOffsetX(), 1)),
            Math.Max(-1, Math.Min(point2.CellY * 3 + point2.CellCorner.GetOffsetY() - point1.CellY * 3 - point1.CellCorner.GetOffsetY(), 1)));

        protected static bool IsHall(IEnumerable<PiecePoint> points)
        {
            var count = 0;
            foreach (var point in points)
                if (point.IsConvex != false) break;
                else count++;
            return count == 4;
        }

        protected abstract TDrawing OnCreate(IEnumerable<IEnumerable<PiecePoint>> path);

        public TDrawing Create(IEnumerable<IEnumerable<PiecePoint>> path)
            => splitCell
            ? OnCreate(path.Where(points => !IsHall(points))
                .SelectMany(points => points.Select(point => new Point(point.CellX, point.CellY))).Distinct()
                .Select(point => CellCornerExtensions.CellCorners.Select(corner => new PiecePoint((int)point.X, (int)point.Y, corner, true))).ToArray())
            : OnCreate(containsHole ? path : path.Where(points => !IsHall(points)));
    }
}
