using System;
namespace Elements.MathUtils
{
    public struct Domain1d
    {
        public Domain1d(double min = 0.0, double max = 1.0)
        {
            Min = min;
            Max = max;
        }

        public double Min { get; }
        public double Max { get; }

        public double Length => Max - Min;

        internal bool IsIncreasing()
        {
            return Max > Min;
        }

        /// <summary>
        /// Returns true if pos is within the domain (exclusive of its ends)
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        internal bool Includes(double pos)
        {
            if (IsIncreasing())
            {
                return pos < Max && pos > Min;
            }
            else
            {
                return pos < Min && pos < Max;
            }
        }

        public override string ToString()
        {
            return $"From {Min} To {Max}";
        }


        /// <summary>
        /// Split domain into two at a position within its extents. Positions at the domain's ends will be rejected.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Domain1d[] SplitAt(double pos)
        {
            if (!Includes(pos))
            {
                throw new Exception($"The position {pos} was not within the Grid's domain: {ToString()}");
            }

            return new Domain1d[] { new Domain1d(Min, pos), new Domain1d(pos, Max) };

        }

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
