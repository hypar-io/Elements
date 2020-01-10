using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A right-handed coordinate with +z up.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/Examples/TransformExample.cs?name=example)]
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
        public Transform(Transform t): 
            this(new Matrix(new Vector3(t.XAxis), new Vector3(t.YAxis), new Vector3(t.ZAxis), new Vector3(t.Origin))){}

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

            if(!z.IsParallelTo(Vector3.ZAxis))
            {   
                // Project up onto the ortho plane
                var p = new Plane(origin, z);
                var test = Vector3.ZAxis.Project(p);
                x = test.Cross(z);
                y = x.Cross(z.Negate()); 
            }
            
            this.Matrix = new Matrix(x, y, z, Vector3.Origin);
            ApplyRotationAndTranslation(rotation, z, origin);
        }

        private void ApplyRotationAndTranslation(double rotation, Vector3 axis, Vector3 translation)
        {
            if(rotation != 0.0)
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
            var x = xAxis.Normalized();
            var z = zAxis.Normalized();
            var y = z.Cross(x).Normalized();
            this.Matrix = new Matrix(x, y, z, Vector3.Origin);
            ApplyRotationAndTranslation(rotation, z, origin);
        }

        /// <summary>
        /// Create a transform by origin, X, Y, and Z axes.
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
            this.Matrix = new Matrix(xAxis, yAxis, zAxis, origin);
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
        /// Transform a vector into the coordinate space defined by this transform ignoring the translation.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <returns>A new vector transformed by this transform.</returns>
        public Vector3 OfPoint(Vector3 vector)
        {
            return vector * this.Matrix;
        }

        /// <summary>
        /// Transform a vector into the coordinate space defined by this transform.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <returns>A new vector transformed by this transform.</returns>
        public Vector3 OfVector(Vector3 vector)
        {
            var m = new Matrix(this.XAxis, this.YAxis, this.ZAxis, Vector3.Origin);
            return new Vector3(
                vector.X*m.XAxis.X + vector.Y*YAxis.X + vector.Z*ZAxis.X,
                vector.X*m.XAxis.Y + vector.Y*YAxis.Y + vector.Z*ZAxis.Y,
                vector.X*m.XAxis.Z + vector.Y*YAxis.Z + vector.Z*ZAxis.Z
            );
        }

        /// <summary>
        /// Transform the specified polygon.
        /// </summary>
        /// <param name="polygon">The polygon to transform.</param>
        /// <returns>A new polygon transformed by this transform.</returns>
        public Polygon OfPolygon(Polygon polygon)
        {
            var transformed = new Vector3[polygon.Vertices.Count];
            for (var i = 0; i < transformed.Length; i++)
            {
                transformed[i] = OfPoint(polygon.Vertices[i]);
            }
            var p = new Polygon(transformed);
            return p;
        }

        /// <summary>
        /// Transform the specified polygons.
        /// </summary>
        /// <param name="polygons">The polygons to transform.</param>
        /// <returns>An array of polygons transformed by this transform.</returns>
        public Polygon[] OfPolygons(IList<Polygon> polygons)
        {
            var result = new Polygon[polygons.Count];
            for(var i=0; i<polygons.Count; i++)
            {
                result[i] = OfPolygon(polygons[i]);
            }
            return result;
        }

        /// <summary>
        /// Transform the specified line.
        /// </summary>
        /// <param name="line">The line to transform.</param>
        /// <returns>A new line transformed by this transform.</returns>
        public Line OfLine(Line line)
        {
            return new Line(OfPoint(line.Start), OfPoint(line.End));
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
                    voids[i] = OfPolygon(profile.Voids[i]);
                }
            }
            var p = new Profile(OfPolygon(profile.Perimeter), voids, Guid.NewGuid(), null);
            return p;
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
        /// Invert this transform.
        /// </summary>
        public void Invert()
        {
            this.Matrix = this.Matrix.Inverse();
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
        /// Scale uniformly about a point
        /// </summary>
        /// <param name="factor">The scale factor</param>
        /// <param name="origin">The origin of scaling</param>
        public void Scale(double factor, Vector3 origin)
        {
            Scale(factor);
            Move(origin * (1-factor));
        }

        /// <summary>
        /// Create a transform that is oriented along 
        /// a curve at parameter t. The transform's +z axis will align with 
        /// the +z world axis, and the +x axis will align with the tangent
        /// of the curve.
        /// </summary>
        /// <param name="curve">The curve along which to orient the transform.</param>
        /// <param name="t">A parameter value between 0.0 and 1.0.</param>
        /// <param name="up"></param>
        public static Transform CreateOrientedAlongCurve(Curve curve, double t, Vector3 up = default(Vector3))
        {
            var temp = curve.TransformAt(t);
            return new Transform(temp.Origin, temp.ZAxis.Negate(), temp.XAxis.Negate(), Vector3.ZAxis);
        }

        /// <summary>
        /// Is this transform equal to the provided transform?
        /// </summary>
        /// <param name="other">The transform to test.</param>
        /// <returns>True if the two transforms are equal, otherwise false.</returns>
        public bool Equals(Transform other)
        {
            if(other == null)
            {
                return false;
            }
            return this.Matrix.Equals(other.Matrix);
        }
    }
}