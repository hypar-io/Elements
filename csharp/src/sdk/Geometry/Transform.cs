using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
        /// Transform a vector into the coordinate space defined by this transform.
        /// </summary>
        /// <param name="vector">The vector to be transformed.</param>
        public Vector3 OfPoint(Vector3 vector)
        {
            return vector * this._matrix;
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