using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// Line represents a linear curve between two points.
    /// </summary>
    public class Line : ICurve
    {
        private Vector3 _start = new Vector3();
        private Vector3 _end = new Vector3(1,0);

        /// <summary>
        /// The start of the line.
        /// </summary>
        /// <returns></returns>
        public Vector3 Start
        {
            get{return _start;}
        }

        /// <summary>
        /// The end of the line.
        /// </summary>
        /// <returns></returns>
        public Vector3 End
        {
            get{return _end;}
        }

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
            if (this._start.IsAlmostEqualTo(end))
            {
                throw new Exception("The start and end of the Line cannot be the same.");
            }

            this._end = end;
            return this;
        }

        private Line()
        {

        }

        /// <summary>
        /// Construct a line from start and end points.
        /// </summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        public Line(Vector3 start, Vector3 end)
        {
            if (start.IsAlmostEqualTo(end))
            {
                throw new Exception("The start and end of the Line cannot be the same.");
            }
            this._start = start;
            this._end = end;
        }

        /// <summary>
        /// Construct a line from a start point, pointing in a direction, with the specified length.
        /// </summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="direction">The direction of the line.</param>
        /// <param name="length">The length of the line.</param>
        public Line(Vector3 start, Vector3 direction, double length)
        {
            this._start = start;
            this._end = start + direction.Normalized() * length;
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
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="up">The vector which will become the Y vector of the transform.</param>
        /// <returns>A transform.</returns>
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
        /// <returns>A vector representing the direction of the line.</returns>
        public Vector3 Direction()
        {
            return (this.End - this.Start).Normalized();
        }

        /// <summary>
        /// Get a point along the line at parameter u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>A point on the curve at parameter u.</returns>
        public Vector3 PointAt(double u)
        {
            if (u > 1.0 || u < 0.0)
            {
                throw new Exception("The parameter t must be between 0.0 and 1.0.");
            }
            var offset = this.Length() * u;
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

        /// <summary>
        /// Tessellate the curve.
        /// </summary>
        /// <returns>A collection of points sampled along the curve.</returns>
        public IEnumerable<Vector3> Tessellate()
        {
            return new[]{Start, End};
        }
    }

    /// <summary>
    /// Extension methods for lines.
    /// </summary>
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