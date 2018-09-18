using Newtonsoft.Json;

namespace Hypar.Geometry
{
    /// <summary>
    /// A transform defined by an origin and x, y, and z axes.
    /// </summary>
    public class Transform
    {
        /// <summary>
        /// The origin.
        /// </summary>
        [JsonProperty("origin")]
        public Vector3 Origin{get;}

        /// <summary>
        /// The X axis.
        /// </summary>
        [JsonProperty("x_axis")]
        public Vector3 XAxis{get;}

        /// <summary>
        /// The Y axis.
        /// </summary>
        [JsonProperty("y_axis")]
        public Vector3 YAxis{get;}

        /// <summary>
        /// The Z axis.
        /// </summary>
        [JsonProperty("z_axis")]
        public Vector3 ZAxis{get;}

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
            this.Origin = new Vector3();
            this.XAxis = Vector3.XAxis;
            this.YAxis = Vector3.YAxis;
            this.ZAxis = Vector3.ZAxis;
        }

        /// <summary>
        /// Construct a transform by origin and axes.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        /// <param name="xAxis">The X axis of the transform.</param>
        /// <param name="zAxis">The Z axis of the transform.</param>
        public Transform(Vector3 origin, Vector3 xAxis, Vector3 zAxis)
        {
            this.Origin = origin;
            this.XAxis = xAxis.Normalized();
            this.ZAxis = zAxis.Normalized();
            this.YAxis = this.ZAxis.Cross(this.XAxis);
        }

        /// <summary>
        /// Get a string representation of the transform.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"X Axis:{this.XAxis}, Y Axis:{this.YAxis}, Z Axis: {this.ZAxis}";
        }

        /// <summary>
        /// Get the 4x4 matrix defined by this transform.
        /// </summary>
        /// <returns></returns>
        public double[] Matrix()
        {
            return new double[]{this.XAxis.X, this.YAxis.X, this.ZAxis.X, this.Origin.X, 
                                this.XAxis.Y, this.YAxis.Y, this.ZAxis.Y, this.Origin.Y,
                                this.XAxis.Z, this.YAxis.Z, this.ZAxis.Z, this.Origin.Z,
                                0.0, 0.0, 0.0, 0.0};
        }

        /// <summary>
        /// Transform a vector into the coordinate space defined by this transform.
        /// </summary>
        /// <param name="vector">The vector to be transformed.</param>
        public Vector3 OfPoint(Vector3 vector)
        {
            var m = Matrix();
            var v1 = new Vector3((m[0] * vector.X + m[1] * vector.Y + m[2] * vector.Z) + this.Origin.X, 
                                (m[4] * vector.X + m[5] * vector.Y + m[6] * vector.Z) + this.Origin.Y,
                                (m[8] * vector.X + m[9] * vector.Y + m[10] * vector.Z) + this.Origin.Z);
            return v1;
        }
    }
}