using System;

namespace Hypar.Geometry
{
    public class Quaternion
    {
        public double X{get;}
        public double Y{get;}
        public double Z{get;}
        public double W{get;}
        public Quaternion(Vector3 axis, double angle)
        {
            this.X = axis.X * Math.Sin(angle/2);
            this.Y = axis.Y * Math.Sin(angle/2);
            this.Z = axis.Z * Math.Sin(angle/2);
            this.W = Math.Cos(angle/2);
        }
    }
}