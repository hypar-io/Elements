#pragma warning disable CS1591

using Elements.Geometry;
using System.Collections.Generic;

namespace Elements
{
    public class HSSPipeProfile : Profile
    {
        public double OD {get; internal set;}
        public double ID {get; internal set;}
        public double t {get; internal set;}
        public double wt {get; internal set;}
        public double A {get;internal set;}
        public double I {get;internal set;}
        public double S {get;internal set;}
        public double r {get;internal set;}
        public double J {get;internal set;}

        public HSSPipeProfile(string name, double OD, double ID, double t) : base(name)
        {
            this.Perimeter = Polygon.Circle(OD);
            this.Voids = new Polygon[]{Polygon.Circle(ID).Reversed()};
        }
    }
}