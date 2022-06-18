using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elements.Search;

namespace Elements.Geometry.Solids
{
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


        /// <summary>
        /// Does this face intersect with the provided plane?
        /// </summary>
        /// <param name="plane">The intersection plane.</param>
        /// <param name="lines">Line segments created by the intersection.</param>
        /// <returns>True if the plane intersects the face, otherwise false.</returns>
        public bool Intersects(Plane plane, out List<Line> lines)
        {
            var pts = new List<Vector3>();

            var outerVerts = this.Outer.Edges.Select(e => e.Vertex).ToList();
            for (var i = 0; i < outerVerts.Count; i++)
            {
                var a = outerVerts[i].Point;
                var b = i == outerVerts.Count - 1 ? outerVerts[0].Point : outerVerts[i + 1].Point;
                var l = new Line(a, b);
                if ((a, b).Intersects(plane, out var result))
                {
                    pts.Add(result);
                }
            }

            if (this.Inner != null)
            {
                foreach (var inner in Inner)
                {
                    var innerVerts = inner.Edges.Select(e => e.Vertex).ToList();
                    for (var i = 0; i < innerVerts.Count; i++)
                    {
                        var a = innerVerts[i].Point;
                        var b = i == innerVerts.Count - 1 ? innerVerts[0].Point : innerVerts[i + 1].Point;
                        var l = new Line(a, b);
                        if ((a, b).Intersects(plane, out var result))
                        {
                            pts.Add(result);
                        }
                    }
                }
            }

            if (pts.Count < 2)
            {
                lines = null;
                return false;
            }

            // var start = pts[0];
            // pts.Sort(new DistanceComparer(start));
            var d = this.Plane().Normal.Cross(plane.Normal).Unitized();
            pts.Sort(new DirectionComparer(d));
            lines = new List<Line>();
            for (var i = 0; i < pts.Count; i += 2)
            {
                lines.Add(new Line(pts[i], pts[i + 1]));
            }
            return true;
        }
    }
}