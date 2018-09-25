#pragma warning disable CS1591

using Hypar.Geometry;
using System.Collections.Generic;

namespace Hypar.Elements
{
    public class HSSPipeProfile : Profile
    {
        public string Shape{get; internal set;}
        public double OD {get; internal set;}
        public double ID {get; internal set;}
        public double t {get; internal set;}
        public double wt {get; internal set;}
        public double A {get;internal set;}
        public double I {get;internal set;}
        public double S {get;internal set;}
        public double r {get;internal set;}
        public double J {get;internal set;}

        public HSSPipeProfile(double OD, double ID, double t)
        {
            this.Perimeter = Polygon.Circle(OD);
            this.Voids = new List<Polygon>(){Polygon.Circle(ID)};
        }
    }
}