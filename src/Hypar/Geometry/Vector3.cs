using System;

namespace Hypar.Geometry
{
    public class Vector3
    {
        public double X{get;}
        public double Y{get;}
        public double Z{get;}

        public Vector3()
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }
        
        public Vector3(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public Vector3(Vector3 v)
        {
            this.X = v.X;
            this.Y = v.Y;
            this.Z = v.Z;
        }

        public Vector3(Vector2 v)
        {
            this.X = v.X;
            this.Y = v.Y;
        }

        public double Length()
        {
            return Math.Sqrt(Math.Pow(X,2) + Math.Pow(Y,2) + Math.Pow(Z,2));
        }

        public Vector3 Normalized()
        {
            var length = Length();
            return new Vector3(X/length, Y/length, Z/length);
        }

        public Vector3 Cross(Vector3 v)
        {   
            var x = Y * v.Z - Z * v.Y;
            var y = Z * v.X - X * v.Z;
            var z = X * v.Y - Y * v.X;

            return new Vector3(x, y, z);
        }

        public double Dot(Vector3 v)
        {
            return v.X * X + v.Y * Y + v.Z * Z;
        }

        public double AngleTo(Vector3 v)
        {
            return Math.Acos((Dot(v)/(Length()*v.Length())));
        }

        public Vector3 ProjectOnto(Vector3 a)
        {   
            var b = this;
            return (a.Dot(b)/Math.Pow(a.Length(),2)) * a;
        }

        public static Vector3 operator * (Vector3 v, double a)
        {
            return new Vector3(v.X * a, v.Y * a, v.Z * a);
        }

        public static Vector3 operator * (double a, Vector3 v)
        {
            return new Vector3(v.X * a, v.Y * a, v.Z * a);
        }

        public static Vector3 operator - (Vector3 a, Vector3 b)
        {
            return new Vector3((a.X - b.X), (a.Y - b.Y), (a.Z - b.Z));
        }

        public static Vector3 operator + (Vector3 a, Vector3 b)
        {
            return new Vector3((a.X + b.X), (a.Y + b.Y), (a.Z + b.Z));
        }

        public bool IsParallelTo(Vector3 v)
        {
            var result = Math.Abs(Dot(v));
            return result == 1.0;
        }

        public double[] ToArray()
        {
            return new[]{X, Y, Z};
        }

        public override string ToString()
        {
            return $"X:{X},Y:{Y},Z:{Z}";
        }
        
        public override bool Equals(object obj)
        {
            if(obj.GetType() != GetType())
            {
                return false;
            }

            var v = (Vector3)obj;
            return X == v.X && Y == v.Y && Z==v.Z;
        }
    }
}