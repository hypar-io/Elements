using System;
namespace Elements.Geometry
{
    /// <summary>
    /// A 2D Rectangle.
    /// </summary>
    public class Rectangle : Polygon
    {
        public Vector3 Min { get; }
        public Vector3 Max { get; }
        public double Width { get; }
        public double Height { get; }

        public Rectangle(double width, double height) : base(new Vector3[0])
        {
            var a = new Vector3(-width / 2, -height / 2);
            var b = new Vector3(width / 2, -height / 2);
            var c = new Vector3(width / 2, height / 2);
            var d = new Vector3(-width / 2, height / 2);

            Min = a;
            Max = c;
            Width = width;
            Height = height;

            Vertices = new[] { a, b, c, d };
        }

        public Rectangle(Vector3 min, Vector3 max) : base(new Vector3[0])
        {
            var a = min;
            var b = new Vector3(max.X, min.Y);
            var c = max;
            var d = new Vector3(min.X, max.Y);

            Min = min;
            Max = max;
            Width = Math.Abs(max.X - min.X);
            Height = Math.Abs(max.Y - min.Y);
           
            Vertices = new[] { a, b, c, d };
        }

    }
}
