using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// Vector3 
    /// </summary>
    public class Vector3ToleranceComparer : IEqualityComparer<Vector3>
    {
        /// <summary>
        /// Create a comparer.
        /// </summary>
        /// <param name="tolerance">Vector3 tolerance</param>
        public Vector3ToleranceComparer(double tolerance)
        {
            _tolerance = tolerance;
        }

        /// <summary>
        /// Check if two vectors are the same within tolerance.
        /// </summary>
        /// <param name="x">First vector.</param>
        /// <param name="y">Second vector</param>
        /// <returns>True if x should be treated the same as y.</returns>
        public bool Equals(Vector3 x, Vector3 y)
        {
            return x.IsAlmostEqualTo(y, _tolerance);
        }

        /// <summary>
        /// Hash code for the vector. 
        /// </summary>
        /// <param name="obj">Vector</param>
        /// <returns>0</returns>
        public int GetHashCode(Vector3 obj)
        {
            // We want to return the same hash code
            // for all vertices to force the comparer
            // to do the comparison.
            return 0;
        }

        private readonly double _tolerance;
    }
}