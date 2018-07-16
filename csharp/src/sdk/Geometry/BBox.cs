using System.Collections.Generic;

namespace Hypar.Geometry
{
    /// <summary>
    /// BBox represents an axis-alignment bounding box.
    /// </summary>
    public class BBox
    {
        private Vector3 _min;
        private Vector3 _max;

        /// <summary>
        /// The maximum extent of the bounding box.
        /// </summary>
        public Vector3 Max => _max;

        /// <summary>
        /// The minimum extent of the bounding box.
        /// </summary>
        public Vector3 Min => _min;

        /// <summary>
        /// Construct a bounding box from a collection of points.
        /// </summary>
        /// <param name="points">The points which are contained within the bounding box.</param>
        /// <returns>A bounding box.</returns>
        public static BBox FromPoints(IEnumerable<Vector3> points)
        {
            var min = Vector3.Origin();
            var max = Vector3.Origin();
            foreach(var p in points)
            {
                if(p < min) min = p;
                if(p > max) max = p;
            }
            return new BBox(min, max);
        }

        /// <summary>
        /// Construct a bounding box specifying minimum and maximum extents.
        /// </summary>
        /// <param name="min">The minimum extent of the bounding box.</param>
        /// <param name="max">The maximum extent of the bounding box.</param>
        public BBox(Vector3 min, Vector3 max)
        {
            this._min = min;
            this._max = max;
        }
    }
}