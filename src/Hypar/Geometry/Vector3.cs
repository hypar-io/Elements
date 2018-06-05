using System;
using System.Collections.Generic;

namespace Hypar.Geometry
{
    public class Vector3
    {
        public double X{get;}
        public double Y{get;}
        public double Z{get;}

        public static Vector3 Origin()
        {
            return new Vector3();
        }
        public static Vector3 XAxis()
        {
            return new Vector3(1.0,0.0,0.0);
        }

        public static Vector3 YAxis()
        {
            return new Vector3(0.0,1.0,0.0);
        }
        public static Vector3 ZAxis()
        {
            return new Vector3(0.0,0.0,1.0);
        }

        public static Vector3 ByXYZ(double x, double y, double z)
        {
            return new Vector3(x,y,z);
        }

        public static Vector3 ByXY(double x, double y)
        {
            return new Vector3(x,y);
        }

        public static IEnumerable<Vector3> AtNEqualSpacesAlongLine(Line l, int n, bool includeEnds = false)
        {   
            var pts = new List<Vector3>();
            var div = 1.0/(double)(n + 1);
            for(var t=0.0; t<=1.0; t+=div)
            {
                var pt = l.PointAt(t);
                
                if((t == 0.0 && !includeEnds) || (t == 1.0 && !includeEnds))
                {
                    continue;
                }
                pts.Add(pt);
            }

            return pts;
        }

        public static IEnumerable<IEnumerable<Vector3>> AtNEqualSpacesAlongLines(IEnumerable<Line> lines, int n, bool includeEnds = false)
        {
            var vs = new List<IEnumerable<Vector3>>();
            foreach(var l in lines)
            {
                var vs1 = Vector3.AtNEqualSpacesAlongLine(l, n, includeEnds);
                vs.Add(vs1);
            }
            return vs;
        }

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

        public static bool operator > (Vector3 a, Vector3 b)
        {
            return a.X > b.X && a.Y > b.Y && a.Z > b.Z;
        }

        public static bool operator < (Vector3 a, Vector3 b)
        {
            return a.X < b.X && a.Y < b.Y && a.Z < b.Z;
        }

        public bool IsParallelTo(Vector3 v)
        {
            var result = Math.Abs(Dot(v));
            return result == 1.0;
        }

        public Vector3 Negate()
        {
            return new Vector3(-X, -Y, -Z);
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