using System.Text.Json.Serialization;

namespace Elements.Geometry
{
    /// <summary>
    /// A UV coordinate.
    /// </summary>
    public struct UV
    {
        /// <summary>The U coordinate.</summary>
        public double U { get; set; }

        /// <summary>The V coordinate.</summary>
        public double V { get; set; }

        /// <summary>
        /// Construct a uv coordinate.
        /// </summary>
        /// <param name="u">The U coordinate.</param>
        /// <param name="v">The V coordinate.</param>
        [JsonConstructor]
        public UV(double @u, double @v)
        {
            this.U = @u;
            this.V = @v;
        }

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
        /// Are the two UVs equal?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool operator ==(UV a, UV b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Are the two UVs equal?
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
        /// Automatically convert a tuple of two doubles into a UV.
        /// </summary>
        /// <param name="uv">An (u,v) tuple of doubles.</param>
        public static implicit operator UV((double u, double v) uv)
        {
            return new UV(uv.u, uv.v);
        }
    }
}