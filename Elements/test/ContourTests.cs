using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class ContourTests : ModelTest
    {
        public ContourTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void Contour()
        {
            this.Name = "Elements_Geometry_Contour";

            // <example>
            var r = 1.0;

            // The reflection plane.
            var t = new Transform();
            t.Reflect(Vector3.YAxis);

            var ctrlPoints = new List<Vector3>{
                new Vector3(0, -r),
                new Vector3(1.25, 1),
                new Vector3(3.75, -1),
                new Vector3(5, -r)
            };
            var l1 = new Bezier(ctrlPoints);
            var a1 = new Arc(new Vector3(5, 0), r, -90.0, 90.0);
            var l2 = (Bezier)l1.Transformed(t);
            l2.ControlPoints.Reverse();
            var a2 = new Arc(new Vector3(0, 0), r, 90.0, 270.0);

            var contour = new Contour(new List<BoundedCurve> { l1, a1, l2, a2 });
            // </example>

            this.Model.AddElement(new ModelCurve(contour.ToPolygon()));
        }
    }
}