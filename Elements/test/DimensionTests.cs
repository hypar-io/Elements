using System.Collections.Generic;
using Elements.Geometry;
using Elements.Search;
using Elements.Tests;
using Xunit;

namespace Elements
{
    public class DimensionTests : ModelTest
    {
        [Fact]
        public void AlignedDimension()
        {
            this.Name = "Elements_Dimension_Aligned";

            var l = Polygon.L(5, 5, 1);
            var m = new Mass(l, 1, BuiltInMaterials.Glass);
            this.Model.AddElement(m);

            var offset = 0.125;
            m.UpdateRepresentations();
            foreach (var f in m.Representation.SolidOperations[0].Solid.Faces)
            {
                var p = f.Value.Outer.ToPolygon();
                var segs = p.Segments();
                var plane = p.Plane();

                for (var i = 0; i < segs.Length; i++)
                {
                    var a = segs[i];
                    var right = a.Direction().Cross(plane.Normal);
                    var b = new Line(a.Start + right * offset, a.End + right * offset);
                    var d = new LinearDimension(a.Start, a.End, plane, b);
                    var draw = d.ToModelArrowsAndText();
                    this.Model.AddElements(draw);
                }
            }
        }

        [Fact]
        public void ContinuousDimension()
        {
            this.Name = "Elements_Dimension_Continuous";

            var l = Polygon.L(5, 5, 1);

            var m = new Mass(l, 1, BuiltInMaterials.Glass);
            this.Model.AddElement(m);

            // Flatten all polygon points along the X axis and sort.
            var pts = new List<Vector3>();
            var yz = new Plane(Vector3.Origin, Vector3.YAxis);
            foreach (var pt in l.Vertices)
            {
                var proj = pt.Project(yz);
                if (!pts.Contains(proj))
                {
                    pts.Add(proj);
                }
            }
            pts.Sort(new DirectionComparer(Vector3.XAxis));

            var refLine = new Line(new Vector3(0, 7, 0), new Vector3(1, 7, 0));
            for (var i = 0; i < pts.Count - 1; i++)
            {
                var d = new LinearDimension(pts[i], pts[i + 1], null, refLine);
                this.Model.AddElements(d.ToModelArrowsAndText());
            }
        }
    }
}