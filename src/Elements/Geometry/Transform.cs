using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;

namespace Elements.Geometry
{
    /// <summary>
    /// A Transform defined by an origin and x, y, and z axes.
    /// </summary>
    public class Transform
    {
        private Matrix _matrix;

        /// <summary>
        /// The origin.
        /// </summary>
        [JsonProperty("origin")]
        public Vector3 Origin
        {
            get { return this._matrix.Translation; }
        }

        /// <summary>
        /// The X axis.
        /// </summary>
        [JsonProperty("x_axis")]
        public Vector3 XAxis
        {
            get { return this._matrix.XAxis; }
        }

        /// <summary>
        /// The Y axis.
        /// </summary>
        [JsonProperty("y_axis")]
        public Vector3 YAxis
        {
            get { return this._matrix.YAxis; }
        }

        /// <summary>
        /// The Z axis.
        /// </summary>
        [JsonProperty("z_axis")]
        public Vector3 ZAxis
        {
            get { return this._matrix.ZAxis; }
        }

        /// <summary>
        /// The XY plane of the Transform.
        /// </summary>
        [JsonIgnore]
        public Plane XY
        {
            get { return new Plane(this.Origin, this.ZAxis); }
        }

        /// <summary>
        /// The YZ plane of the Transform.
        /// </summary>
        [JsonIgnore]
        public Plane YZ
        {
            get { return new Plane(this.Origin, this.XAxis); }
        }

        /// <summary>
        /// The XZ plane of the Transform.
        /// </summary>
        [JsonIgnore]
        public Plane XZ
        {
            get { return new Plane(this.Origin, this.YAxis); }
        }

        /// <summary>
        /// Construct the identity Transform.
        /// </summary>
        public Transform()
        {
            this._matrix = new Matrix();
        }

        /// <summary>
        /// Construct a Transform with a translation.
        /// </summary>
        /// <param name="origin">The origin of the Transform.</param>
        public Transform(Vector3 origin)
        {
            this._matrix = new Matrix();
            this._matrix.SetupTranslation(origin);
        }

        /// <summary>
        /// Construct a Transform with a translation.
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
        /// Construct a Transform by origin and axes.
        /// </summary>
        /// <param name="origin">The origin of the Transform.</param>
        /// <param name="xAxis">The X axis of the Transform.</param>
        /// <param name="zAxis">The Z axis of the Transform.</param>
        public Transform(Vector3 origin, Vector3 xAxis, Vector3 zAxis)
        {
            var x = xAxis.Normalized();
            var z = zAxis.Normalized();
            var y = z.Cross(x).Normalized();
            this._matrix = new Matrix(x, y, z, origin);
        }

        /// <summary>
        /// Compute the Transform with origin at o,
        /// whose Z axis points from a to b, and whose
        /// up direction is up.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="up"></param>
        internal Transform(Vector3 o, Vector3 a, Vector3 b, Vector3 up = null)
        {
            var z = (b - a).Normalized();

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
            this._matrix = new Matrix(x, y, z, o);
        }

        /// <summary>
        /// Get a string representation of the Transform.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._matrix.ToString();
        }

        /// <summary>
        /// Transform a Vector into the coordinate space defined by this Transform.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <returns>A new Vector transformed by this Transform.</returns>
        public Vector3 OfPoint(Vector3 vector)
        {
            var v = vector * this._matrix;
            return vector * this._matrix;
        }

        /// <summary>
        /// Transform the specified Polygon.
        /// </summary>
        /// <param name="polygon">The polygon to Transform.</param>
        /// <returns>A new Polygon transformed by this Transform.</returns>
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
        /// Transform the specified Line.
        /// </summary>
        /// <param name="line">The Line to transform.</param>
        /// <returns>A new Line transformed by this Transform.</returns>
        public Line OfLine(Line line)
        {
            return new Line(OfPoint(line.Start), OfPoint(line.End));
        }

        /// <summary>
        /// Transform the specified Profile.
        /// </summary>
        /// <param name="profile">The Profile to transform.</param>
        /// <returns>A new Profile transformed by this Transform.</returns>
        public Profile OfProfile(IProfile profile)
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
        /// Apply a translation to the Transform.
        /// </summary>
        /// <param name="translation">The translation to apply.</param>
        public void Move(Vector3 translation)
        {
            var m = new Matrix();
            m.SetupTranslation(translation);
            this._matrix = this._matrix * m;
        }

        /// <summary>
        /// Apply a rotation to the Transform.
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
        /// Apply a scale to the Transform.
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