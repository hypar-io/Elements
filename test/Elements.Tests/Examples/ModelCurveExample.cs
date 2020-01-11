using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class ModelCurveExample : ModelTest
    {
        [Fact]
        public void Example()
        {
            this.Name = "Elements_ModelCurve";

            // <example>
            // A line
            var line = new Line(Vector3.Origin, new Vector3(5,5,5));

            // An arc
            var arc = new Arc(Vector3.Origin, 2.0, 45.0, 135.0);

            // A polygon
            var pline = Polygon.L(2, 2, 0.5);

            // A Bezier
            var a = Vector3.Origin;
            var b = new Vector3(5, 0, 1);
            var c = new Vector3(5, 5, 2);
            var d = new Vector3(0, 5, 3);
            var e = new Vector3(0, 0, 4);
            var f = new Vector3(5, 0, 5);
            var ctrlPts = new List<Vector3>{a,b,c,d,e,f};
            var bezier = new Bezier(ctrlPts);

            // Create model curves from the curves.
            var lineModelCurve = new ModelCurve(line, new Material("Red", Colors.Red));
            var arcModelCurve = new ModelCurve(arc, new Material("Orange", Colors.Orange), new Transform(5, 0, 0));
            var plineModelCurve = new ModelCurve(pline, new Material("Purple", Colors.Purple), new Transform(10, 0, 0));
            var bezierModelCurve = new ModelCurve(bezier, new Material("Green", Colors.Green), new Transform(15, 0, 0));
            // </example>
            this.Model.AddElements(new[]{lineModelCurve, arcModelCurve, plineModelCurve});
        }
    }
}