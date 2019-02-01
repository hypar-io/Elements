using System;

namespace Elements.Geometry
{
    /// <summary>
    /// A quaternion.
    /// </summary>
    public class Quaternion
    {
        /// <summary>
        /// The X component.
        /// </summary>
        /// <returns></returns>
        public double X{get;}

        /// <summary>
        /// The Y component.
        /// </summary>
        /// <returns></returns>
        public double Y{get;}

        /// <summary>
        /// The Z component.
        /// </summary>
        /// <returns></returns>
        public double Z{get;}

        /// <summary>
        /// The W component.
        /// </summary>
        /// <returns></returns>
        public double W{get;}

        /// <summary>
        /// Construct a Quaternion from an axis and an angle in radians.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        public Quaternion(Vector3 axis, double angle)
        {
            this.X = axis.X * Math.Sin(angle/2);
            this.Y = axis.Y * Math.Sin(angle/2);
            this.Z = axis.Z * Math.Sin(angle/2);
            this.W = Math.Cos(angle/2);
        }
    }
}