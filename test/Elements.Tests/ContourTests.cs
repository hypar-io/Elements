using System.Collections.Generic;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Xunit;

namespace Elements.Tests
{
    public class ContourTests : ModelTest
    {
        [Fact]
        public void Contour()
        {
            this.Name = "Contour";

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
            var l2 = t.OfBezier(l1);
            l2.ControlPoints.Reverse();
            var a2 = new Arc(new Vector3(0, 0), r, 90.0, 270.0);

            var mc1 = new ModelCurve(l1);
            var mc2 = new ModelCurve(l2);
            var mc3 = new ModelCurve(a1);
            var mc4 = new ModelCurve(a2);
            this.Model.AddElements(new[]{mc1, mc2, mc3, mc4});

            var contour = new Contour(new List<Curve> { l1, a1, l2, a2 });
            var mass = new Mass(new Profile(contour.ToPolygon()));
            this.Model.AddElement(mass);

            this.Model.ToGlTF("Contour.gltf", false);
        }
    }
}