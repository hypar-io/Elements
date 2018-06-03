using System;

namespace Hypar.Geometry
{
    public class Line
    {
        public Vector3 Start{get;}
        public Vector3 End{get;}

        public Line(Vector3 start, Vector3 end)
        {
            if(start.Equals(end))
            {
                throw new Exception("The start and end of the Line cannot be the same.");
            }
            this.Start = start;
            this.End = end;
        }

        public double Length()
        {
            return Math.Sqrt(Math.Pow(this.Start.X - this.End.X, 2) + Math.Pow(this.Start.Y - this.End.Y,2) + Math.Pow(this.Start.Z - this.End.Z, 2));
        }

        public Transform GetTransform(Vector3 up = null)
        {   
            var v = Direction();
            var x = new Vector3(1,0,0);
            var y = new Vector3(0,1,0);
            var z = new Vector3(0,0,1);

            if(up == null)
            {
                up = z;
            }
            if(up.IsParallelTo(v))
            {
                up = x;
            }
            
            var xAxis = v.Cross(up).Normalized();
            var t = new Transform(this.Start, xAxis, v);

            return t;
        }

        public Vector3 Direction()
        {
            return (this.End - this.Start).Normalized();
        }

        public Vector3 PointAt(double t)
        {
            if(t > 1.0 || t < 0.0)
            {
                throw new Exception("The parameter t must be between 0.0 and 1.0.");
            }
            var offset = this.Length() * t;
            return this.Start + offset * this.Direction();
        }

        public Line Reversed()
        {
            return new Line(End, Start);
        }
    }
}