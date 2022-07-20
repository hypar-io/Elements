using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A right-handed coordinate system with +Z up.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/TransformTests.cs?name=example)]
    /// </example>
    public partial class Transform : IEquatable<Transform>
    {
        /// <summary>
        /// The origin of the transform.
        /// </summary>
        [JsonIgnore]
        public Vector3 Origin => this.Matrix.Translation;

        /// <summary>
        /// The x axis of the transform.
        /// </summary>
        [JsonIgnore]
        public Vector3 XAxis => this.Matrix.XAxis;

        /// <summary>
        /// The y axis of the transform.
        /// </summary>
        [JsonIgnore]
        public Vector3 YAxis => this.Matrix.YAxis;

        /// <summary>
        /// The z axis of the transform.
        /// </summary>
        [JsonIgnore]
        public Vector3 ZAxis => this.Matrix.ZAxis;

        /// <summary>The transform's matrix.</summary>
        [JsonProperty("Matrix", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public Matrix Matrix { get; set; } = new Matrix();

        /// <summary>
        /// Construct a transform.
        /// </summary>
        /// <param name="matrix"></param>
        [JsonConstructor]
        public Transform(Matrix @matrix)
        {
            this.Matrix = @matrix;
        }

        /// <summary>
        /// Create the identity transform.
        /// </summary>
        public Transform()
        {
            this.Matrix = new Matrix();
        }

        /// <summary>
        /// Create a transform by copying another transform.
        /// </summary>
        /// <param name="t">The transform to copy.</param>
        public Transform(Transform t) :
            this(new Matrix(new Vector3(t.XAxis), new Vector3(t.YAxis), new Vector3(t.ZAxis), new Vector3(t.Origin)))
        { }

        /// <summary>
        /// Create a transform with a translation.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        /// <param name="rotation">An optional rotation in degrees around the transform's z axis.</param>
        public Transform(Vector3 origin, double rotation = 0.0)
        {
            this.Matrix = new Matrix();
            this.Matrix.SetupTranslation(origin);
            ApplyRotationAndTranslation(rotation, this.Matrix.ZAxis, Vector3.Origin);
        }

        /// <summary>
        /// Create a transform with a translation.
        /// </summary>
        /// <param name="x">The X component of translation.</param>
        /// <param name="y">The Y component of translation.</param>
        /// <param name="z">The Z component of translation.</param>
        /// <param name="rotation">An optional rotation in degrees around the transform's z axis.</param>
        public Transform(double x, double y, double z, double rotation = 0.0)
        {
            this.Matrix = new Matrix();
            this.Matrix.SetupTranslation(new Vector3(x, y, z));
            ApplyRotationAndTranslation(rotation, this.Matrix.ZAxis, Vector3.Origin);
        }

        /// <summary>
        /// Create a transform centered at origin with its Z axis pointing
        /// along up.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        /// <param name="z">The vector which will define the Z axis of the transform.</param>
        /// <param name="rotation">An optional rotation around the z axis.</param>
        public Transform(Vector3 origin, Vector3 z, double rotation = 0.0)
        {
            Vector3 x = Vector3.XAxis;
            Vector3 y = Vector3.YAxis;

            if (!z.IsParallelTo(Vector3.ZAxis))
            {
                // Project up onto the ortho plane
                var p = new Plane(origin, z);
                var test = Vector3.ZAxis.Project(p);
                x = test.Cross(z).Unitized();
                y = x.Cross(z.Negate()).Unitized();
            }
            else
            {
                // Ensure that we have a right-handed coordinate system.
                if (z.Dot(Vector3.ZAxis).ApproximatelyEquals(-1))
                {
                    y = Vector3.YAxis.Negate();
                }
            }

            this.Matrix = new Matrix(x, y, z, Vector3.Origin);
            ApplyRotationAndTranslation(rotation, z, origin);
        }

        /// <summary>
        /// Create a transform using the provided plane's origin and normal.
        /// </summary>
        /// <param name="plane">The plane used to orient the transform.</param>
        public Transform(Plane plane) : this(plane.Origin, plane.Normal) { }

        private void ApplyRotationAndTranslation(double rotation, Vector3 axis, Vector3 translation)
        {
            if (rotation != 0.0)
            {
                this.Rotate(axis, rotation);
            }
            // Apply translation after rotation.
            this.Move(translation);
        }

        /// <summary>
        /// Create a transform by origin and X and Z axes.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        /// <param name="xAxis">The X axis of the transform.</param>
        /// <param name="zAxis">The Z axis of the transform.</param>
        /// <param name="rotation">An optional rotation in degrees around the transform's z axis.</param>
        public Transform(Vector3 origin, Vector3 xAxis, Vector3 zAxis, double rotation = 0.0)
        {
            var x = xAxis.Unitized();
            var z = zAxis.Unitized();
            var y = z.Cross(x).Unitized();
            this.Matrix = new Matrix(x, y, z, Vector3.Origin);
            ApplyRotationAndTranslation(rotation, z, origin);
        }

        /// <summary>
        /// Create a transform by origin, X, Y, and Z axes. Axes are automatically unitized — to create non-uniform transforms, use Transform.Scale.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        /// <param name="xAxis">The X axis of the transform.</param>
        /// <param name="yAxis">The Y axis of the transform.</param>
        /// <param name="zAxis">The Z axis of the transform.</param>
        public Transform(Vector3 origin,
                         Vector3 xAxis,
                         Vector3 yAxis,
                         Vector3 zAxis)
        {
            this.Matrix = new Matrix(xAxis.Unitized(), yAxis.Unitized(), zAxis.Unitized(), origin);
        }

        /// <summary>
        /// Get a string representation of the transform.
        /// </summary>
        /// <returns>A string representation of the transform.</returns>
        public override string ToString()
        {
            return this.Matrix.ToString();
        }

        /// <summary>
        /// Transform a vector into the coordinate space defined by this transform.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <returns>A new vector transformed by this transform.</returns>
        public Vector3 OfPoint(Vector3 vector)
        {
            return vector * this.Matrix;
        }

        /// <summary>
        /// Transform a vector into the coordinate space defined by this transform ignoring the translation.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <returns>A new vector transformed by this transform.</returns>
        public Vector3 OfVector(Vector3 vector)
        {
            return new Vector3(
                vector.X * XAxis.X + vector.Y * YAxis.X + vector.Z * ZAxis.X,
                vector.X * XAxis.Y + vector.Y * YAxis.Y + vector.Z * ZAxis.Y,
                vector.X * XAxis.Z + vector.Y * YAxis.Z + vector.Z * ZAxis.Z
            );
        }

        /// <summary>
        /// A transformed copy of the supplied curve.
        /// </summary>
        /// <param name="curve">The curve to transform.</param>
        [Obsolete("Use Curve.Transformed(Transform) instead.")]
        public Curve OfCurve(Curve curve)
        {
            return curve.Transformed(this);
        }

        /// <summary>
        /// Transform the specified polygon.
        /// </summary>
        /// <param name="polygon">The polygon to transform.</param>
        /// <returns>A new polygon transformed by this transform.</returns>
        [Obsolete("Use Polygon.Transformed(Transform) instead.")]
        public Polygon OfPolygon(Polygon polygon)
        {
            return polygon.TransformedPolygon(this);
        }

        /// <summary>
        /// Transform the specified polygons.
        /// </summary>
        /// <param name="polygons">The polygons to transform.</param>
        /// <returns>An array of polygons transformed by this transform.</returns>
        public Polygon[] OfPolygons(IList<Polygon> polygons)
        {
            var result = new Polygon[polygons.Count];
            for (var i = 0; i < polygons.Count; i++)
            {
                result[i] = polygons[i].TransformedPolygon(this);
            }
            return result;
        }

        /// <summary>
        /// Transform the specified line.
        /// </summary>
        /// <param name="line">The line to transform.</param>
        /// <returns>A new line transformed by this transform.</returns>
        [Obsolete("Use Line.Transformed(Transform) instead.")]
        public Line OfLine(Line line)
        {
            return line.TransformedLine(this);
        }

        /// <summary>
        /// Transform the specified plane.
        /// </summary>
        /// <param name="plane">The plane to transform.</param>
        /// <returns>A new plane transformed by this transform.</returns>
        public Plane OfPlane(Plane plane)
        {
            return new Plane(OfPoint(plane.Origin), OfVector(plane.Normal));
        }

        /// <summary>
        /// Transform the specified profile.
        /// </summary>
        /// <param name="profile">The profile to transform.</param>
        /// <returns>A new profile transformed by this transform.</returns>
        public Profile OfProfile(Profile profile)
        {
            Polygon[] voids = null;
            if (profile.Voids != null)
            {
                voids = new Polygon[profile.Voids.Count];
                for (var i = 0; i < voids.Length; i++)
                {
                    voids[i] = profile.Voids[i].TransformedPolygon(this);
                }
            }
            var p = new Profile(profile.Perimeter.TransformedPolygon(this), voids, Guid.NewGuid(), null);
            return p;
        }

        /// <summary>
        /// Transform the specifed bezier.
        /// </summary>
        /// <param name="bezier">The bezier to transform.</param>
        /// <returns>A new bezier transformed by this transform.</returns>
        [Obsolete("Use Bezier.Transformed(Transform) instead.")]
        public Bezier OfBezier(Bezier bezier)
        {
            return bezier.TransformedBezier(this);
        }

        /// <summary>
        /// Concatenate the transform.
        /// </summary>
        /// <param name="transform"></param>
        public void Concatenate(Transform transform)
        {
            this.Matrix = this.Matrix * transform.Matrix;
        }

        /// <summary>
        /// Return a new transform which is the supplied transform concatenated to this transform.
        /// </summary>
        /// <param name="transform">The transform to concatenate.</param>
        public Transform Concatenated(Transform transform)
        {
            var result = new Transform(this);
            result.Concatenate(transform);
            return result;
        }

        /// <summary>
        /// Invert this transform.
        /// </summary>
        public void Invert()
        {
            this.Matrix = this.Matrix.Inverted();
        }

        /// <summary>
        /// Return a new transform which is the inverse of this transform.
        /// </summary>
        public Transform Inverted()
        {
            return new Transform(this.Matrix.Inverted());
        }

        /// <summary>
        /// Apply a translation to the transform.
        /// </summary>
        /// <param name="translation">The translation to apply.</param>
        public void Move(Vector3 translation)
        {
            var m = new Matrix();
            m.SetupTranslation(translation);
            this.Matrix = this.Matrix * m;
        }

        /// <summary>
        /// Apply a translation to the transform.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Move(double x = 0.0, double y = 0.0, double z = 0.0)
        {
            Move(new Vector3(x, y, z));
        }

        /// <summary>
        /// Return a new transform which is this transform moved by the specified amount.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Transform Moved(double x = 0.0, double y = 0.0, double z = 0.0)
        {
            var result = new Transform(this);
            result.Move(x, y, z);
            return result;
        }

        /// <summary>
        /// Return a new transform which is this transform moved by the specified amount.
        /// </summary>
        /// <param name="translation">The translation to apply.</param>
        public Transform Moved(Vector3 translation)
        {
            var result = new Transform(this);
            result.Move(translation);
            return result;
        }

        /// <summary>
        /// Apply a rotation to the transform.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation in degrees.</param>
        public void Rotate(Vector3 axis, double angle)
        {
            var m = new Matrix();
            m.SetupRotate(axis, angle * (Math.PI / 180.0));
            this.Matrix = this.Matrix * m;
        }

        /// <summary>
        /// Apply a rotation to the transform around the Z axis.
        /// </summary>
        /// <param name="angle">The angle of rotation in degrees.</param>
        public void Rotate(double angle)
        {
            Rotate(Vector3.ZAxis, angle);
        }

        /// <summary>
        /// Apply a rotation to the transform about a center.
        /// </summary>
        /// <param name="point">The center of rotation.</param>
        /// <param name="axis">The axis direction.</param>
        /// <param name="angle">The angle of rotation in degrees.</param>
        public void RotateAboutPoint(Vector3 point, Vector3 axis, double angle)
        {
            this.Move(point * -1);
            this.Rotate(axis, angle);
            this.Move(point);
        }

        /// <summary>
        /// Return a new transform which is a rotated copy of this transform.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation in degrees.</param>
        public Transform Rotated(Vector3 axis, double angle)
        {
            var result = new Transform(this);
            result.Rotate(axis, angle);
            return result;
        }

        /// <summary>
        /// Return a new Transform which is a rotated copy of this transform.
        /// </summary>
        /// <param name="point">The center of rotation.</param>
        /// <param name="axis">The axis direction.</param>
        /// <param name="angle">The angle of rotation in degrees.</param>
        public Transform RotatedAboutPoint(Vector3 point, Vector3 axis, double angle)
        {
            var result = new Transform(this);
            result.RotateAboutPoint(point, axis, angle);
            return result;
        }

        /// <summary>
        /// Apply a scale to the transform.
        /// </summary>
        /// <param name="amount">The amount to scale.</param>
        public void Scale(Vector3 amount)
        {
            var m = new Matrix();
            m.SetupScale(amount);
            this.Matrix = this.Matrix * m;
        }

        /// <summary>
        /// Return a copy of this transform scaled by the given value.
        /// </summary>
        /// <param name="amount">The amount to scale.</param>
        public Transform Scaled(Vector3 amount)
        {
            var m = new Matrix();
            m.SetupScale(amount);
            var copy = new Transform(this.Matrix * m);
            return copy;
        }

        /// <summary>
        /// Reflect about the plane with normal n.
        /// </summary>
        /// <param name="n">The normal of the reflection plane.</param>
        public void Reflect(Vector3 n)
        {
            var m = new Matrix();
            m.SetupReflect(n);
            this.Matrix = this.Matrix * m;
        }

        /// <summary>
        /// Calculate XY plane of the transform.
        /// </summary>
        public Plane XY()
        {
            return new Plane(this.Origin, this.ZAxis);
        }

        /// <summary>
        /// Calculate the YZ plane of the transform.
        /// </summary>
        public Plane YZ()
        {
            return new Plane(this.Origin, this.XAxis);
        }

        /// <summary>
        /// Calculate the XZ plane of the transform.
        /// </summary>
        public Plane XZ()
        {
            return new Plane(this.Origin, this.YAxis);
        }

        /// <summary>
        /// Scale uniformly about the origin.
        /// </summary>
        /// <param name="factor">The amount to scale uniformly</param>
        public void Scale(double factor)
        {
            Scale(new Vector3(factor, factor, factor));
        }

        /// <summary>
        /// Return a copy of this transform scaled uniformly.
        /// </summary>
        /// <param name="factor">The amount to scale uniformly</param>
        public Transform Scaled(double factor)
        {
            return Scaled(new Vector3(factor, factor, factor));
        }

        /// <summary>
        /// Scale uniformly about a point
        /// </summary>
        /// <param name="factor">The scale factor</param>
        /// <param name="origin">The origin of scaling</param>
        public void Scale(double factor, Vector3 origin)
        {
            Scale(factor);
            Move(origin * (1 - factor));
        }

        /// <summary>
        /// Create a transform that is oriented along 
        /// a curve at parameter t. The transform's +z axis will align with 
        /// the +z world axis, and the +x axis will align with the tangent
        /// of the curve. If you want a perpendicular transform, use `Curve.TransformAt(t)` instead.
        /// </summary>
        /// <param name="curve">The curve along which to orient the transform.</param>
        /// <param name="t">A parameter value between 0.0 and 1.0.</param>
        /// <param name="up"></param>

        [Obsolete]
        public static Transform CreateOrientedAlongCurve(Curve curve, double t, Vector3 up = default(Vector3))
        {
            var temp = curve.TransformAt(t);
            return new Transform(temp.Origin, temp.ZAxis.Negate(), temp.XAxis.Negate(), Vector3.ZAxis);
        }


        /// <summary>
        /// Create a transform that is oriented along 
        /// a curve at parameter t. The transform's +z axis will align with 
        /// the +z world axis, and the +x axis will align with the XY projection of the tangent
        /// of the curve. If you want a perpendicular transform, use `Curve.TransformAt(t)` instead.
        /// </summary>
        /// <param name="curve">The curve along which to orient the transform.</param>
        /// <param name="t">A parameter value between 0.0 and 1.0.</param>
        /// <param name="up"></param>
        public static Transform CreateHorizontalFrameAlongCurve(Curve curve, double t, Vector3 up = default(Vector3))
        {
            var temp = curve.TransformAt(t);
            var tangent = temp.XAxis.Negate();
            var normal = Vector3.ZAxis;
            if (tangent.Z > 1 - Vector3.EPSILON)
            {
                tangent = Vector3.XAxis;
            }
            tangent = (tangent.X, tangent.Y);
            return new Transform(temp.Origin, tangent, normal);
        }

        /// <summary>
        /// Is this transform equal to the provided transform?
        /// </summary>
        /// <param name="other">The transform to test.</param>
        /// <returns>True if the two transforms are equal, otherwise false.</returns>
        public bool Equals(Transform other)
        {
            if (other == null)
            {
                return false;
            }
            return this.Matrix.Equals(other.Matrix);
        }
    }
}