using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A 3D vector.
    /// </summary>
    public partial struct Vector3 : IComparable<Vector3>, IEquatable<Vector3>
    {
        /// <summary>
        /// A tolerance for comparison operations of 1e-5.
        /// </summary>
        public const double EPSILON = 1e-5;

        private static Vector3 _xAxis = new Vector3(1, 0, 0);
        private static Vector3 _yAxis = new Vector3(0, 1, 0);
        private static Vector3 _zAxis = new Vector3(0, 0, 1);
        private static Vector3 _origin = new Vector3();

        /// <summary>
        /// Create a vector at the origin.
        /// </summary>
        /// <returns></returns>
        public static Vector3 Origin
        {
            get { return _origin; }
        }

        /// <summary>
        /// Get the hash code for the vector.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Create a vector along the X axis.
        /// </summary>
        public static Vector3 XAxis
        {
            get { return _xAxis; }
        }

        /// <summary>
        /// Create a vector along the Y axis.
        /// </summary>
        public static Vector3 YAxis
        {
            get { return _yAxis; }
        }

        /// <summary>
        /// Create a vector along the Z axis.
        /// </summary>
        public static Vector3 ZAxis
        {
            get { return _zAxis; }
        }

        /// <summary>
        /// Create vectors at n equal spaces along the provided line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="n">The number of samples along the line.</param>
        /// <param name="includeEnds">A flag indicating whether or not to include points for the start and end of the line.</param>
        /// <returns></returns>
        public static IList<Vector3> AtNEqualSpacesAlongLine(Line line, int n, bool includeEnds = false)
        {
            var div = 1.0 / (double)(n + 1);
            var pts = new List<Vector3>();
            for (var t = 0.0; t <= 1.0; t += div)
            {
                var pt = line.PointAt(t);

                if ((t == 0.0 && !includeEnds) || (t == 1.0 && !includeEnds))
                {
                    continue;
                }
                pts.Add(pt);
            }
            return pts;
        }

        /// <summary>
        /// Create a Vector3 by copying the components of another Vector3.
        /// </summary>
        /// <param name="v">The Vector3 to copy.</param>
        public Vector3(Vector3 v)
        {
            this.X = v.X;
            this.Y = v.Y;
            this.Z = v.Z;
        }

        /// <summary>
        /// Create a vector from x, and y coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the vector.</param>
        /// <param name="y">Thy y coordinate of the vector.</param>
        /// <exception>Thrown if any components of the vector are NaN or Infinity.</exception>
        public Vector3(double x, double y)
        {
            if (Double.IsNaN(x) || Double.IsNaN(y))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was NaN.");
            }

            if (Double.IsInfinity(x) || Double.IsInfinity(y))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was infinity.");
            }

            this.X = x;
            this.Y = y;
            this.Z = 0;
        }

        /// <summary>
        /// Get the length of this vector.
        /// </summary>
        public double Length()
        {
            return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
        }

        /// <summary>
        /// Return a new vector which is the unitized version of this vector.
        /// </summary>
        public Vector3 Unitized()
        {
            var length = Length();
            if (length == 0)
            {
                return this;
            }
            return new Vector3(X / length, Y / length, Z / length);
        }

        /// <summary>
        /// Compute the cross product of this vector and v.
        /// </summary>
        /// <param name="v">The vector with which to compute the cross product.</param>
        public Vector3 Cross(Vector3 v)
        {
            var x = Y * v.Z - Z * v.Y;
            var y = Z * v.X - X * v.Z;
            var z = X * v.Y - Y * v.X;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Compute the dot product of this vector and v.
        /// </summary>
        /// <param name="v">The vector with which to compute the dot product.</param>
        /// <returns>The dot product.</returns>
        public double Dot(Vector3 v)
        {
            return v.X * this.X + v.Y * this.Y + v.Z * this.Z;
        }

        /// <summary>
        /// The angle in degrees from this vector to the provided vector. 
        /// Note that for angles in the plane that can be greater than 180 degrees, 
        /// you should use Vector3.PlaneAngleTo.
        /// </summary>
        /// <param name="v">The vector with which to measure the angle.</param>
        /// <returns>The angle in degrees between 0 and 180. </returns>
        public double AngleTo(Vector3 v)
        {
            var rad = Math.Acos((Dot(v) / (Length() * v.Length())));
            return rad * 180 / Math.PI;
        }

        /// <summary>
        /// Calculate a counter-clockwise plane angle between this vector and the provided vector in the XY plane.
        /// </summary>
        /// <param name="v">The vector with which to measure the angle.</param>
        /// <returns>Angle in degrees between 0 and 360, or NaN if the projected input vectors are invalid.</returns>
        public double PlaneAngleTo(Vector3 v)
        {
            return PlaneAngleTo(v, ZAxis);
        }

        /// <summary>
        /// Calculate a counter-clockwise plane angle between this vector and the provided vector, projected to the plane perpendicular to the provided normal.
        /// </summary>
        /// <param name="v">The vector with which to measure the angle.</param>
        /// <param name="normal">The normal of the plane in which you wish to calculate the angle.</param>
        /// <returns>Angle in degrees between 0 and 360, or NaN if the projected input vectors are invalid.</returns>
        public double PlaneAngleTo(Vector3 v, Vector3 normal)
        {
            var transformFromPlane = new Transform(Vector3.Origin, normal);
            transformFromPlane.Invert();
            var thisTransformed = transformFromPlane.OfVector(this);
            var otherTransformed = transformFromPlane.OfVector(v);
            // project to Plane
            Vector3 a = new Vector3(thisTransformed.X, thisTransformed.Y, 0);
            Vector3 b = new Vector3(otherTransformed.X, otherTransformed.Y, 0);

            // reject very small vectors
            if (a.Length() < Vector3.EPSILON || b.Length() < Vector3.EPSILON)
            {
                return double.NaN;
            }

            Vector3 aUnitized = a.Unitized();
            Vector3 bUnitized = b.Unitized();

            // Cos^-1(a dot b), a dot b clamped to [-1, 1]
            var angle = Math.Acos(Math.Max(Math.Min(aUnitized.Dot(bUnitized), 1.0), -1.0));
            if (Math.Abs(angle) < Vector3.EPSILON)
            {
                return 0;
            }
            // check if should be reflex angle
            Vector3 aCrossB = aUnitized.Cross(bUnitized).Unitized();
            if (Vector3.ZAxis.Dot(aCrossB) > 0)
            {
                return angle * 180 / Math.PI;
            }
            else
            {
                return (Math.PI * 2 - angle) * 180 / Math.PI;
            }
        }

        #region DistanceTo methods
        /// <summary>
        /// The distance from this point to b.
        /// </summary>
        /// <param name="v">The target vector.</param>
        /// <returns>The distance between this vector and the provided vector.</returns>
        public double DistanceTo(Vector3 v)
        {
            return Math.Sqrt(Math.Pow(this.X - v.X, 2) + Math.Pow(this.Y - v.Y, 2) + Math.Pow(this.Z - v.Z, 2));
        }

        /// <summary>
        /// The distance from this point to the plane.
        /// The distance will be negative when this point lies
        /// "behind" the plane.
        /// </summary>
        /// <param name="p">The plane.</param>
        public double DistanceTo(Plane p)
        {
            var d = p.Origin.Dot(p.Normal);
            return this.Dot(p.Normal) - d;
        }

        /// <summary>
        /// Find the distance from this point to the line, and output the location 
        /// of the closest point on that line.
        /// Using formula from https://diego.assencio.com/?index=ec3d5dfdfc0b6a0d147a656f0af332bd
        /// </summary>
        /// <param name="line">The line to find the distance to.</param>
        /// <param name="closestPoint">The point on the line that is closest to this point.</param>
        public double DistanceTo(Line line, out Vector3 closestPoint)
        {
            var lambda = (this - line.Start).Dot(line.End - line.Start) / (line.End - line.Start).Dot(line.End - line.Start);
            if (lambda >= 1)
            {
                closestPoint = line.End;
                return this.DistanceTo(line.End);
            }
            else if (lambda <= 0)
            {
                closestPoint = line.Start;
                return this.DistanceTo(line.Start);
            }
            else
            {
                closestPoint = (line.Start + lambda * (line.End - line.Start));
                return this.DistanceTo(closestPoint);
            }
        }

        /// <summary>
        /// Find the distance from this point to the line.
        /// </summary>
        /// <param name="line"></param>
        public double DistanceTo(Line line)
        {
            return DistanceTo(line, out var _);
        }

        /// <summary>
        /// Find the shortest distance from this point to any point on the
        /// polyline, and output the location of the closest point on that polyline.
        /// </summary>
        /// <param name="polyline">The polyline for computing the distance.</param>
        /// <param name="closestPoint">The point on the polyline that is closest to this point.</param>
        public double DistanceTo(Polyline polyline, out Vector3 closestPoint)
        {
            var closest = double.MaxValue;
            closestPoint = default(Vector3);

            foreach (var line in polyline.Segments())
            {
                var distance = this.DistanceTo(line, out var thisClosestPoint);
                if (distance < closest)
                {
                    closest = distance;
                    closestPoint = thisClosestPoint;
                }
            }

            return closest;
        }

        /// <summary>
        /// Find the shortest distance from this point to any point on the
        /// polyline, and output the location of the closest point on that polyline.
        /// </summary>
        /// <param name="polyline">The polyline for computing the distance.</param>
        public double DistanceTo(Polyline polyline)
        {
            var closest = double.MaxValue;

            foreach (var line in polyline.Segments())
            {
                var distance = this.DistanceTo(line);
                if (distance < closest)
                {
                    closest = distance;
                }
            }

            return closest;
        }
        #endregion

        /// <summary>
        /// Compute the average of this Vector3 and v.
        /// </summary>
        /// <param name="v">The vector with which to compute the average.</param>
        /// <returns>A vector which is the average of this and v.</returns>
        public Vector3 Average(Vector3 v)
        {
            return new Vector3((this.X + v.X) / 2, (this.Y + v.Y) / 2, (this.Z + v.Z) / 2);
        }

        /// <summary>
        /// Project vector a onto this vector.
        /// </summary>
        /// <param name="a">The vector to project onto this vector.</param>
        /// <returns>A new vector which is the projection of a onto this vector.</returns>
        public Vector3 ProjectOnto(Vector3 a)
        {
            var b = this;
            return (a.Dot(b) / Math.Pow(a.Length(), 2)) * a;
        }

        /// <summary>
        /// Multiply a vector and a scalar.
        /// </summary>
        /// <param name="v">The vector to multiply.</param>
        /// <param name="a">The scalar value to multiply.</param>
        /// <returns>A vector whose magnitude is multiplied by a.</returns>
        public static Vector3 operator *(Vector3 v, double a)
        {
            return new Vector3(v.X * a, v.Y * a, v.Z * a);
        }

        /// <summary>
        /// Multiply a scalar and a vector.
        /// </summary>
        /// <param name="a">The scalar value to multiply.</param>
        /// <param name="v">The vector to multiply.</param>
        /// <returns>A vector whose magnitude is multiplied by a.</returns>
        public static Vector3 operator *(double a, Vector3 v)
        {
            return new Vector3(v.X * a, v.Y * a, v.Z * a);
        }

        /// <summary>
        /// Divide a vector by a scalar.
        /// </summary>
        /// <param name="a">The scalar divisor.</param>
        /// <param name="v">The vector to divide.</param>
        /// <returns>A vector whose magnitude is multiplied by a.</returns>
        public static Vector3 operator /(Vector3 v, double a)
        {
            return new Vector3(v.X / a, v.Y / a, v.Z / a);
        }

        /// <summary>
        /// Subtract two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>A vector which is the difference between a and b.</returns>
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3((a.X - b.X), (a.Y - b.Y), (a.Z - b.Z));
        }

        /// <summary>
        /// Add two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>A vector which is the sum of a and b.</returns>
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3((a.X + b.X), (a.Y + b.Y), (a.Z + b.Z));
        }

        /// <summary>
        /// Compute whether all components of vector a are greater than those of vector b.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>True if all of a's components are greater than those of b, otherwise false.</returns>
        public static bool operator >(Vector3 a, Vector3 b)
        {
            return a.X > b.X && a.Y > b.Y && a.Z > b.Z;
        }

        /// <summary>
        /// Compute whether all components of vector a are less than those of vector b.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>True if all of a's components are less than those of b, otherwise false.</returns>
        public static bool operator <(Vector3 a, Vector3 b)
        {
            return a.X < b.X && a.Y < b.Y && a.Z < b.Z;
        }

        /// <summary>
        /// Are the two vectors the same within Epsilon?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Are the two vectors not the same within Epsilon?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Determine whether this vector is parallel to v.
        /// </summary>
        /// <param name="v">The vector to compare to this vector.</param>
        /// <returns>True if the vectors are parallel, otherwise false.</returns>
        public bool IsParallelTo(Vector3 v)
        {
            var result = Math.Abs(this.Unitized().Dot(v.Unitized()));
            return result.ApproximatelyEquals(1);
        }

        /// <summary>
        /// Construct a new vector which is the inverse of this vector.
        /// </summary>
        /// <returns>A new vector which is the inverse of this vector.</returns>
        public Vector3 Negate()
        {
            return new Vector3(-X, -Y, -Z);
        }

        /// <summary>
        /// Convert a vector's components to an array.
        /// </summary>
        /// <returns>An array of comprised of the x, y, and z components of this vector.</returns>
        public double[] ToArray()
        {
            return new[] { X, Y, Z };
        }

        /// <summary>
        /// A string representation of the vector.
        /// </summary>
        /// <returns>The string representation of this vector.</returns>
        public override string ToString()
        {
            return $"X:{this.X.ToString("F4")},Y:{this.Y.ToString("F4")},Z:{this.Z.ToString("F4")}";
        }

        /// <summary>
        /// Determine whether this vector's components are equal to those of v, within tolerance.
        /// </summary>
        /// <param name="v">The vector to compare.</param>
        /// <returns>True if the difference of this vector and the supplied vector's components are all within Tolerance, otherwise false.</returns>
        public bool IsAlmostEqualTo(Vector3 v)
        {
            if (Math.Abs(this.X - v.X) < EPSILON &&
                Math.Abs(this.Y - v.Y) < EPSILON &&
                Math.Abs(this.Z - v.Z) < EPSILON)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine whether this vector's components are equal to the provided components, within tolerance.
        /// </summary>
        /// <param name="x">The x component to compare.</param>
        /// <param name="y">The y component to compare.</param>
        /// <param name="z">The z component to compare.</param>
        /// <returns>True if the difference of this vector and the supplied vector's components are all within Tolerance, otherwise false.</returns>
        public bool IsAlmostEqualTo(double x, double y, double z = 0)
        {
            return IsAlmostEqualTo(new Vector3(x, y, z));
        }

        /// <summary>
        /// Project this vector onto the plane.
        /// </summary>
        /// <param name="p">The plane on which to project the point.</param>
        public Vector3 Project(Plane p)
        {
            //Ax+By+Cz+d=0
            //p' = p - (n ⋅ (p - o)) * n
            var d = -p.Origin.X * p.Normal.X - p.Origin.Y * p.Normal.Y - p.Origin.Z * p.Normal.Z;
            var p1 = this - (p.Normal.Dot(this - p.Origin)) * p.Normal;
            return p1;
        }

        /// <summary>
        /// Project this vector onto the plane along a vector.
        /// </summary>
        /// <param name="v">The vector along which t project.</param>
        /// <param name="p">The plane on which to project.</param>
        /// <returns>A point on the plane.</returns>
        public Vector3 ProjectAlong(Vector3 v, Plane p)
        {
            var x = ((p.Origin - this).Dot(p.Normal)) / p.Normal.Dot(v.Unitized());
            return this + x * v;
        }


        /// <summary>
        /// Implement IComparable interface.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public int CompareTo(Vector3 v)
        {
            if (this > v)
            {
                return -1;
            }
            if (this.Equals(v))
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Is this vector equal to the provided vector?
        /// </summary>
        /// <param name="other">The vector to test.</param>
        /// <returns>Returns true if all components of the two vectors are within Epsilon, otherwise false.</returns>
        public bool Equals(Vector3 other)
        {
            return this.IsAlmostEqualTo(other);
        }

        /// <summary>
        /// Are any components of this vector NaN?
        /// </summary>
        /// <returns>True if any components are NaN otherwise false.</returns>
        public bool IsNaN()
        {
            return Double.IsNaN(this.X) || Double.IsNaN(this.Y) || Double.IsNaN(this.Z);
        }

        /// <summary>
        /// Is this vector zero length?
        /// </summary>
        /// <returns>True if this vector's components are all less than Epsilon.</returns>
        public bool IsZero()
        {
            return Math.Abs(this.X) < Vector3.EPSILON && Math.Abs(this.Y) < Vector3.EPSILON && Math.Abs(this.Z) < Vector3.EPSILON;
        }

        /// <summary>
        /// Check if two vectors are coplanar.
        /// </summary>
        /// <param name="b">The second vector.</param>
        /// <param name="c">The third vector.</param>
        /// <returns>True is the vectors are coplanar, otherwise false.</returns>
        public double TripleProduct(Vector3 b, Vector3 c)
        {
            // https://en.wikipedia.org/wiki/Triple_product
            var a = this;
            var prod = a.Dot(b.Cross(c));
            return prod;
        }

        /// <summary>
        /// Get the closest point on the line from this point.
        /// </summary>
        /// <param name="line">The line on which to find the closest point.</param>
        /// <returns>The closest point on the line from this point.</returns>
        public Vector3 ClosestPointOn(Line line)
        {
            var dir = line.Direction();
            var v = this - line.Start;
            var d = v.Dot(dir);
            d = Math.Min(line.Length(), d);
            d = Math.Max(d, 0);
            return line.Start + dir * d;
        }

        /// <summary>
        /// Check whether three points are wound CCW in two dimensions.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <returns>Greater than 0 if the points are CCW, less than 0 if they are CW, and 0 if they are colinear.</returns>
        public static double CCW(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
        }
    }

    /// <summary>
    /// Extension methods for Vector3.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Are the provided points on the same plane?
        /// </summary>
        /// <param name="points"></param>
        public static bool AreCoplanar(this IList<Vector3> points)
        {
            if (points.Count < 3) return true;

            //TODO: https://github.com/hypar-io/sdk/issues/54
            // Ensure that all triple products are equal to 0.
            // a.Dot(b.Cross(c));
            var a = points[0];
            var b = points[1];
            var c = points[2];
            var ab = b - a;
            var ac = c - a;
            for (var i = 3; i < points.Count; i++)
            {
                var d = points[i];
                var cd = d - a;
                var tp = ab.Dot(ac.Cross(cd));
                if (Math.Abs(tp) > Vector3.EPSILON)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Are the provided points along the same line?
        /// </summary>
        /// <param name="points"></param>
        public static bool AreCollinear(this IList<Vector3> points)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("Cannot test collinearity of an empty list");
            }
            if (points.Count < 3)
            {
                return true;
            }
            var testVector = (points[1] - points[0]).Unitized();
            // in general this loop should not execute. This is just a check in case the first two points are
            // coincident.
            while (testVector.IsZero()) //loop until you find an initial vector that isn't zero-length
            {
                points.RemoveAt(0);
                if (points.Count < 3)
                {
                    return true;
                }
                testVector = (points[1] - points[0]).Unitized();
            }
            for (int i = 2; i < points.Count; i++)
            {
                var nextVector = (points[i] - points[i - 1]).Unitized();
                if (nextVector.IsZero()) // coincident points may be safely skipped
                {
                    continue;
                }
                if (Math.Abs(nextVector.Dot(testVector)) < (1 - Vector3.EPSILON))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Compute a transform with the origin at points[0], with
        /// an X axis along points[1]->points[0], and a normal
        /// computed using the vectors points[2]->points[1] and 
        /// points[1]->points[0].
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Transform ToTransform(this IList<Vector3> points)
        {
            var a = (points[1] - points[0]).Unitized();
            // We need to search for a second vector that is not colinear 
            // with the first. If all the vectors are tried, and one isn't
            // found that's not parallel to the first, you'll 
            // get a zero-length normal.
            Vector3 b = new Vector3();
            for (var i = 2; i < points.Count; i++)
            {
                b = (points[i] - points[1]).Unitized();
                var dot = b.Dot(a);
                if (dot > -1 + Vector3.EPSILON && dot < 1 - Vector3.EPSILON)
                {
                    // Console.WriteLine("Found valid second vector.");
                    break;
                }
            }

            var n = b.Cross(a);
            var t = new Transform(points[0], a, n);
            return t;
        }

        /// <summary>
        /// Find the average of a collection of Vector3.
        /// </summary>
        /// <param name="points">The Vector3 collection to average.</param>
        /// <returns>A Vector3 representing the average.</returns>
        public static Vector3 Average(this IList<Vector3> points)
        {
            double x = 0.0, y = 0.0, z = 0.0;
            foreach (var p in points)
            {
                x += p.X;
                y += p.Y;
                z += p.Z;
            }
            return new Vector3(x / points.Count, y / points.Count, z / points.Count);
        }

        /// <summary>
        /// Shrink a collection of Vector3 towards their average.
        /// </summary>
        /// <param name="points">The collection of Vector3 to shrink.</param>
        /// <param name="distance">The distance to shrink along the vector to average.</param>
        /// <returns></returns>
        public static Vector3[] Shrink(this Vector3[] points, double distance)
        {
            var avg = points.Average();
            var shrink = new Vector3[points.Length];
            for (var i = 0; i < shrink.Length; i++)
            {
                var p = points[i];
                shrink[i] = p + (avg - p).Unitized() * distance;
            }
            return shrink;
        }

        /// <summary>
        /// Convert a collection of Vector3 to a flat array of double.
        /// </summary>
        /// <param name="points">The collection of Vector3 to convert.</param>
        /// <returns>An array containing x,y,z,x1,y1,z1,x2,y2,z2,...</returns>
        public static double[] ToArray(this IList<Vector3> points)
        {
            var arr = new double[points.Count * 3];
            var c = 0;
            for (var i = 0; i < points.Count; i++)
            {
                var v = points[i];
                arr[c] = v.X;
                arr[c + 1] = v.Y;
                arr[c + 2] = v.Z;
                c += 3;
            }
            return arr;
        }
    }
}