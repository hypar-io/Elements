using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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
            get{return this._matrix.Translation;}
        }

        /// <summary>
        /// The X axis.
        /// </summary>
        [JsonProperty("x_axis")]
        public Vector3 XAxis
        {
            get{return this._matrix.XAxis;}
        }

        /// <summary>
        /// The Y axis.
        /// </summary>
        [JsonProperty("y_axis")]
        public Vector3 YAxis
        {
            get{return this._matrix.YAxis;}
        }

        /// <summary>
        /// The Z axis.
        /// </summary>
        [JsonProperty("z_axis")]
        public Vector3 ZAxis
        {
            get{return this._matrix.ZAxis;}
        }

        /// <summary>
        /// The XY plane of the Transform.
        /// </summary>
        [JsonIgnore]
        public Plane XY
        {
            get{return new Plane(this.Origin, this.ZAxis);}
        }

        /// <summary>
        /// The YZ plane of the Transform.
        /// </summary>
        [JsonIgnore]
        public Plane YZ
        {
            get{return new Plane(this.Origin, this.XAxis);}
        }

        /// <summary>
        /// The XZ plane of the Transform.
        /// </summary>
        [JsonIgnore]
        public Plane XZ
        {
            get{return new Plane(this.Origin, this.YAxis);}
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
            this._matrix.SetupTranslation(new Vector3(x,y,z));
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
            var y = z.Cross(x);
            this._matrix = new Matrix(x, y, z, origin);
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
            return vector * this._matrix;
        }

        /// <summary>
        /// Transform the specified Polygon.
        /// </summary>
        /// <param name="polygon">The polygon to Transform.</param>
        /// <returns>A new Polygon transformed by this Transform.</returns>
        public Polygon OfPolygon(Polygon polygon)
        {
            return new Polygon(polygon.Vertices.Select(v=>OfPoint(v)).ToList());
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
        public Profile OfProfile(Profile profile)
        {
            var voids = profile.Voids == null ? null : profile.Voids.Select(v=>OfPolygon(v)).ToList();
            return new Profile(OfPolygon(profile.Perimeter), voids);
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
            m.SetupRotate(axis, angle * (Math.PI/180.0));
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