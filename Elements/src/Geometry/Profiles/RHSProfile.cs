#pragma warning disable CS1591
using System;
using Newtonsoft.Json;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A rectangular hollow section profile.
    /// </summary>
    public class RHSProfile : Profile
    {
        // A,B,t,M,A,Ix,Iy,ix,iy,Zx,Zy,Zpx,Zpy,ti,tm,sa,nl,J
        public double A { get; internal set; }

        public double B { get; internal set; }

        public double t { get; internal set; }

        [JsonIgnore]
        public double M { get; internal set; }

        [JsonIgnore]
        public double Ix { get; internal set; }

        [JsonIgnore]
        public double Iy { get; internal set; }

        [JsonIgnore]
        public double ix { get; internal set; }

        [JsonIgnore]
        public double iy { get; internal set; }

        [JsonIgnore]
        public double Zx { get; internal set; }

        [JsonIgnore]
        public double Zy { get; internal set; }

        [JsonIgnore]
        public double Zpx { get; internal set; }

        [JsonIgnore]
        public double Zpy { get; internal set; }

        [JsonIgnore]
        public double ti { get; internal set; }

        [JsonIgnore]
        public double tm { get; internal set; }

        [JsonIgnore]
        public double sa { get; internal set; }

        [JsonIgnore]
        public double nl { get; internal set; }

        [JsonIgnore]
        public double J { get; internal set; }

        /// <summary>
        /// Construct a rectangular hollow section profile.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        [JsonConstructor]
        public RHSProfile(string name,
                          Guid id,
                          double a,
                          double b,
                          double t) : base(Polygon.Rectangle(b, a),
                                           new[] { Polygon.Rectangle(b - 2 * t, a - 2 * t).Reversed() },
                                           id,
                                           name)
        {
            this.A = a;
            this.B = b;
            this.t = t;
        }
    }
}