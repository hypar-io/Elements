using Elements;
using Elements.Geometry;
using Elements.Tests;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Hypar.Tests
{
    public class BezierTests : ModelTest
    {
        ITestOutputHelper _output;

        public BezierTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact, Trait("Category", "Examples")]
        public void Bezier()
        {
            this.Name = "Elements_Geometry_Bezier";
            
            // <example>
            var a = Vector3.Origin;
            var b = new Vector3(5, 0, 1);
            var c = new Vector3(5, 5, 2);
            var d = new Vector3(0, 5, 3);
            var e = new Vector3(0, 0, 4);
            var f = new Vector3(5, 0, 5);
            var ctrlPts = new List<Vector3>{a,b,c,d,e,f};

            var bezier = new Bezier(ctrlPts);
            // </example>

            this.Model.AddElement(new ModelCurve(bezier));
        }
    }
}