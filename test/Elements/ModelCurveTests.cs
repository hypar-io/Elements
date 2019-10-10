using System.Collections.Generic;
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
        }

        [Fact]
        public void OffsetModelCurves()
        {
            this.Name = "OffsetModelCurves";

            var pline = Polygon.L(2, 2, 0.5);
            var mcs = new List<ModelCurve>();
            var m = new Material("Purple", Colors.Blue);
            var plineModelCurve = new ModelCurve(pline, m);
            mcs.Add(plineModelCurve);
            for(var i=0; i<100; i++)
            {
                pline = pline.Offset(1.0)[0];
                plineModelCurve = new ModelCurve(pline, m);
                mcs.Add(plineModelCurve);
            }
            this.Model.AddElements(mcs);
        }

        [Fact]
        public void ModelPoints()
        {
            this.Name = "ModelPoints";

            var ptMaterial = new Material("Points", Colors.Blue);
            var modelPoints = new ModelPoints(ptMaterial);
            for(var x=0; x<50; x++)
            {
                for(var y=0; y<50; y++)
                {
                    for(var z=0; z<50; z++)
                    {
                        modelPoints.Locations.Add(new Vector3(x,y,z));
                    }
                }
            }
            
            this.Model.AddElement(modelPoints);
        }
    }
}