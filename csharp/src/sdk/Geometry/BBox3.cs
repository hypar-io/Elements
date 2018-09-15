using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// An axis-alignment bounding box.
    /// </summary>
    public class BBox3
    {
        /// <summary>
        /// The maximum extent of the bounding box.
        /// </summary>
        public Vector3 Max{get;}

        /// <summary>
        /// The minimum extent of the bounding box.
        /// </summary>
        public Vector3 Min{get;}

        /// <summary>
        /// Construct a bounding box from a collection of points.
        /// </summary>
        /// <param name="points">The points which are contained within the bounding box.</param>
        /// <returns>A bounding box.</returns>
        public BBox3(IEnumerable<Vector3> points)
        {
            this.Min = Vector3.Origin;
            this.Max = Vector3.Origin;
            foreach(var p in points)
            {
                if(p < this.Min) this.Min = p;
                if(p > this.Max) this.Max = p;
            }
        }

        /// <summary>
        /// Construct the bounding box for a curve.
        /// </summary>
        /// <param name="curve"></param>
        public BBox3(ICurve curve)
        {
            var verts = curve.Tessellate();
            this.Min = Vector3.Origin;
            this.Max = Vector3.Origin;
            foreach(var p in verts)
            {
                if(p < this.Min) this.Min = p;
                if(p > this.Max) this.Max = p;
            }
        }

        /// <summary>
        /// Construct a bounding box for a collection of polygons.
        /// </summary>
        /// <param name="polygons"></param>
        public BBox3(IEnumerable<Polygon> polygons)
        {
            var verts = polygons.SelectMany(p=>p.Tessellate());
            this.Min = Vector3.Origin;
            this.Max = Vector3.Origin;
            foreach(var p in verts)
            {
                if(p < this.Min) this.Min = p;
                if(p > this.Max) this.Max = p;
            }
        }

        /// <summary>
        /// Construct a bounding box specifying minimum and maximum extents.
        /// </summary>
        /// <param name="min">The minimum extent of the bounding box.</param>
        /// <param name="max">The maximum extent of the bounding box.</param>
        public BBox3(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
        }
    }
}