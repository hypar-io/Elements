using System;

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
        /// <returns>A rectangular Polygon centered around origin.</returns>
        public static Polygon Rectangle(double width, double height)
        {
            var a = new Vector3( -width / 2, -height / 2);
            var b = new Vector3( width / 2, -height / 2);
            var c = new Vector3( width / 2, height / 2);
            var d = new Vector3( -width / 2, height / 2);

            return new Polygon(new[] { a, b, c, d });
        }

        /// <summary>
        /// Create a rectangle.
        /// </summary>
        /// <param name="min">The minimum coordinate.</param>
        /// <param name="max">The maximum coordinate.</param>
        /// <returns>A rectangular Polygon with its lower left corner at min and its upper right corner at max.</returns>
        public static Polygon Rectangle(Vector3 min, Vector3 max)
        {
            var a = min;
            var b = new Vector3(max.X, min.Y);
            var c = max;
            var d = new Vector3(min.X, max.Y);

            return new Polygon(new[] { a, b, c, d });
        }

        /// <summary>
        /// Create a circle.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="divisions">The number of divisions of the circle.</param>
        /// <returns>A circle as a Polygon tessellated into the specified number of divisions.</returns>
        public static Polygon Circle(double radius = 1.0, int divisions = 10)
        {
            var verts = new Vector3[divisions];
            for (var i = 0; i < divisions; i++)
            {
                var t = i * (Math.PI * 2 / divisions);
                verts[i] = new Vector3(radius * Math.Cos(t), radius * Math.Sin(t));
            }
            return new Polygon(verts);
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
            return new Polygon(verts);
        }

        /// <summary>
        /// Create an L.
        /// </summary>
        /// <param name="width">The width of the L.</param>
        /// <param name="length">The length of the L.</param>
        /// <param name="thickness">The thickness of the L.</param>
        /// <returns></returns>
        public static Polygon L(double width, double length, double thickness)
        {   
            if(thickness > length)
            {
                throw new ArgumentOutOfRangeException("The thickness cannot be greater than the length.");
            }
            if(thickness > width)
            {
                throw new ArgumentOutOfRangeException("The thickness cannot be greater that the width.");
            }
            
            var a = new Vector3(0,0,0);
            var b = new Vector3(width,0,0);
            var c = new Vector3(width, thickness, 0);
            var d = new Vector3(thickness,thickness,0);
            var e = new Vector3(thickness, length, 0);
            var f = new Vector3(0,length,0);
            return new Polygon(new[]{a,b,c,d,e,f});
        }
    }
}