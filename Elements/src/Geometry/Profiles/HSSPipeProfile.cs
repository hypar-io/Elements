#pragma warning disable CS1591
using System;
using System.Text.Json.Serialization;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A hollow structural steel profile.
    /// </summary>
    public class HSSPipeProfile : Profile
    {
        public double OuterDiam { get; internal set; }
        public double InnerDiam { get; internal set; }
        public double t { get; internal set; }

        [JsonIgnore]
        public double wt { get; internal set; }

        [JsonIgnore]
        public double A { get; internal set; }

        [JsonIgnore]
        public double I { get; internal set; }

        [JsonIgnore]
        public double S { get; internal set; }

        [JsonIgnore]
        public double r { get; internal set; }

        [JsonIgnore]
        public double J { get; internal set; }

        /// <summary>
        /// Construct a hollow structural steel profile.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="outerDiam"></param>
        /// <param name="innerDiam"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        [JsonConstructor]
        public HSSPipeProfile(string name,
                              Guid id,
                              double outerDiam,
                              double innerDiam,
                              double t) :
            base(new Circle(outerDiam).ToPolygon(10),
                 new Polygon[] { new Circle(innerDiam).ToPolygon(10).Reversed() },
                 id,
                 name)
        {
            this.OuterDiam = outerDiam;
            this.InnerDiam = innerDiam;
            this.t = t;
        }
    }
}