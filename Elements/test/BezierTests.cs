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
            this.GenerateIfc = false;
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
            var ctrlPts = new List<Vector3> { a, b, c, d, e, f };

            var bezier = new Bezier(ctrlPts);
            // </example>

            this.Model.AddElement(new ModelCurve(bezier));
        }

        [Fact]
        public void Bezier_Length_ZeroLength()
        {
            var a = Vector3.Origin;
            var b = Vector3.Origin;
            var c = Vector3.Origin;
            var ctrlPts = new List<Vector3> { a, b, c };
            var bezier = new Bezier(ctrlPts);

            var targetLength = 0;
            Assert.Equal(targetLength, bezier.Length());
        }

        [Fact]
        public void Bezier_Length_OffsetFromOrigin()
        {
            var b = new Vector3(5, 0, 1);
            var c = new Vector3(5, 5, 2);
            var d = new Vector3(0, 5, 3);
            var e = new Vector3(0, 0, 4);
            var f = new Vector3(5, 0, 5);
            var ctrlPts = new List<Vector3> { b, c, d, e, f };
            var bezier = new Bezier(ctrlPts);

            var expectedLength = 11.85;  // approximation as the linear interpolation used for calculating length is not hugely accurate
            Assert.Equal(expectedLength, bezier.Length(), 2);
            var divisions = 50; // brittle as it relies on number of samples within Bezier being unchanged
            var polylineLength = bezier.ToPolyline(divisions).Length();
            Assert.Equal(polylineLength, bezier.Length());
        }
    }
}