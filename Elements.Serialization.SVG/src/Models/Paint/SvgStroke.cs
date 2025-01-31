using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// The stroke style and size information
    /// </summary>
    public class SvgStroke
    {
        /// <summary>
        /// The stroke width.
        /// </summary>
        public double Width { get; set; } = 0.01;

        /// <summary>
        /// The stroke color.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The definition of the dash pattern via an even number of entries.
        /// </summary>
        public IEnumerable<double> DashIntervals => _dashIntervals;

        /// <summary>
        /// The offset into the intervals array. (mod the sum of all of the intervals).
        /// </summary>
        public double DashPhase => _dashPhase;

        /// <summary>
        /// A value indicating whether paint has a dash pattern.
        /// </summary>
        public bool HasDash { get; private set; }

        /// <summary>
        /// Creates a dash path effect by specifying the dash intervals.
        /// </summary>
        /// <param name="intervals">The definition of the dash pattern via an even number of entries.</param>
        /// <param name="phase">The offset into the intervals array. (mod the sum of all of the intervals).</param>
        public void CreateDash(double[] intervals, float phase)
        {
            _dashIntervals.Clear();
            _dashIntervals.AddRange(intervals);
            _dashPhase = phase;
            HasDash = true;
        }

        /// <summary>
        /// Deletes dash pattern
        /// </summary>
        public void DeleteDash()
        {
            _dashIntervals.Clear();
            _dashPhase = 0;
            HasDash = false;
        }

        private readonly List<double> _dashIntervals = new List<double>();
        private double _dashPhase;
    }
}