using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// A transform defined by an origin and x, y, and z axes.
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
        /// The XY plane of the transform.
        /// </summary>
        public Plane XY
        {
            get{return new Plane(this.Origin, this.ZAxis);}
        }

        /// <summary>
        /// The YZ plane of the transform.
        /// </summary>
        /// <value></value>
        public Plane YZ
        {
            get{return new Plane(this.Origin, this.XAxis);}
        }

        /// <summary>
        /// The XZ plane of the transform.
        /// </summary>
        /// <value></value>
        public Plane XZ
        {
            get{return new Plane(this.Origin, this.YAxis);}
        }

        /// <summary>
        /// Construct the identity transform.
        /// </summary>
        public Transform()
        {
            this._matrix = new Matrix();
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
            var y = z.Cross(x);
            this._matrix = new Matrix(x, y, z, origin);
        }

        /// <summary>
        /// Get a string representation of the transform.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._matrix.ToString();
        }

        /// <summary>
        /// Transform a Vector into the coordinate space defined by this Transform.
        /// </summary>
        /// <param name="vector">The vector to be transformed.</param>
        /// <returns>A new Vector transformed by this transform.</returns>
        public Vector3 OfPoint(Vector3 vector)
        {
            return vector * this._matrix;
        }

        /// <summary>
        /// Transform a Polygon using this Transform.
        /// </summary>
        /// <param name="polygon">The polygon to transform.</param>
        /// <returns>A new Polygon transformed by this Transform.</returns>
        public Polygon OfPolygon(Polygon polygon)
        {
            return new Polygon(polygon.Vertices.Select(v=>OfPoint(v)).ToList());
        }

        /// <summary>
        /// Transform a Line using this Transform.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>A new Line transformed by this Transform.</returns>
        public Line OfLine(Line line)
        {
            return new Line(OfPoint(line.Start), OfPoint(line.End));
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
            m.SetupRotate(axis, angle * (Math.PI/180.0));
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