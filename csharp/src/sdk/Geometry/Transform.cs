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
        /// <returns></returns>
        public Vector3 Origin{get;}

        /// <summary>
        /// The X axis.
        /// </summary>
        /// <returns></returns>
        public Vector3 XAxis{get;}

        /// <summary>
        /// The Y axis.
        /// </summary>
        /// <returns></returns>
        public Vector3 YAxis{get;}

        /// <summary>
        /// The Z axis.
        /// </summary>
        /// <returns></returns>
        public Vector3 ZAxis{get;}

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
    }
}