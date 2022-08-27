#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Elements.Geometry.Profiles
{
    public class STProfile : ParametricProfile
    {
        public double W;
        public double A;
        public double d;
        public double ddet;
        public double bf;
        public double bfdet;
        public double tw;
        public double twdet;
        public double twdet_2;
        public double tf;
        public double tfdet;
        public double kdes;
        public double kdet;
        public double y;
        public double yp;
        public double bf_2tf;
        public double D_t;
        public double Ix;
        public double Zx;
        public double Sx;
        public double rx;
        public double Iy;
        public double Zy;
        public double Sy;
        public double ry;
        public double J;
        public double Cw;
        public double ro;
        public double H;
        public double WGi;

        public STProfile() : base(new List<VectorExpression>(){
                                                    new VectorExpression("bf/2", "0"),
                                                    new VectorExpression("-bf/2", "0"),
                                                    new VectorExpression("-bf/2", "-tf"),
                                                    new VectorExpression("-tw/2", "-kdes"),
                                                    new VectorExpression("-tw/2", "-d"),
                                                    new VectorExpression("tw/2", "-d"),
                                                    new VectorExpression("tw/2", "-kdes"),
                                                    new VectorExpression("bf/2", "-tf")
                                                }, id: Guid.NewGuid())
        { }

        [JsonConstructor]
        public STProfile(Polygon @perimeter,
                        IList<Polygon> @voids,
                        Guid @id = default,
                        string @name = null) : base(new List<VectorExpression>(){
                                                   new VectorExpression("bf/2", "0"),
                                                    new VectorExpression("-bf/2", "0"),
                                                    new VectorExpression("-bf/2", "-tf"),
                                                    new VectorExpression("-tw/2", "-kdes"),
                                                    new VectorExpression("-tw/2", "-d"),
                                                    new VectorExpression("tw/2", "-d"),
                                                    new VectorExpression("tw/2", "-kdes"),
                                                    new VectorExpression("bf/2", "-tf")
                                                }, null, perimeter, voids, id, name)
        { }
    }
}