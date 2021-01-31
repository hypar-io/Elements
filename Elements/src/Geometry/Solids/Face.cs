using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Hypar.Elements.Tests")]

namespace Elements.Geometry.Solids
{
    internal class IntersectionComparer : IComparer<Vector3>
    {
        private Vector3 _origin;
        public IntersectionComparer(Vector3 origin)
        {
            this._origin = origin;
        }

        public int Compare(Vector3 x, Vector3 y)
        {
            var a = x.DistanceTo(_origin);
            var b = y.DistanceTo(_origin);

            if (a < b)
            {
                return -1;
            }
            else if (a > b)
            {
                return 1;
            }
            return 0;
        }
    }

    /// <summary>
    /// A Solid Face.
    /// </summary>
    public class Face
    {
        /// <summary>
        /// The Id of the Face.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// A CCW wound list of Edges.
        /// </summary>
        public Loop Outer { get; internal set; }

        /// <summary>
        /// A collection of CW wound Edges.
        /// </summary>
        public Loop[] Inner { get; internal set; }

        /// <summary>
        /// Construct a Face.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="outer">The outer loop of the Face.</param>
        /// <param name="inner">The inner loops of the Face.</param>
        internal Face(long id, Loop outer, Loop[] inner)
        {
            this.Id = id;
            this.Outer = outer;
            outer.Face = this;
            this.Inner = inner;
            if (this.Inner != null)
            {
                foreach (var loop in inner)
                {
                    loop.Face = this;
                }
            }
        }

        /// <summary>
        /// The string representation of the Face.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var he in this.Outer.Edges)
            {
                sb.AppendLine($"HalfEdge: {he.ToString()}");
            }
            return sb.ToString();
        }

        internal bool TryIntersect(Plane p, out List<Line> lines)
        {
            lines = new List<Line>();

            var xsects = new List<Vector3>();
            foreach (var he in this.Outer.Edges)
            {
                var line = new Line(he.Edge.Left.Vertex.Point, he.Edge.Right.Vertex.Point);
                if (line.Intersects(p, out var result))
                {
                    xsects.Add(result);
                }
            }

            if (this.Inner != null)
            {
                foreach (var loop in this.Inner)
                {
                    foreach (var he in loop.Edges)
                    {
                        var line = new Line(he.Edge.Left.Vertex.Point, he.Edge.Right.Vertex.Point);
                        if (line.Intersects(p, out var result))
                        {
                            xsects.Add(result);
                        }
                    }
                }
            }

            if (xsects.Count == 0)
            {
                return false;
            }

            // Sort the intersections along a vector, then
            // use the standard inside/out alternation to
            // derive segments.
            xsects.Sort(new IntersectionComparer(xsects[0]));
            for (var i = 0; i < xsects.Count - 1; i += 2)
            {
                lines.Add(new Line(xsects[i], xsects[i + 1]));
            }

            return true;
        }
    }
}