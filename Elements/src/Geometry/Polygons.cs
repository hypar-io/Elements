using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// Methods to construct various polygons.
    /// </summary>
    public partial class Polygon
    {
        /// <summary>
        /// Create a rectangle.
        /// </summary>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="plane">The plane to draw the rectangle on.</param>
        /// <returns>A rectangular Polygon centered around origin.</returns>
        public static Polygon Rectangle(double width, double height, Plane plane = null)
        {
            Vector3 a, b, c, d;
            if (plane != null && plane.Normal.Dot(Vector3.ZAxis) != 1)
            {
                Vector3 origin = plane.ClosestPoint(Vector3.Origin);
                // calculate Vector3 for each component based on plane normal
                Vector3 XZ = plane.Normal.Cross(Vector3.ZAxis).Unitized() * width / 2;
                Vector3 YZ = plane.Normal.Cross(XZ).Unitized() * height / 2;

                // Vector addition to determine 4 vertices starting from origin and adding
                // x and y components
                a = origin - XZ - YZ;
                b = origin + XZ - YZ;
                c = origin + XZ + YZ;
                d = origin - XZ + YZ;
            }
            else
            {
                // default: draw rectangle on XY plane
                a = new Vector3(-width / 2, -height / 2);
                b = new Vector3(width / 2, -height / 2);
                c = new Vector3(width / 2, height / 2);
                d = new Vector3(-width / 2, height / 2);

            }
            return new Polygon(true, a, b, c, d);
        }

        /// <summary>
        /// Create a rectangle.
        /// </summary>
        /// <param name="min">The minimum coordinate.</param>
        /// <param name="max">The maximum coordinate.</param>
        /// <param name="transform">The transform reference to construct the rectangle from.</param>
        /// <returns>A rectangular Polygon with its lower left corner at min and its upper right corner at max.</returns>
        Polygon Rectangle(Vector3 min, Vector3 max, Transform transform = null)
        {
            transform ??= new Transform();
            var inverse = transform.Inverted();
            var a = inverse.OfPoint(min);
            a.Z = 0;
            var c = inverse.OfPoint(max);
            c.Z = 0;
            var b = ((c.X, a.Y));
            var d = ((a.X, c.Y));
            return new Polygon(new[] { a, b, c, d }).TransformedPolygon(transform);
        }

        /// <summary>
        /// Create a rectangle.
        /// </summary>
        /// <param name="min">The minimum coordinate.</param>
        /// <param name="max">The maximum coordinate.</param>
        /// <param name="plane">The plane to draw the rectangle on.</param>
        /// <param name="alignment">The alignment reference to construct the rectangle from.</param>
        /// <returns>A rectangular Polygon with its lower left corner at min and its upper right corner at max.</returns>
        Polygon Rectangle(Vector3 min, Vector3 max, Plane plane, Vector3? alignment = null)
        {
            alignment ??= plane.Normal.IsParallelTo(Vector3.ZAxis) ? Vector3.XAxis : Vector3.ZAxis;
            var transform = new Transform(plane.Origin, alignment.Value, plane.Normal);
            return Rectangle(min, max, transform);
        }

        /// <summary>
        /// Create a circle.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="divisions">The number of divisions of the circle.</param>
        /// <returns>A circle as a Polygon tessellated into the specified number of divisions.</returns>
        [Obsolete("Please use Elements.Geometry.Circle.ToPolygon() instead.")]
        public static Polygon Circle(double radius = 1.0, int divisions = 10)
        {
            var verts = new Vector3[divisions];
            for (var i = 0; i < divisions; i++)
            {
                var t = i * (Math.PI * 2 / divisions);
                verts[i] = new Vector3(radius * Math.Cos(t), radius * Math.Sin(t));
            }
            return new Polygon(verts, true);
        }

        /// <summary>
        /// Create an ngon.
        /// </summary>
        /// <param name="sides">The number of side of the Polygon.</param>
        /// <param name="radius">The radius of the circle in which the Ngon is inscribed.</param>
        /// <returns>A Polygon with the specified number of sides.</returns>
        /// <exception>Thrown when the radius is less than or equal to zero.</exception>
        /// <exception>Thrown when the number of sides is less than 3.</exception>
        public static Polygon Ngon(int sides, double radius = 0.5)
        {
            if (radius <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The radius must be greater than 0.0.");
            }

            if (sides < 3)
            {
                throw new ArgumentOutOfRangeException("The number of sides must be greater than 3.");
            }

            var verts = new Vector3[sides];
            for (var i = 0; i < sides; i++)
            {
                var t = i * (Math.PI * 2 / sides);
                verts[i] = new Vector3(radius * Math.Cos(t), radius * Math.Sin(t));
            }
            return new Polygon(verts, true);
        }

        /// <summary>
        /// Create an L.
        /// </summary>
        /// <param name="width">The width of the L.</param>
        /// <param name="length">The length of the L.</param>
        /// <param name="thickness">The thickness of the L.</param>
        /// <returns>An L shaped polygon with the origin at the outer corner
        /// of the bend in the L.</returns>
        public static Polygon L(double width, double length, double thickness)
        {
            if (thickness > length)
            {
                throw new ArgumentOutOfRangeException("The thickness cannot be greater than the length.");
            }
            if (thickness > width)
            {
                throw new ArgumentOutOfRangeException("The thickness cannot be greater that the width.");
            }

            var a = new Vector3(0, 0, 0);
            var b = new Vector3(width, 0, 0);
            var c = new Vector3(width, thickness, 0);
            var d = new Vector3(thickness, thickness, 0);
            var e = new Vector3(thickness, length, 0);
            var f = new Vector3(0, length, 0);
            return new Polygon(true, a, b, c, d, e, f);
        }

        /// <summary>
        /// Create a U.
        /// </summary>
        /// <param name="width">The width of the U.</param>
        /// <param name="length">The length of the U.</param>
        /// <param name="thickness">The thickness of the U.</param>
        /// <returns>A U shaped polygon with the origin at the center of
        /// the inside bend of the U.</returns>
        public static Polygon U(double width, double length, double thickness)
        {
            if (thickness >= width / 2)
            {
                throw new ArgumentOutOfRangeException("The thickness cannot be greater that the width.");
            }

            var a = new Vector3(0, 0, 0);
            var b = new Vector3(width / 2 - thickness, 0);
            var c = new Vector3(width / 2 - thickness, length - thickness);
            var d = new Vector3(width / 2, length - thickness);
            var e = new Vector3(width / 2, -thickness);
            var f = new Vector3(-width / 2, -thickness);
            var g = new Vector3(-width / 2, length - thickness);
            var h = new Vector3(-width / 2 + thickness, length - thickness);
            var i = new Vector3(-width / 2 + thickness, 0);
            return new Polygon(true, a, b, c, d, e, f, g, h, i);
        }

        /// <summary>
        /// Create a star.
        /// </summary>
        /// <param name="outerRadius">The outer radius.</param>
        /// <param name="innerRadius">The inner radius.</param>
        /// <param name="points">The number of points.</param>
        /// <returns>A star shaped polygon with the specified number of points
        /// along the outer radius and their compliment along the inner radius.</returns>
        public static Polygon Star(double outerRadius, double innerRadius, int points)
        {
            var c1 = new Circle(Vector3.Origin, innerRadius);
            var c2 = new Circle(Vector3.Origin, outerRadius);
            var verts = new List<Vector3>();
            var count = points * 2;
            for (var i = 0; i < count; i++)
            {
                var t = i * (Math.PI * 2) / count;
                if (i % 2 == 0)
                {
                    verts.Add(c2.PointAt(t));
                }
                else
                {
                    verts.Add(c1.PointAt(t));
                }
            }
            return new Polygon(verts, true);
        }
    }
}