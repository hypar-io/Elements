#pragma warning disable CS1591
using System;
using Newtonsoft.Json;

namespace Elements.Geometry.Profiles
{
    public class HSSPipeProfile : Profile
    {
        public double OuterDiam {get; internal set;}
        public double InnerDiam {get; internal set;}
        public double t {get; internal set;}

        [JsonIgnore]
        public double wt {get; internal set;}

        [JsonIgnore]
        public double A {get;internal set;}

        [JsonIgnore]
        public double I {get;internal set;}

        [JsonIgnore]
        public double S {get;internal set;}

        [JsonIgnore]
        public double r {get;internal set;}

        [JsonIgnore]
        public double J {get;internal set;}

        public HSSPipeProfile(string name,
                              double outerDiam,
                              double innerDiam,
                              double t,
                              Guid id = default(Guid)) : 
            base(Polygon.Circle(outerDiam), new Polygon[]{Polygon.Circle(innerDiam).Reversed()}, id, name)
            {
                this.OuterDiam = outerDiam;
                this.InnerDiam = innerDiam;
                this.t = t;
            }
    }
}