using Elements.Validators;
using Newtonsoft.Json;
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

        /// <summary>
        /// A tolerance for angle comparison operation of cos of 0.001 degrees.
        /// </summary>
        public const double COS_ANGLE_EPSILON = 0.99999999984769128;

        private static Vector3 _xAxis = new Vector3(1, 0, 0);
        private static Vector3 _yAxis = new Vector3(0, 1, 0);
        private static Vector3 _zAxis = new Vector3(0, 0, 1);
        private static Vector3 _origin = new Vector3();
        private static readonly Vector3 _min = new Vector3(double.MinValue, double.MinValue, double.MinValue);
        private static readonly Vector3 _max = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);

        /// <summary>
        /// Create a vector.
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <param name="z">The z component.</param>
        [JsonConstructor]
        public Vector3(double @x, double @y, double @z)
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                if (Double.IsNaN(x) || Double.IsNaN(y) || Double.IsNaN(z))
                {
                    throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was NaN.");
                }

                if (Double.IsInfinity(x) || Double.IsInfinity(y) || Double.IsInfinity(z))
                {
                    throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was infinity.");
                }
            }

            this.X = @x;
            this.Y = @y;
            this.Z = @z;
        }

        /// <summary>The X component of the vector.</summary>
        [JsonProperty("X", Required = Required.Always)]
        public double X { get; set; }

        /// <summary>The Y component of the vector.</summary>
        [JsonProperty("Y", Required = Required.Always)]
        public double Y { get; set; }

        /// <summary>The Z component of the vector.</summary>
        [JsonProperty("Z", Required = Required.Always)]
        public double Z { get; set; }

        /// <summary>
        /// Create a vector at the origin.
        /// </summary>
        /// <returns></returns>
        public static Vector3 Origin
        {
            get { return _origin; }
        }

        /// <summary>
        /// The smallest possible value of a Vector3.
        /// </summary>
        public static Vector3 Min
        {
            get { return _min; }
        }

        /// <summary>
        /// The largest possible value of a Vector3.
        /// </summary>
        public static Vector3 Max
        {
            get { return _max; }
        }

        /// <summary>
        /// Get the hash code for the vector.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Z.GetHashCode();
                return hash;
            }
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
            var l = line.Length();
            var div = l / (double)(n + 1);
            var pts = new List<Vector3>();
            for (var t = 0.0; t <= l; t += div)
            {
                var pt = line.PointAt(t);

                if ((t == 0.0 && !includeEnds) || (t == l && !includeEnds))
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
            return Math.Sqrt(this.LengthSquared());
        }

        /// <summary>
        /// Get the squared length of this vector.
        /// </summary>
        public double LengthSquared()
        {
            return Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2);
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
        /// Is this vector of unit length?
        /// </summary>
        /// <returns>True if the vector is of unit length, otherwise false.</returns>
        public bool IsUnitized()
        {
            return this.Length().ApproximatelyEquals(1.0);
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
        /// Compute the cross product of this vector and a vector composed
        /// of the provided components.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        public Vector3 Cross(double x, double y, double z)
        {
            var xx = Y * z - Z * y;
            var yy = Z * x - X * z;
            var zz = X * y - Y * x;

            return new Vector3(xx, yy, zz);
        }

        /// <summary>
        /// Compute the dot product of this vector and v.
        /// </summary>
        /// <param name="v">The vector with which to compute the dot product.</param>
        /// <returns>A value between 1 and -1.</returns>
        public double Dot(Vector3 v)
        {
            return v.X * this.X + v.Y * this.Y + v.Z * this.Z;
        }

        /// <summary>
        /// Compute the dot product of this vector and a vector composed
        /// of the provided components.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <returns>A value between 1 and -1.</returns>
        public double Dot(double x, double y, double z)
        {
            return x * this.X + y * this.Y + z * this.Z;
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
            return Units.RadiansToDegrees(AngleToInternal(v));
        }

        internal double AngleToInternal(Vector3 v)
        {
            var n = Dot(v);
            var d = Length() * v.Length();
            if (d == 0.0)
            {
                // Avoid a division by zero below.
                return 0;
            }
            var r = n / d;
            if (r.ApproximatelyEquals(1.0))
            {
                return 0.0;
            }
            if (r.ApproximatelyEquals(-1.0))
            {
                return Math.PI;
            }
            var rad = Math.Acos(r);
            return rad;
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
            return Units.RadiansToDegrees(PlaneAngleToInternal(v, normal));
        }

        internal double PlaneAngleToInternal(Vector3 v, Vector3 normal)
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
                return angle;
            }
            else
            {
                return Math.PI * 2 - angle;
            }
        }

        #region DistanceTo methods
        /// <summary>
        /// The distance from this point to v.
        /// </summary>
        /// <param name="v">The target vector.</param>
        /// <returns>The distance between this vector and the provided vector.</returns>
        public double DistanceTo(Vector3 v)
        {
            return Math.Sqrt(Math.Pow(this.X - v.X, 2) + Math.Pow(this.Y - v.Y, 2) + Math.Pow(this.Z - v.Z, 2));
        }

        /// <summary>
        /// The distance from this point to the ray.
        /// The ray is treated as being infinitely long.
        /// </summary>
        /// <param name="ray">The target ray.</param>
        public double DistanceTo(Ray ray)
        {
            var t = ProjectedParameterOn(ray);
            if (Double.IsNaN(t))
            {
                return double.PositiveInfinity;
            }
            var closestPointOnRay = ray.Origin + t * ray.Direction;
            return closestPointOnRay.DistanceTo(this);
        }

        internal double ProjectedParameterOn(Ray ray)
        {
            return ray.Direction.Dot(this - ray.Origin) / ray.Direction.Length(); // t will be [0,1]
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
            return DistanceToEdgeInternal(line.Start, line.End, out closestPoint);
        }

        /// <summary>
        /// Find the distance from this point to the edge, and output the location
        /// of the closest point on that line.
        /// Using formula from https://diego.assencio.com/?index=ec3d5dfdfc0b6a0d147a656f0af332bd
        /// </summary>
        /// <param name="edge">The edge to find the distance to.</param>
        /// <param name="closestPoint">The point on the line that is closest to this point.</param>
        public double DistanceTo((Vector3 from, Vector3 to) edge, out Vector3 closestPoint)
        {
            return DistanceToEdgeInternal(edge.from, edge.to, out closestPoint);
        }

        private double DistanceToEdgeInternal(Vector3 start, Vector3 end, out Vector3 closestPoint)
        {
            var d1 = this - start;
            var d2 = end - start;

            var lambda = d1.Dot(d2) / d2.Dot(d2);
            if (lambda >= 1)
            {
                closestPoint = end;
                return this.DistanceTo(end);
            }
            else if (lambda <= 0)
            {
                closestPoint = start;
                return this.DistanceTo(start);
            }
            else
            {
                closestPoint = start + lambda * d2;
                return this.DistanceTo(closestPoint);
            }
        }

        /// <summary>
        /// Find the distance from this point to the line.
        /// </summary>
        /// <param name="line"></param>
        public double DistanceTo(Line line)
        {
            return DistanceTo(line, out _);
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
            return this.DistanceTo(polyline, out _);
        }

        /// <summary>
        /// Find the shortest distance from this point to any point within the
        /// polygon, and output the location of the closest point on that polygon.
        /// </summary>
        /// <param name="polygon">The polygon for computing the distance</param>
        /// <param name="closestPoint">Point within the polygon that is closest to this point</param>
        /// <returns></returns>
        public double DistanceTo(Polygon polygon, out Vector3 closestPoint)
        {
            var pointOnPolygonPlane = this.Project(polygon.Plane());
            if (polygon.Contains(pointOnPolygonPlane, out _))
            {
                closestPoint = pointOnPolygonPlane;
                return this.DistanceTo(pointOnPolygonPlane);
            }
            else
            {
                return this.DistanceTo(new Polyline(polygon.Vertices), out closestPoint);
            }
        }

        /// <summary>
        /// Find the shortest distance from this point to any point within the polygon
        /// </summary>
        /// <param name="polygon">The polygon for computing the distance</param>
        /// <returns></returns>
        public double DistanceTo(Polygon polygon)
        {
            return this.DistanceTo(polygon, out _);
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
            return (a.Dot(b) / a.LengthSquared()) * a;
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
        /// Compute whether all components of vector a are greater than or
        /// equal to those of vector b.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>True if all of a's components are greater than or equal
        /// to all of those of b, otherwise false.</returns>
        public static bool operator >=(Vector3 a, Vector3 b)
        {
            return a.X >= b.X && a.Y >= b.Y && a.Z >= b.Z;
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
        /// Compute whether all components of vector a are less than or
        /// equal to those of vector b.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>True if all of a's components are less than or equal
        /// to all of those of b, otherwise false.</returns>
        public static bool operator <=(Vector3 a, Vector3 b)
        {
            return a.X <= b.X && a.Y <= b.Y && a.Z <= b.Z;
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
        /// <param name="tolerance">The amount of tolerance in the parallel comparison.</param>
        /// <returns>True if the vectors are parallel, otherwise false.</returns>
        public bool IsParallelTo(Vector3 v, double tolerance = Vector3.EPSILON)
        {
            var result = Math.Abs(this.Unitized().Dot(v.Unitized()));
            return result.ApproximatelyEquals(1, tolerance);
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
            return $"X:{this.X.ToString("F4")}, Y:{this.Y.ToString("F4")}, Z:{this.Z.ToString("F4")}";
        }

        /// <summary>
        /// Determine whether this vector's components are equal to those of v, within tolerance.
        /// </summary>
        /// <param name="v">The vector to compare.</param>
        /// <param name="tolerance">Optional custom tolerance value.</param>
        /// <returns>True if the difference of this vector and the supplied vector's components are all within Tolerance, otherwise false.</returns>
        public bool IsAlmostEqualTo(Vector3 v, double tolerance = Vector3.EPSILON)
        {
            if ((this.X - v.X) * (this.X - v.X)
              + (this.Y - v.Y) * (this.Y - v.Y)
              + (this.Z - v.Z) * (this.Z - v.Z) < (tolerance * tolerance))
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
            var p1 = this - p.Normal.Dot(this - p.Origin) * p.Normal;
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
        /// Is this vector equal to the provided vector?
        /// </summary>
        /// <param name="other">The vector to test.</param>
        /// <returns>Returns true if all components of the two vectors are within Epsilon, otherwise false.</returns>
        public override bool Equals(object other)
        {
            if (!(other is Vector3))
            {
                return false;
            }
            var v = (Vector3)other;
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
        /// Get the closest point on the infinite line from this point.
        /// </summary>
        /// <param name="line">The infinite line on which to find the closest point.</param>
        /// <returns>The closest point on the infinite line from this point.</returns>
        public Vector3 ClosestPointOn(InfiniteLine line)
        {
            var v = this - line.Origin;
            var d = v.Dot(line.Direction);
            return line.Origin + line.Direction * d;
        }


        /// <summary>
        /// Get the closest point on the line from this point.
        /// </summary>
        /// <param name="line">The line on which to find the closest point.</param>
        /// <param name="infinite">If true, line will be treated as infinite. (False by default)</param>
        /// <returns>The closest point on the line from this point.</returns>
        public Vector3 ClosestPointOn(Line line, bool infinite = false)
        {
            var dir = line.Direction();
            var v = this - line.Start;
            var d = v.Dot(dir);
            if (!infinite)
            {
                d = Math.Min(line.Length(), d);
                d = Math.Max(d, 0);
            }
            return line.Start + dir * d;
        }

        /// <summary>
        /// Check whether three points are wound CCW in two dimensions.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <returns>Greater than 0 if the points are CCW, less than 0 if they are CW, and 0 if they are collinear.</returns>
        public static double CCW(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
        }

        /// <summary>
        /// Check whether three points are on the same line.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <returns>True if the points are on the same line, false otherwise.</returns>
        [Obsolete("Use AreCollinearByDistance or AreCollinearByAngle instead")]
        public static bool AreCollinear(Vector3 a, Vector3 b, Vector3 c)
        {
            return AreCollinearByDistance(a, b, c);
        }

        /// <summary>
        /// Check whether three points are on the same line within certain distance.
        /// This function is slower than AreCollinearByAngle and less suitable for high complexity code.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <param name="tolerance">Distance tolerance.</param>
        /// <returns>True if the points are on the same line, false otherwise.</returns>
        public static bool AreCollinearByDistance(Vector3 a, Vector3 b, Vector3 c, double tolerance = Vector3.EPSILON)
        {
            var vectorList = new List<Vector3> { a, b, c };
            return vectorList.AreCollinearByDistance(tolerance);
        }

        /// <summary>
        /// Check whether three points are on the same line within certain angle.
        /// Order is important since unsigned abc angle is checked.
        /// This function is much faster than AreCollinearByDistance but angle deviation scales with the distance of points being compared.
        /// If points are far away from each other they might appear collinear even if there are large distance offsets between them.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <param name="cosAngleTolerance">Angle tolerance as cos.</param>
        /// <returns></returns>
        public static bool AreCollinearByAngle(Vector3 a, Vector3 b, Vector3 c, double cosAngleTolerance = Vector3.COS_ANGLE_EPSILON)
        {
            var baX = b.X - a.X;
            var baY = b.Y - a.Y;
            var baZ = b.Z - a.Z;
            var baLength = Math.Sqrt(Math.Pow(baX, 2) + Math.Pow(baY, 2) + Math.Pow(baZ, 2));
            if (baLength < Vector3.EPSILON)
            {
                return true;
            }
            baX = baX / baLength;
            baY = baY / baLength;
            baZ = baZ / baLength;

            var cbX = c.X - b.X;
            var cbY = c.Y - b.Y;
            var cbZ = c.Z - b.Z;
            var cbLength = Math.Sqrt(Math.Pow(cbX, 2) + Math.Pow(cbY, 2) + Math.Pow(cbZ, 2));
            if (cbLength < Vector3.EPSILON)
            {
                return true;
            }
            cbX = cbX / cbLength;
            cbY = cbY / cbLength;
            cbZ = cbZ / cbLength;

            return Math.Abs(baX * cbX + baY * cbY + baZ * cbZ) > cosAngleTolerance;
        }

        /// <summary>
        /// Are four provided points on the same plane?
        /// </summary>
        public static bool AreCoplanar(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var ab = b - a;
            var ac = c - a;
            var cd = d - a;
            var tp = ab.TripleProduct(ac, cd);
            return Math.Abs(tp) < EPSILON;
        }

        /// <summary>
        /// Are the provided points within tolerance of each other?
        /// </summary>
        /// <param name="points">The points to compare.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>True if all points are within tolerance of each other, otherwise false.</returns>
        public static bool AreApproximatelyEqual(IEnumerable<Vector3> points, double tolerance = Vector3.EPSILON)
        {
            if (points.Count() < 2)
            {
                return true;
            }
            var average = points.Average();
            // we halve the tolerance to guarantee that all points in the set are
            // within tolerance of each other. If all points are within
            // tolerance/2 of some point, then they must all be within tolerance
            // of each other.
            return points.All(p => p.IsAlmostEqualTo(average, tolerance / 2.0)); 
        }

        /// <summary>
        /// Compute basis vectors for this vector.
        /// By default, the cross product of the world Z axis and this vector
        /// are used to compute the U direction. If this vector is parallel
        /// the world Z axis, then the world Y axis is used instead.
        /// </summary>
        public (Vector3 U, Vector3 V) ComputeDefaultBasisVectors()
        {
            var u = (this.IsParallelTo(Vector3.ZAxis) ? Vector3.YAxis : Vector3.ZAxis).Cross(this).Unitized();
            var v = this.Cross(u).Unitized();
            return (u, v);
        }

        /// <summary>
        /// Remove sequential duplicates from a list of points.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="wrap">Whether or not to assume a closed shape like a polygon. If true, the last vertex will be compared to the first, and deleted if identical.</param>
        /// <param name="tolerance">An optional distance tolerance for the comparison.</param>
        /// <returns></returns>
        internal static List<Vector3> RemoveSequentialDuplicates(IList<Vector3> vertices, bool wrap = false, double tolerance = Vector3.EPSILON)
        {
            List<Vector3> newList = new List<Vector3> { vertices[0] };
            for (int i = 1; i < vertices.Count; i++)
            {
                var vertex = vertices[i];
                var prevVertex = newList[newList.Count - 1];
                if (!vertex.IsAlmostEqualTo(prevVertex, tolerance))
                {
                    // if we wrap, and we're at the last vertex, also check for a zero-length segment between first and last.
                    if (wrap && i == vertices.Count - 1)
                    {
                        if (!vertex.IsAlmostEqualTo(vertices[0], tolerance))
                        {
                            newList.Add(vertex);
                        }
                    }
                    else
                    {
                        newList.Add(vertex);
                    }
                }
            }
            return newList;
        }

        internal static List<Vector3> AttemptPostClipperCleanup(IList<Vector3> vertices)
        {
            var deduplicated = RemoveSequentialDuplicates(vertices, true, Vector3.EPSILON * 2);
            DeleteVerticesForOverlappingEdges(deduplicated, 0.001);
            return deduplicated;
        }

        /// <summary>
        /// Deletes Vertices that are out on overlapping Edges
        /// D__________C
        ///  |         |
        ///  |         |
        /// E|_________|B_____A
        /// Vertex A will be deleted
        /// </summary>
        internal static void DeleteVerticesForOverlappingEdges(IList<Vector3> vertices, double tolerance = Vector3.EPSILON)
        {
            if (vertices.Count < 4)
            {
                return;
            }

            for (var i = 0; i < vertices.Count; i++)
            {
                var a = vertices[i];
                var b = vertices[(i + 1) % vertices.Count];
                var c = vertices[(i + 2) % vertices.Count];
                bool invalid = (a - b).Unitized().Dot((b - c).Unitized()) < (tolerance - 1);
                if (invalid)
                {
                    vertices.Remove(b);
                    i--;

                    if (a.IsAlmostEqualTo(c))
                    {
                        vertices.Remove(c);
                    }
                }
            }
        }

        /// <summary>
        /// A Polygon can't have self intersections, but points can still lay on other lines.
        /// This leads to hidden voids embedded in the perimeter.
        /// This function checks if any points are on another line of the loop and splits into distinct loops if found.
        /// </summary>
        /// <returns>List of simple polygons</returns>
        internal static List<List<Vector3>> SplitInternalLoops(IList<Vector3> vertices)
        {
            List<List<Vector3>> polygonPresets = new List<List<Vector3>>();

            //Store accumulated vertices and lines between them.
            List<Vector3> simpleLoop = new List<Vector3>();
            List<Line> openLoop = new List<Line>();

            //Check if a point lay on active open loop lines.
            foreach (var v in vertices)
            {
                bool intersectionFound = false;
                for (int i = 0; i < openLoop.Count; i++)
                {
                    if (openLoop[i].PointOnLine(v) && v.DistanceTo(openLoop[i]) < Vector3.EPSILON)
                    {
                        //Remove points and lines from intersection points to this.
                        var loopVertices = simpleLoop.Skip(i + 1).ToList();
                        simpleLoop.RemoveRange(i + 1, loopVertices.Count);
                        openLoop.RemoveRange(i + 1, loopVertices.Count - 1);
                        //Cut intersected line and add this point to open loop.
                        simpleLoop.Add(v);
                        openLoop[i] = new Line(openLoop[i].Start, v);

                        //Loop can possibly be just two points connected forth and back.
                        //Filter it early.
                        loopVertices.Add(v);
                        if (loopVertices.Count > 2)
                        {
                            polygonPresets.Add(loopVertices);
                        }
                        intersectionFound = true;
                        break;
                    }
                }

                //Then check if line (this plus last points) intersects with any accumulated points (going backward)
                if (!intersectionFound)
                {
                    Line segment = simpleLoop.Any() ? new Line(simpleLoop.Last(), v) : null;
                    for (int i = simpleLoop.Count - 1; i >= 0; i--)
                    {
                        //Last point is already part of the line.
                        if (i == simpleLoop.Count)
                        {
                            continue;
                        }

                        if (segment.PointOnLine(simpleLoop[i]) && simpleLoop[i].DistanceTo(segment) < Vector3.EPSILON)
                        {
                            var loop = simpleLoop.Skip(i).ToList();
                            segment = new Line(simpleLoop[i], segment.End);

                            simpleLoop.RemoveRange(i + 1, loop.Count - 1);
                            openLoop.RemoveRange(i, loop.Count - 1);

                            if (loop.Count > 2)
                            {
                                polygonPresets.Add(loop);
                            }
                        }
                    }

                    //If no intersection found just add point and line to open loop.
                    simpleLoop.Add(v);
                    if (segment != null)
                    {
                        openLoop.Add(segment);
                    }
                }
            }

            //Leftover points form last loop if it has enough points.
            if (simpleLoop.Count > 2)
            {
                polygonPresets.Add(simpleLoop);
            }

            return polygonPresets;
        }

        /// <summary>
        /// Automatically convert a tuple of three doubles into a Vector3.
        /// </summary>
        /// <param name="vector">An (X,Y,Z) tuple of doubles.</param>
        public static implicit operator Vector3((double X, double Y, double Z) vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Automatically convert a tuple of three ints into a Vector3.
        /// </summary>
        /// <param name="vector">An (X,Y,Z) tuple of ints.</param>
        public static implicit operator Vector3((int X, int Y, int Z) vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Automatically convert a tuple of two doubles into a Vector3.
        /// </summary>
        /// <param name="vector">An (X,Y) tuple of doubles.</param>
        public static implicit operator Vector3((double X, double Y) vector)
        {
            return new Vector3(vector.X, vector.Y);
        }

        /// <summary>
        /// Automatically convert a tuple of two ints into a Vector3.
        /// </summary>
        /// <param name="vector">An (X,Y) tuple of ints.</param>
        public static implicit operator Vector3((int X, int Y) vector)
        {
            return new Vector3(vector.X, vector.Y);
        }

        /// <summary>
        /// Construct X and Y vectors from the provided Z vector.
        /// Construction is done by projecting the Z vector onto the global XY
        /// plane and using the resulting vector to find the right (+X) vector,
        /// then the forward (+Y) vectors.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="zAxis">The z vector.</param>
        /// <returns>A tuple containing the X and Y vectors.</returns>
        internal static (Vector3 X, Vector3 Y) ConstructBasisVectorsFromZAxis(Vector3 origin, Vector3 zAxis)
        {
            Vector3 x = XAxis;
            Vector3 y = YAxis;

            if (!zAxis.IsParallelTo(ZAxis))
            {
                // Project up onto the ortho plane
                var p = new Plane(origin, zAxis);
                var test = ZAxis.Project(p);
                x = test.Cross(zAxis).Unitized();
                y = x.Cross(zAxis.Negate()).Unitized();
            }
            else
            {
                // Ensure that we have a right-handed coordinate system.
                if (zAxis.Dot(ZAxis).ApproximatelyEquals(-1))
                {
                    y = YAxis.Negate();
                }
            }

            return (x, y);
        }
    }
}