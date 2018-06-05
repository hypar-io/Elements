using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    public class Line
    {
        private Vector3 _start = new Vector3();
        private Vector3 _end = new Vector3(1,0);

        public Vector3 Start => _start;

        public Vector3 End => _end;

        /// <summary>
        /// Construct a line starting at a point.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static Line FromStart(Vector3 start)
        {
            var l = new Line();
            l._start = start;
            return l;
        }

        /// <summary>
        /// Construct a collection of lines from many points.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static IEnumerable<Line> FromStart(IEnumerable<Vector3> start)
        {
            var lines = new List<Line>();
            foreach(var v in start)
            {
                var l = Line.FromStart(v);
                lines.Add(l);
            }
            return lines;
        }

        /// <summary>
        /// Set the end point of a line.
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public Line ToEnd(Vector3 end)
        {
            this._end = end;
            return this;
        }

        private Line()
        {

        }

        internal Line(Vector3 start, Vector3 end)
        {
            if (start.Equals(end))
            {
                throw new Exception("The start and end of the Line cannot be the same.");
            }
            this._start = start;
            this._end = end;
        }
        
        /// <summary>
        /// Get the length of the line.
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(Math.Pow(this.Start.X - this.End.X, 2) + Math.Pow(this.Start.Y - this.End.Y, 2) + Math.Pow(this.Start.Z - this.End.Z, 2));
        }

        /// <summary>
        /// Get a transform from the line, where the origin of the transform
        /// is the start of the line, and the Z axis of the transform
        /// is the line's direction.
        /// </summary>
        /// <param name="up"></param>
        /// <returns></returns>
        public Transform GetTransform(Vector3 up = null)
        {
            var v = Direction();
            var x = new Vector3(1, 0, 0);
            var y = new Vector3(0, 1, 0);
            var z = new Vector3(0, 0, 1);

            if (up == null)
            {
                up = z;
            }
            if (up.IsParallelTo(v))
            {
                up = x;
            }

            var xAxis = v.Cross(up).Normalized();
            var t = new Transform(this.Start, xAxis, v);

            return t;
        }

        /// <summary>
        /// Get a normalized vector representing the direction of the line.
        /// </summary>
        /// <returns></returns>
        public Vector3 Direction()
        {
            return (this.End - this.Start).Normalized();
        }

        /// <summary>
        /// Get a point along the line at parameter t.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 PointAt(double t)
        {
            if (t > 1.0 || t < 0.0)
            {
                throw new Exception("The parameter t must be between 0.0 and 1.0.");
            }
            var offset = this.Length() * t;
            return this.Start + offset * this.Direction();
        }

        /// <summary>
        /// Get a new line that is the reverse of the original line.
        /// </summary>
        /// <returns></returns>
        public Line Reversed()
        {
            return new Line(End, Start);
        }
    }

    public static class LineCollectionExtensions
    {
        /// <summary>
        /// Set the end point of a collection of lines.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static IEnumerable<Line> ToEnd(this IEnumerable<Line> lines, Vector3 end)
        {
            foreach(var l in lines)
            {
                l.ToEnd(end);
            }
            return lines;
        }
        
        /// <summary>
        /// Set the end point of a collection of lines to a collection of vectors.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static IEnumerable<Line> ToEnd(this IEnumerable<Line> lines, IEnumerable<Vector3> end)
        {
            var linesArr = lines.ToArray();
            var endArr = end.ToArray();

            for(var i=0; i<linesArr.Length; i++)
            {
                linesArr[i].ToEnd(endArr[i]);
            }
            return lines;
        }
    }
}