using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A coordinate system defined by an origin, x, y, and z axes.
    /// </summary>
    public class Transform
    {
        private Matrix _matrix;

        /// <summary>
        /// The transform's matrix.
        /// </summary>
        [JsonProperty("matrix")]
        public Matrix Matrix
        {
            get => _matrix;
        }

        /// <summary>
        /// The origin.
        /// </summary>
        [JsonIgnore]
        public Vector3 Origin
        {
            get { return this._matrix.Translation; }
        }

        /// <summary>
        /// The X axis.
        /// </summary>
        [JsonIgnore]
        public Vector3 XAxis
        {
            get { return this._matrix.XAxis; }
        }

        /// <summary>
        /// The Y axis.
        /// </summary>
        [JsonIgnore]
        public Vector3 YAxis
        {
            get { return this._matrix.YAxis; }
        }

        /// <summary>
        /// The Z axis.
        /// </summary>
        [JsonIgnore]
        public Vector3 ZAxis
        {
            get { return this._matrix.ZAxis; }
        }

        /// <summary>
        /// The XY plane of the transform.
        /// </summary>
        [JsonIgnore]
        public Plane XY
        {
            get { return new Plane(this.Origin, this.ZAxis); }
        }

        /// <summary>
        /// The YZ plane of the transform.
        /// </summary>
        [JsonIgnore]
        public Plane YZ
        {
            get { return new Plane(this.Origin, this.XAxis); }
        }

        /// <summary>
        /// The XZ plane of the transform.
        /// </summary>
        [JsonIgnore]
        public Plane XZ
        {
            get { return new Plane(this.Origin, this.YAxis); }
        }

        /// <summary>
        /// Construct the identity transform.
        /// </summary>
        public Transform()
        {
            this._matrix = new Matrix();
        }
        
        /// <summary>
        /// Construct a Transform by copying another transform.
        /// </summary>
        /// <param name="t">The transform to copy.</param>
        public Transform(Transform t)
        {
            this._matrix = new Matrix(new Vector3(t.XAxis), new Vector3(t.YAxis), new Vector3(t.ZAxis), new Vector3(t.Origin));
        }

        /// <summary>
        /// Construct a transform with a translation.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        public Transform(Vector3 origin)
        {
            this._matrix = new Matrix();
            this._matrix.SetupTranslation(origin);
        }

        /// <summary>
        /// Construct a transform with a translation.
        /// </summary>
        /// <param name="x">The X component of translation.</param>
        /// <param name="y">The Y component of translation.</param>
        /// <param name="z">The Z component of translation.</param>
        public Transform(double x, double y, double z)
        {
            this._matrix = new Matrix();
            this._matrix.SetupTranslation(new Vector3(x, y, z));
        }

        /// <summary>
        /// Construct a transform by origin and axes.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        /// <param name="xAxis">The X axis of the transform.</param>
        /// <param name="zAxis">The Z axis of the transform.</param>
        public Transform(Vector3 origin, Vector3 xAxis, Vector3 zAxis)
        {
            var x = xAxis.Normalized();
            var z = zAxis.Normalized();
            var y = z.Cross(x).Normalized();
            this._matrix = new Matrix(x, y, z, origin);
        }

        /// <summary>
        /// Construct a transform by a matrix.
        /// </summary>
        /// <param name="matrix">The Transform's Matrix.</param>
        [JsonConstructor]
        public Transform(Matrix matrix)
        {
            this._matrix = matrix;
        }

        /// <summary>
        /// Construct a transform with origin at origin,
        /// whose Z axis points from start to end, and whose
        /// up direction is up.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        /// <param name="start">The start of the z vector.</param>
        /// <param name="end">The end of the z vector.</param>
        /// <param name="up">A vector which can be used to orient the transform.</param>
        internal Transform(Vector3 origin, Vector3 start, Vector3 end, Vector3 up = null)
        {
            var z = (end - start).Normalized();

            if (up == null)
            {
                up = Vector3.ZAxis;
                if (up.IsParallelTo(z))
                {
                    if (z.IsParallelTo(Vector3.XAxis))
                    {
                        up = Vector3.YAxis;
                    }
                    else
                    {
                        up = Vector3.XAxis;
                    }
                }
            }

            var x = z.Cross(up).Normalized();
            var y = x.Cross(z);
            this._matrix = new Matrix(x, y, z, origin);
        }

        /// <summary>
        /// Get a string representation of the transform.
        /// </summary>
        /// <returns>A string representation of the transform.</returns>
        public override string ToString()
        {
            return this._matrix.ToString();
        }

        /// <summary>
        /// Transform a vector into the coordinate space defined by this transform.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <returns>A new vector transformed by this transform.</returns>
        public Vector3 OfPoint(Vector3 vector)
        {
            var v = vector * this._matrix;
            return vector * this._matrix;
        }

        /// <summary>
        /// Transform the specified polygon.
        /// </summary>
        /// <param name="polygon">The polygon to transform.</param>
        /// <returns>A new polygon transformed by this transform.</returns>
        public Polygon OfPolygon(Polygon polygon)
        {
            var transformed = new Vector3[polygon.Vertices.Length];
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
        public Polygon[] OfPolygons(Polygon[] polygons)
        {
            return polygons.Select(p=>OfPolygon(p)).ToArray();
        }

        /// <summary>
        /// Transform the specified Line.
        /// </summary>
        /// <param name="line">The line to transform.</param>
        /// <returns>A new line transformed by this transforms.</returns>
        public Line OfLine(Line line)
        {
            return new Line(OfPoint(line.Start), OfPoint(line.End));
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
                voids = new Polygon[profile.Voids.Length];
                for (var i = 0; i < voids.Length; i++)
                {
                    voids[i] = OfPolygon(profile.Voids[i]);
                }
            }
            var p = new Profile(OfPolygon(profile.Perimeter), voids);
            return p;
        }

        /// <summary>
        /// Concatenate the transform.
        /// </summary>
        /// <param name="transform"></param>
        public void Concatenate(Transform transform)
        {
            this._matrix = this._matrix * transform._matrix;
        }

        /// <summary>
        /// Apply a translation to the transform.
        /// </summary>
        /// <param name="translation">The translation to apply.</param>
        public void Move(Vector3 translation)
        {
            var m = new Matrix();
            m.SetupTranslation(translation);
            this._matrix = this._matrix * m;
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
            this._matrix = this._matrix * m;
        }

        /// <summary>
        /// Apply a scale to the transform.
        /// </summary>
        /// <param name="amount">The amount to scale.</param>
        public void Scale(Vector3 amount)
        {
            var m = new Matrix();
            m.SetupScale(amount);
            this._matrix = this._matrix * m;
        }
    }
}