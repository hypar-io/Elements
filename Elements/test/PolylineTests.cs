using System.Collections.Generic;
using Elements.Tests;
using Xunit;

namespace Elements.Geometry.Tests
{
    public class PolylineTests : ModelTest
    {
        public PolylineTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void Polyline()
        {
            this.Name = "Elements_Geometry_Polyline";

            // <example>
            var a = new Vector3();
            var b = new Vector3(10, 10);
            var c = new Vector3(20, 5);
            var d = new Vector3(25, 10);

            var pline = new Polyline(new[] { a, b, c, d });
            var offset = pline.Offset(1, EndType.Square);
            // </example>

            this.Model.AddElement(new ModelCurve(pline, BuiltInMaterials.XAxis));
            this.Model.AddElement(new ModelCurve(offset[0], BuiltInMaterials.YAxis));

            Assert.Equal(4, pline.Vertices.Count);
            Assert.Equal(3, pline.Segments().Length);
        }

        [Fact]
        public void Polyline_ClosedOffset()
        {
            var length = 10;
            var offsetAmt = 1;
            var a = new Vector3();
            var b = new Vector3(length, 0);
            var pline = new Polyline(new[] { a, b });
            var offsetResults = pline.Offset(offsetAmt, EndType.Square);
            Assert.Single<Polygon>(offsetResults);
            var offsetResult = offsetResults[0];
            Assert.Equal(4, offsetResult.Vertices.Count);
            // offsets to a rectangle that's offsetAmt longer than the segment in
            // each direction, and 2x offsetAmt in width, so the long sides are 
            // each length + 2x offsetAmt, and the short sides are each 2x offsetAmt.
            var targetLength = 2 * length + 8 * offsetAmt;
            Assert.Equal(targetLength, offsetResult.Length(), 2);
        }


        [Fact]
        public void Polyline_ConsistentFrameOrientation()
        {
            var polyline = new Polyline(new[] {
              new Vector3(0,0,0),
              new Vector3(20,0,0),
              new Vector3(20,20,0),
              new Vector3(0, 20,0),
            });

            var bezier = new Bezier(new List<Vector3>{
                 new Vector3(0,0,0),
              new Vector3(20,0,0),
              new Vector3(20,20,0),
              new Vector3(0, 20,0),
            });

            var t_bez = bezier.TransformAt(0);

            var t1 = polyline.TransformAt(0);

            var p1 = $"🐝: {t_bez.XAxis}, {t_bez.YAxis}, {t_bez.ZAxis}";
            var p2 =  $"🍐: {t1.XAxis}, {t1.YAxis}, {t1.ZAxis}";

            //var polyline = new Polyline(new[] {
            //  new Vector3(0,0,0),
            //  new Vector3(20,0,0),
            //  new Vector3(20,20,0),
            //  new Vector3(0, 20,0),
            //});

            //var bz = new Bezier(new List<Vector3>
            //{
            //     new Vector3(0,0,0),
            //  new Vector3(20,0,0),
            //  new Vector3(20,20,0),
            //  new Vector3(0, 20,0),
            //});

            //var b1 = bz.TransformAt(0);
            //var t1 = polyline.TransformAt(0);
            //var t2 = polyline.TransformAt(0.01);

            //Assert.Equal(1.0, t1.XAxis.Dot(t2.XAxis));
            //Assert.Equal(1.0, t1.YAxis.Dot(t2.YAxis));
            //Assert.Equal(1.0, t1.ZAxis.Dot(t2.ZAxis));
        }

        [Fact]
        public void Polyline_ConsistentMiterFrameOrientation()
        {
            var polyline = new Polyline(new[] {
              new Vector3(0,0,0),
              new Vector3(10,0,0),
              new Vector3(20,0,0)
            });
            var t1 = polyline.TransformAt(0);
            var t2 = polyline.TransformAt(0.5);
            Assert.Equal(1.0, t1.XAxis.Dot(t2.XAxis));
            Assert.Equal(1.0, t1.YAxis.Dot(t2.YAxis));
            Assert.Equal(1.0, t1.ZAxis.Dot(t2.ZAxis));
        }
    }
}