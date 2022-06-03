using System.Collections.Generic;
using Elements.Dimensions;
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

            // <aligned_dimension_example>
            var l = Polygon.L(5, 5, 1);
            var m = new Mass(l, 1, BuiltInMaterials.Glass);
            this.Model.AddElement(m);

            var offset = 0.125;
            m.UpdateRepresentations();
            var dimensions = new List<LinearDimension>();
            foreach (var f in m.Representation.SolidOperations[0].Solid.Faces)
            {
                var p = f.Value.Outer.ToPolygon();
                var segs = p.Segments();
                var plane = p.Plane();

                for (var i = 0; i < segs.Length; i++)
                {
                    var a = segs[i];
                    var d = new AlignedDimension(a.Start, a.End, plane, offset);
                    dimensions.Add(d);
                }
            }
            // </aligned_dimension_example>
            this.Model.AddElements(dimensions);
            this.Model.AddElements(LinearDimension.ToModelArrowsAndTexts(Colors.Granite, dimensions));
        }

        [Fact]
        public void ContinuousDimension()
        {
            this.Name = "Elements_Dimension_Continuous";

            // <continuous_dimension_example>
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

            var dimensions = new List<LinearDimension>();
            var refLine = new Line(new Vector3(0, 7, 0), new Vector3(1, 7, 0));
            for (var i = 0; i < pts.Count - 1; i++)
            {
                var d = new ContinuousDimension(pts[i], pts[i + 1], refLine, null);
                dimensions.Add(d);
            }
            // </continuous_dimension_example>

            this.Model.AddElements(LinearDimension.ToModelArrowsAndTexts(Colors.Granite, dimensions));
        }

        [Fact]
        public void DimensionDisplayValue()
        {
            this.Name = nameof(DimensionDisplayValue);

            var l = Polygon.L(5, 5, 1);
            var m = new ModelCurve(l);
            this.Model.AddElement(m);

            var segs = l.Segments();
            var dimensions = new List<LinearDimension>();
            var plane = new Plane(Vector3.Origin, Vector3.ZAxis);
            var offset = 0.125;

            for (var i = 0; i < segs.Length; i++)
            {
                var a = segs[i];
                var d = new AlignedDimension(a.Start, a.End, plane, offset)
                {
                    DisplayValue = $"Polygon {i}"
                };
                dimensions.Add(d);
            }

            this.Model.AddElements(dimensions);
            this.Model.AddElements(LinearDimension.ToModelArrowsAndTexts(Colors.Granite, dimensions));
        }
    }
}