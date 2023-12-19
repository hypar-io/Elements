#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elements.Geometry.Profiles
{
    public class HSSProfile : ParametricProfile
    {
        public double W;
        public double A;
        public double Ht;
        public double h;
        public double B;
        public double b;
        public double tnom;
        public double tdes;
        public double b_tdes;
        public double h_tdes;
        public double Ix;
        public double Zx;
        public double Sx;
        public double rx;
        public double Iy;
        public double Zy;
        public double Sy;
        public double ry;
        public double J;
        public double C;

        public HSSProfile() : base(new List<VectorExpression>(){
                                                    new VectorExpression("B/2", "Ht/2"),
                                                    new VectorExpression("-B/2", "Ht/2"),
                                                    new VectorExpression("-B/2", "-Ht/2"),
                                                    new VectorExpression("B/2", "-Ht/2")
                                                }, new List<List<VectorExpression>>(){
                                                    new List<VectorExpression>(){
                                                        new VectorExpression("B/2-tnom", "Ht/2-tnom"),
                                                        new VectorExpression("B/2-tnom", "-Ht/2+tnom"),
                                                        new VectorExpression("-B/2+tnom", "-Ht/2+tnom"),
                                                        new VectorExpression("-B/2+tnom", "Ht/2-tnom")
                                                    }
                                                }, id: Guid.NewGuid())
        { }

        [JsonConstructor]
        public HSSProfile(Polygon @perimeter,
                        IList<Polygon> @voids,
                        Guid @id = default,
                        string @name = null) : base(new List<VectorExpression>(){
                                                    new VectorExpression("B/2", "Ht/2"),
                                                    new VectorExpression("-B/2", "Ht/2"),
                                                    new VectorExpression("-B/2", "-Ht/2"),
                                                    new VectorExpression("B/2", "-Ht/2")
                                                }, new List<List<VectorExpression>>(){
                                                    new List<VectorExpression>(){
                                                        new VectorExpression("B/2-tnom", "Ht/2-tnom"),
                                                        new VectorExpression("B/2-tnom", "-Ht/2+tnom"),
                                                        new VectorExpression("-B/2+tnom", "-Ht/2+tnom"),
                                                        new VectorExpression("-B/2+tnom", "Ht/2-tnom")
                                                    }
                                                }, perimeter, voids, id, name)
        { }
    }
}