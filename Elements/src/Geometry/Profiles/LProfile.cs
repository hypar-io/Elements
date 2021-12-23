#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elements.Geometry.Profiles
{
    public class LProfile : ParametricProfile
    {
        public double W;
        public double A;
        public double d;
        public double b;
        public double t;
        public double kdes;
        public double kdet;
        public double x;
        public double y;
        public double xp;
        public double yp;
        public double b_t;
        public double Ix;
        public double Zx;
        public double Sx;
        public double rx;
        public double Iy;
        public double Zy;
        public double Sy;
        public double ry;
        public double Iz;
        public double rz;
        public double Sz;
        public double J;
        public double Cw;
        public double ro;
        public double H;
        public double tan_α;
        public double Iw;
        public double zA;
        public double zB;
        public double zC;
        public double wA;
        public double wB;
        public double wC;
        public double SwA;
        public double SwB;
        public double SwC;
        public double SzA;
        public double SzB;
        public double SzC;
        public double PA;
        public double PA2;
        public double PB;

        public LProfile() : base(new List<VectorExpression>(){
                                                    new VectorExpression("0", "0"),
                                                    new VectorExpression("b", "0"),
                                                    new VectorExpression("b", "t"),
                                                    new VectorExpression("t", "t"),
                                                    new VectorExpression("t", "d"),
                                                    new VectorExpression("0", "d"),
                                                }, id: Guid.NewGuid())
        { }

        [JsonConstructor]
        public LProfile(Polygon @perimeter,
                        IList<Polygon> @voids,
                        Guid @id = default,
                        string @name = null) : base(new List<VectorExpression>(){
                                                    new VectorExpression("0", "0"),
                                                    new VectorExpression("b", "0"),
                                                    new VectorExpression("b", "t"),
                                                    new VectorExpression("t", "t"),
                                                    new VectorExpression("t", "d"),
                                                    new VectorExpression("0", "d"),
                                                }, null, perimeter, voids, id, name)
        { }
    }
}