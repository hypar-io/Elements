using System.Collections.Generic;

namespace Elements.Search
{
    /// <summary>
    /// Double comparer that treats all numbers within tolerance as the same.
    /// This comparer doesn't use hash code as it is *impossible* to create a hashing 
    /// algorithm that consistently returns identical values for any two points
    /// within tolerance of each other.
    /// Note that any item that is not within tolerance of other key is set as a new key.
    /// This mean that keys are not always whole number in the middle of a range.
    /// </summary>
    public class DoubleToleranceComparer : IEqualityComparer<double>
    {
        /// <summary>
        /// Create a comparer.
        /// </summary>
        /// <param name="tolerance">Number tolerance</param>
        public DoubleToleranceComparer(double tolerance)
        {
            _tolerance = tolerance;
        }

        /// <summary>
        /// Check if two numbers are the same within tolerance
        /// </summary>
        /// <param name="x">First number</param>
        /// <param name="y">Second number</param>
        /// <returns>True if x should be treated the same as y.</returns>
        public bool Equals(double x, double y)
        {
            return x.ApproximatelyEquals(y, _tolerance);
        }

        /// <summary>
        /// Hash code for number. Always returns 0.
        /// </summary>
        /// <param name="obj">number</param>
        /// <returns>0</returns>
        public int GetHashCode(double obj)
        {
            return 0;
        }

        private double _tolerance;
    }
}