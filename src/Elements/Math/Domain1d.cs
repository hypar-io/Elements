using System;
namespace Elements
{
    /// <summary>
    /// A 1 dimensional interval or domain.
    /// </summary>
    public struct Domain1d
    {
        internal bool IsIncreasing()
        {
            return Max > Min;
        }

        /// <summary>
        /// Returns true if pos is within the domain (exclusive of its ends)
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is within the domain</returns>
        internal bool Includes(double value)
        {
            if (IsIncreasing())
            {
                return value < Max && value > Min;
            }
            else
            {
                return value < Min && value < Max;
            }
        }

        /// <summary>
        /// The lower bound of the domain
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// The upper bound of the domain
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// The length of the domain — Max-Min. Note that for non-increasing
        /// domains this value can be negative.
        /// </summary>
        public double Length => Max - Min;

        /// <summary>
        /// Construct a 1D Domain
        /// </summary>
        /// <param name="min">The lower bound of the domain.</param>
        /// <param name="max">The upper bound of the domain.</param>
        public Domain1d(double min = 0.0, double max = 1.0)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Convert to string of the form "From Min to Max"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"From {Min} to {Max}";
        }


        /// <summary>
        /// Split domain into two at a position within its extents. Positions at the domain's ends will be rejected.
        /// </summary>
        /// <param name="pos">The position value at which to split the domain.</param>
        /// <returns>An array of 2 1d domains split at the designated position.</returns>
        public Domain1d[] SplitAt(double pos)
        {
            if (!Includes(pos))
            {
                throw new Exception($"The position {pos} was not within the Grid's domain: {ToString()}");
            }

            return new Domain1d[] { new Domain1d(Min, pos), new Domain1d(pos, Max) };

        }

        /// <summary>
        /// Split a domain evenly into N subdomains.
        /// </summary>
        /// <param name="n">The number of domains</param>
        /// <returns>An array of N equally-sized subdomains.</returns>
        public Domain1d[] DivideByCount(int n)
        {
            if (n < 2)
            {
                return new Domain1d[] { this };
            }
            var results = new Domain1d[n];
            for (int i = 0; i < n; i++)
            {
                var from = ((1.0 / n) * i).MapToDomain(this);
                var to = ((1.0 / n) * (i + 1)).MapToDomain(this);
                results[i] = new Domain1d(from, to);
            }
            return results;
        }


    }
}
