using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Tests;

namespace Elements.Components.Tests
{
    public class ComponentTest : ModelTest
    {
        public ComponentTest()
        {
            GenerateIfc = false;
        }

        public void ArrayResults(ComponentDefinition def, List<List<Vector3>> targetAnchors)
        {
            var currentX = 0.0;
            foreach (var anchorSet in targetAnchors)
            {
                var bbox = new BBox3(anchorSet);
                var xForm = new Transform(currentX - bbox.Min.X, 0, 0);
                var transformedSet = anchorSet.Select(v => xForm.OfPoint(v)).ToList();
                foreach (var anchor in transformedSet)
                {
                    Model.AddElement(new ModelCurve(Polygon.Ngon(10, 0.1), transform: new Transform(anchor)));
                }
                Model.AddElement(def.Instantiate(transformedSet));
                currentX += (bbox.Max.X - bbox.Min.X + 1);
            }
        }
    }
}