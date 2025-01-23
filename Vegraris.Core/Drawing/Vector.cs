using System;

namespace Vegraris.Drawing
{
    public struct Vector
    {
        public Vector(in double x, in double y)
        {
            X = x;
            Y = y;
        }

        public double X;
        public double Y;
        public double Length => Math.Sqrt(X * X + Y * Y);
        public static Vector operator *(in Vector vector, in double scale) => new Vector(vector.X * scale, vector.Y * scale);
        public static Vector operator -(in Vector vector) => new Vector(-vector.X, -vector.Y);
        public static Point operator -(in Point point, in Vector vector) => new Point(point.X - vector.X, point.Y - vector.Y);
        public static Point operator +(in Point point, in Vector vector) => new Point(point.X + vector.X, point.Y + vector.Y);
    }
}
