using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Analysis;
using Elements.Geometry;
using Xunit;



namespace Elements.Tests
{
    public class ConvexHullTests : ModelTest
    {
        [Fact]
        public void ConvexHullOrthogonalWithDuplicateVertices()
        {
            var points = new List<Vector3>()
            {
                new Vector3(51.8160, 6.0960, 45.7200),
                new Vector3(0.0000, 6.0960, 45.7200),
                new Vector3(0.0000, 24.3840, 45.7200),
                new Vector3(51.8160, 24.3840, 45.7200),
                new Vector3(51.8160, 24.3840, 45.7200),
                new Vector3(51.8160, 6.0960, 45.7200)
            };
            var hull = ConvexHull.FromPoints(points);
            Assert.Equal(4, hull.Segments().Length);
        }

        [Fact]
        public void ConvexHullFrom3DCloudAndNormal()
        {
            Name = nameof(ConvexHullFrom3DCloudAndNormal);
            var L = Polygon.L(2, 4, 1).TransformedPolygon(new Transform().Moved(3, 4));
            var jitteredVertices = L.Vertices.ToList();

            jitteredVertices[0] = new Transform().Moved(0, 0, 0.1).OfPoint(jitteredVertices[0]);
            jitteredVertices[1] = new Transform().Moved(0, 0, -0.1).OfPoint(jitteredVertices[1]);

            var flatHull = ConvexHull.FromPoints(L.Vertices);

            var jitteredHull = ConvexHull.FromPointsInPlane(jitteredVertices, Vector3.ZAxis);
            Assert.True(jitteredHull.IsAlmostEqualTo(flatHull), "Jittered flat hull doesn't match original hull.");

            // Simple transform allows simple polygon comparison for Assert.
            var simpleTransform = new Transform(Vector3.Origin, new Vector3(1, 0, 0).Unitized(), 0);
            var liftedPoints = jitteredVertices.Select(p => simpleTransform.OfPoint(p));
            var liftedHull = ConvexHull.FromPointsInPlane(liftedPoints, Vector3.XAxis);
            Assert.True(liftedHull.IsAlmostEqualTo(flatHull.TransformedPolygon(simpleTransform)), "The lifted hull doesn't match the transformed flat hull.");

            // Complex transform can be used for visual inspection of the resulting frame.
            var complexTransform = new Transform(Vector3.Origin, new Vector3(1, 2, 0).Unitized(), 0);
            var complexLiftedPoints = jitteredVertices.Select(p => complexTransform.OfPoint(p));
            var complexLiftedHull = ConvexHull.FromPointsInPlane(complexLiftedPoints, Vector3.XAxis);
            var complexliftedHull2 = ConvexHull.FromPointsInPlane(complexLiftedPoints, new Vector3(1, 1, 0)); ;

            this.Model.AddElement(L);
            this.Model.AddElement(new ModelCurve(flatHull, new Material("aqua", Colors.Aqua)));
            this.Model.AddElement(new ModelCurve(jitteredHull, new Material("cobalt", Colors.Cobalt)));
            this.Model.AddElements(CylinderAtPoints(complexLiftedPoints));
            this.Model.AddElement(new ModelCurve(complexLiftedHull, new Material("cobalt", Colors.Cobalt)));
            this.Model.AddElement(new ModelCurve(complexliftedHull2, new Material("crimson", Colors.Crimson)));
        }

        private static IEnumerable<Mass> CylinderAtPoints(IEnumerable<Vector3> liftedPoints)
        {
            return liftedPoints.Select(p => new Mass(new Circle(p, 0.02).ToPolygon(), 0.02) { Transform = new Transform(0, 0, p.Z), Material = BuiltInMaterials.XAxis });
        }
    }
}