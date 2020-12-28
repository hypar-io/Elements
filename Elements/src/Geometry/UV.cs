using System;

namespace Elements.Geometry
{
    public partial struct UV
    {
        /// <summary>
        /// Are the two uvs equal?
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj)
        {
            if (!(obj is UV))
            {
                return false;
            }
            var uv = (UV)obj;
            return this.U == uv.U && this.V == uv.V;
        }

        /// <summary>
        /// Are the two UVs the same?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool operator ==(UV a, UV b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Are the two UVs the same?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool operator !=(UV a, UV b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Get the hash code for the uv.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// A string representation of the uv.
        /// </summary>
        public override string ToString()
        {
            return $"U:{this.U}, V:{this.V}";
        }

        /// <summary>
        /// Convert this UV to an array.
        /// </summary>
        public double[] ToArray()
        {
            return new[] { this.U, this.V };
        }

        /// <summary>
        /// Construct a UV from an array of numbers.
        /// </summary>
        /// <param name="uv"></param>
        public static UV FromArray(double[] uv)
        {
            if (uv.Length != 2)
            {
                throw new Exception($"A uv cannot be created from an array of {uv.Length} numbers.");
            }
            return new UV(uv[0], uv[1]);
        }
    }
}