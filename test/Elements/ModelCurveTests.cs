using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class ModelCurveTests : ModelTest
    {
        [Fact]
        public void ModelCurves()
        {
            this.Name = "ModelCurves";
            var line = new Line(Vector3.Origin, new Vector3(5,5,5));
            var arc = new Arc(new Vector3(0,0,0), 2.0, 45.0, 135.0);
            var pline = Polygon.L(2, 2, 0.5);

            var lineModelCurve = new ModelCurve(line, new Material("Red", Colors.Red));
            var arcModelCurve = new ModelCurve(arc, new Material("Orange", Colors.Orange), new Transform(5, 0, 0));
            var plineModelCurve = new ModelCurve(pline, new Material("Purple", Colors.Purple), new Transform(10, 0, 0));
            
            this.Model.AddElements(new[]{lineModelCurve, arcModelCurve, plineModelCurve});

            var ptMaterial = new Material("Points", Colors.Blue);
            var modelPoints = new ModelPoints(ptMaterial);
            for(var x=0; x<20; x++)
            {
                for(var y=0; y<20; y++)
                {
                    for(var z=0; z<20; z++)
                    {
                        modelPoints.Locations.Add(new Vector3(x,y,z));
                    }
                }
            }
            
            this.Model.AddElement(modelPoints);
        }
    }
}