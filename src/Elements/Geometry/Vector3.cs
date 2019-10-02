using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A 3D vector.
    /// </summary>
    public partial class Vector3 : IComparable<Vector3>, IEquatable<Vector3>
    {
        /// <summary>
        /// A tolerance for comparison operations.
        /// </summary>
        public static double Tolerance = 0.000000001;

        private static Vector3 _xAxis = new Vector3(1, 0, 0);
        private static Vector3 _yAxis = new Vector3(0, 1, 0);
        private static Vector3 _zAxis = new Vector3(0, 0, 1);
        private static Vector3 _origin = new Vector3();

        /// <summary>
        /// Construct a vector at the origin.
        /// </summary>
        /// <returns></returns>
        public static Vector3 Origin
        {
            get { return _origin; }
        }

        /// <summary>
        /// Is this vector equal to the provided vector?
        /// </summary>
        public override bool Equals(object obj)
        {
            var v = obj as Vector3;
            if (v == null)
            {
                return false;
            }

            return this.IsAlmostEqualTo(v);
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
        /// Construct a vector along the X axis.
        /// </summary>
        public static Vector3 XAxis
        {
            get { return _xAxis; }
        }

        /// <summary>
        /// Construct a vector along the Y axis.
        /// </summary>
        public static Vector3 YAxis
        {
            get { return _yAxis; }
        }

        /// <summary>
        /// Construct a vector along the Z axis.
        /// </summary>
        /// <returns></returns>
        public static Vector3 ZAxis
        {
            get { return _zAxis; }
        }

        /// <summary>
        /// Construct vectors at n equal spaces along the provided line.
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
        /// Construct a default vector at the origin.
        /// </summary>
        public Vector3()
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        /// <summary>
        /// Construct a Vector3 by copying the components of another Vector3.
        /// </summary>
        /// <param name="v">The Vector3 to copy.</param>
        public Vector3(Vector3 v)
        {
            this.X = v.X;
            this.Y = v.Y;
            this.Z = v.Z;
        }

        /// <summary>
        /// Construct a vector from x, y, and z coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the vector.</param>
        /// <param name="y">The y coordinate of the vector.</param>
        /// <param name="z">The z coordinate of the vector.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if any components of the vector are NaN or Infinity.</exception>
        [JsonConstructor]
        public Vector3(double x, double y, double z)
        {
            if(Double.IsNaN(x) || Double.IsNaN(y) || Double.IsNaN(z))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was NaN.");
            }

            if(Double.IsInfinity(x) || Double.IsInfinity(y) || Double.IsInfinity(z))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was infinity.");
            }

            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Construct a vector from x, and y coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the vector.</param>
        /// <param name="y">Thy y coordinate of the vector.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if any components of the vector are NaN or Infinity.</exception>
        public Vector3(double x, double y)
        {
            if(Double.IsNaN(x) || Double.IsNaN(y))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was NaN.");
            }

            if(Double.IsInfinity(x) || Double.IsInfinity(y))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was infinity.");
            }

            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Get the length of this vector.
        /// </summary>
        public double Length()
        {
            return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
        }

        /// <summary>
        /// Return a new vector which is the normalized version of this vector.
        /// </summary>
        /// <returns></returns>
        public Vector3 Normalized()
        {
            var length = Length();
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
        /// </summary>
        /// <param name="v">The vector with which to measure the angle.</param>
        public double AngleTo(Vector3 v)
        {
            var rad = Math.Acos((Dot(v) / (Length() * v.Length())));
            return rad * 180 / Math.PI;
        }

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
        /// <returns>A vector whose magnitude is mutiplied by a.</returns>
        public static Vector3 operator *(double a, Vector3 v)
        {
            return new Vector3(v.X * a, v.Y * a, v.Z * a);
        }

        /// <summary>
        /// Divide a vector by a scalar.
        /// </summary>
        /// <param name="a">The scalar divisor.</param>
        /// <param name="v">The vector to divide.</param>
        /// <returns>A vector whose magnitude is mutiplied by a.</returns>
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
        /// Determine whether this vector is parallel to v.
        /// </summary>
        /// <param name="v">The vector to compare to this vector.</param>
        /// <returns>True if the vectors are parallel, otherwise false.</returns>
        public bool IsParallelTo(Vector3 v)
        {
            var result = Math.Abs(Dot(v));
            return result == 1.0;
        }

        /// <summary>
        /// Construct a new vector which is the inverse of this vector.
        /// </summary>
        /// <returns>A new vector which is the inverse of this vector.</returns>
        public Vector3 Negated()
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
            if (Math.Abs(this.X - v.X) < Tolerance &&
                Math.Abs(this.Y - v.Y) < Tolerance &&
                Math.Abs(this.Z - v.Z) < Tolerance)
            {
                return true;
            }
            return false;
        }

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
        /// The distance from this vector to p.
        /// The distance will be negative when this vector lies
        /// "behind" the plane.
        /// </summary>
        /// <param name="p">The plane.</param>
        public double DistanceTo(Plane p)
        {
            var d = p.Origin.Dot(p.Normal);
            return this.Dot(p.Normal) - d;
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
            var x = ((p.Origin - this).Dot(p.Normal)) / p.Normal.Dot(v.Normalized());
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
        /// Implement the IEquatable interface.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if all the components of this and the provided vector are equal.</returns>
        public bool Equals(Vector3 other)
        {
            var v = other as Vector3;
            if (v == null)
            {
                return false;
            }

            return this.IsAlmostEqualTo(v);
        }

        /// <summary>
        /// Are any components of this vector NaN?
        /// </summary>
        /// <returns>True if any components are NaN otherwise false.</returns>
        public bool IsNaN()
        {
            return Double.IsNaN(this.X) || Double.IsNaN(this.Y) || Double.IsNaN(this.Z);
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
                if (Math.Abs(tp) > Vector3.Tolerance)
                {
                    return false;
                }
            }
            return true;
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
                shrink[i] = p + (avg - p).Normalized() * distance;
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