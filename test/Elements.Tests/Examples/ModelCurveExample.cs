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
            // Create some curves for your model curves.
            var line = new Line(Vector3.Origin, new Vector3(5,5,5));
            var arc = new Arc(Vector3.Origin, 2.0, 45.0, 135.0);
            var pline = Polygon.L(2, 2, 0.5);

            // Create model curves from the curves.
            var lineModelCurve = new ModelCurve(line, new Material("Red", Colors.Red));
            var arcModelCurve = new ModelCurve(arc, new Material("Orange", Colors.Orange), new Transform(5, 0, 0));
            var plineModelCurve = new ModelCurve(pline, new Material("Purple", Colors.Purple), new Transform(10, 0, 0));
            // </example>
            this.Model.AddElements(new[]{lineModelCurve, arcModelCurve, plineModelCurve});
        }
    }
}