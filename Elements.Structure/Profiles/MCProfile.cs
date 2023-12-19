#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elements.Geometry.Profiles
{
    public class MCProfile : ParametricProfile
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
        public double k1;
        public double x;
        public double eo;
        public double xp;
        public double b_t;
        public double h_tw;
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
        public double Wno;
        public double Sw1;
        public double Sw2;
        public double Sw3;
        public double Qf;
        public double Qw;
        public double ro;
        public double H;
        public double rts;
        public double ho;
        public double PA;
        public double PB;
        public double PC;
        public double PD;
        public double T;
        public double WGi;

        public MCProfile() : base(new List<VectorExpression>(){
                                                    new VectorExpression("0", "0"),
                                                    new VectorExpression("bf", "0"),
                                                    new VectorExpression("bf", "tf"),
                                                    new VectorExpression("tw", "kdes"),
                                                    new VectorExpression("tw", "d - kdes"),
                                                    new VectorExpression("bf", "d - tf"),
                                                    new VectorExpression("bf", "d"),
                                                    new VectorExpression("0", "d")
                                                }, id: Guid.NewGuid())
        { }

        [JsonConstructor]
        public MCProfile(Polygon @perimeter,
                        IList<Polygon> @voids,
                        Guid @id = default,
                        string @name = null) : base(new List<VectorExpression>(){
                                                    new VectorExpression("0", "0"),
                                                    new VectorExpression("bf", "0"),
                                                    new VectorExpression("bf", "tf"),
                                                    new VectorExpression("tw", "kdes"),
                                                    new VectorExpression("tw", "d - kdes"),
                                                    new VectorExpression("bf", "d - tf"),
                                                    new VectorExpression("bf", "d"),
                                                    new VectorExpression("0", "d")
                                                }, null, perimeter, voids, id, name)
        { }
    }
}