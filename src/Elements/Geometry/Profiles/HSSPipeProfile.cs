#pragma warning disable CS1591
using System;

namespace Elements.Geometry.Profiles
{
    public class HSSPipeProfile : Profile
    {
        public double OuterDiam {get; internal set;}
        public double InnerDiam {get; internal set;}
        public double t {get; internal set;}
        public double wt {get; internal set;}
        public double A {get;internal set;}
        public double I {get;internal set;}
        public double S {get;internal set;}
        public double r {get;internal set;}
        public double J {get;internal set;}

        public HSSPipeProfile(string name, double outerDiam, double innerDiam, double t) : 
            base(Guid.NewGuid(), Polygon.Circle(outerDiam), new Polygon[]{Polygon.Circle(innerDiam).Reversed()}, name){}
    }
}