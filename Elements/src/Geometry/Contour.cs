using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A continguous set of curves.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ContourTests.cs?name=example)]
    /// </example>
    public class Contour : IEnumerable<Curve>
    {
        private List<BoundedCurve> _curves = new List<BoundedCurve>();

        /// <summary>
        /// Construct a contour.
        /// </summary>
        /// <param name="curves">A list of curves to create the contour.</param>
        /// <exception>Throws an ArgumentException when the provided curves are not contiguous.</exception>
        public Contour(List<BoundedCurve> curves)
        {
            for (var i = 0; i <= curves.Count - 1; i++)
            {
                var a = curves[i];
                var next = i == curves.Count - 1 ? 0 : i + 1;
                var b = curves[next];
                if (a.PointAt(1).IsAlmostEqualTo(b.PointAt(0)) ||
                    a.PointAt(1).IsAlmostEqualTo(b.PointAt(1)) ||
                    a.PointAt(0).IsAlmostEqualTo(b.PointAt(0)))
                {
                    continue;
                }
                else
                {
                    throw new ArgumentException($"The contour could not be constructed. Curves {i} and {next} are not continguous.");
                }
            }
            this._curves = curves;
        }

        /// <summary>
        /// Get the enumerator for the collection of curves.
        /// </summary>
        public IEnumerator<Curve> GetEnumerator()
        {
            return _curves.GetEnumerator();
        }

        /// <summary>
        /// Convert the contour to a polygon.
        /// </summary>
        public Polygon ToPolygon()
        {
            // Convert the contour into a polygon.
            // Merge the coincident points.
            var verts = new List<Vector3>();
            foreach (var c in this._curves)
            {
                var v = c.RenderVertices();
                if (verts.Any())
                {
                    if (verts.Last().IsAlmostEqualTo(v.First()))
                    {
                        verts.AddRange(v.Skip(1));
                    }
                    else if (verts.Last().IsAlmostEqualTo(v.Last()))
                    {
                        var revVerts = v.Reverse();
                        verts.AddRange(revVerts.Skip(1));
                    }
                    else
                    {
                        verts.AddRange(v);
                    }
                }
                else
                {
                    // We skip the first vert assuming that the polygon
                    // is closed and that the final set of vertices
                    // will contain the "last" vert.
                    verts.AddRange(v.Skip(1));
                }
            }

            return new Polygon(verts);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}