using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A continguous set of curves.
    /// </summary>
    public class Contour : IEnumerable<Curve>
    {
        private List<Curve> _curves = new List<Curve>();

        /// <summary>
        /// Construct a contour.
        /// </summary>
        /// <param name="curves">A list of curves to create the contour.</param>
        public Contour(List<Curve> curves)
        {
            for (var i = 0; i <= curves.Count - 1; i++)
            {
                var a = curves[i];
                var next = i == curves.Count - 1 ? 0 : i + 1;
                var b = curves[next];
                if (!a.PointAt(1).IsAlmostEqualTo(b.PointAt(0)))
                {
                    throw new ArgumentException($"The contour could not be constructed. The end of curve {i} and the start of curve {next} are not continguous.");
                }
            }
            this._curves = curves;
        }

        /// <summary>
        /// Get the enumerator for the collection of curves.
        /// </summary>
        /// <returns></returns>
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
            foreach(var c in this._curves)
            {
                verts.AddRange(c.RenderVertices());
            }

            return new Polygon(verts.Distinct().ToList());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}