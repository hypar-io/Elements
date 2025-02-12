#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elements.Geometry.Profiles
{
    public class WProfile : ParametricProfile
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
        public double bf_2tf;
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
        public double Qf;
        public double Qw;
        public double rts;
        public double ho;
        public double PA;
        public double PB;
        public double PC;
        public double PD;
        public double T;
        public double WGi;

        public WProfile() : base(new List<VectorExpression>(){
                                                    new VectorExpression("bf/2", "d/2"),
                                                    new VectorExpression("-bf/2", "d/2"),
                                                    new VectorExpression("-bf/2", "d/2 - tf"),
                                                    new VectorExpression("-tw/2", "d/2-tf"),
                                                    new VectorExpression("-tw/2", "-d/2 + tf"),
                                                    new VectorExpression("-bf/2", "-d/2 + tf"),
                                                    new VectorExpression("-bf/2", "-d/2"),
                                                    new VectorExpression("bf/2", "-d/2"), // bottom right
                                                    new VectorExpression("bf/2", "-d/2 + tf"),
                                                    new VectorExpression("tw/2", "-d/2 + tf"),
                                                    new VectorExpression("tw/2", "d/2 - tf"),
                                                    new VectorExpression("bf/2", "d/2 - tf")
                                                }, id: Guid.NewGuid())
        { }

        [JsonConstructor]
        public WProfile(Polygon @perimeter,
                        IList<Polygon> @voids,
                        Guid @id = default,
                        string @name = null) : base(new List<VectorExpression>(){
                                                    new VectorExpression("bf/2", "d/2"),
                                                    new VectorExpression("-bf/2", "d/2"),
                                                    new VectorExpression("-bf/2", "d/2 - tf"),
                                                    new VectorExpression("-tw/2", "d/2-tf"),
                                                    new VectorExpression("-tw/2", "-d/2 + tf"),
                                                    new VectorExpression("-bf/2", "-d/2 + tf"),
                                                    new VectorExpression("-bf/2", "-d/2"),
                                                    new VectorExpression("bf/2", "-d/2"), // bottom right
                                                    new VectorExpression("bf/2", "-d/2 + tf"),
                                                    new VectorExpression("tw/2", "-d/2 + tf"),
                                                    new VectorExpression("tw/2", "d/2 - tf"),
                                                    new VectorExpression("bf/2", "d/2 - tf")
                                                }, null, perimeter, voids, id, name)
        { }
    }
}