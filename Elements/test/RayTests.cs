using System;
using System.Collections.Generic;
using Elements.Analysis;
using Elements.Geometry;
using solids = Elements.Geometry.Solids;
using Xunit;
using Xunit.Abstractions;
using System.Diagnostics;
using Vertex = Elements.Geometry.Vertex;
using Xunit.Sdk;
using System.Linq;

namespace Elements.Tests
{
    public class RayTests : ModelTest
    {
        private ITestOutputHelper _output;

        public RayTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void TriangleIntersection()
        {
            var a = new Vertex(new Vector3(-0.5, -0.5, 1.0));
            var b = new Vertex(new Vector3(0.5, -0.5, 1.0));
            var c = new Vertex(new Vector3(0, 0.5, 1.0));
            var t = new Triangle(a, b, c);
            var r = new Ray(Vector3.Origin, Vector3.ZAxis);
            Vector3 xsect;
            Assert.True(r.Intersects(t, out xsect));

            r = new Ray(Vector3.Origin, Vector3.ZAxis.Negate());
            Assert.False(r.Intersects(t, out xsect));
        }

        [Fact]
        public void IntersectsAtVertex()
        {
            var a = new Vertex(new Vector3(-0.5, -0.5, 1.0));
            var b = new Vertex(new Vector3(0.5, -0.5, 1.0));
            var c = new Vertex(new Vector3(0, 0.5, 1.0));
            var t = new Triangle(a, b, c);
            var r = new Ray(new Vector3(-0.5, -0.5, 0.0), Vector3.ZAxis);
            Vector3 xsect;
            Assert.True(r.Intersects(t, out xsect));
        }

        [Fact]
        public void IsParallelTo()
        {
            var a = new Vertex(new Vector3(-0.5, -0.5, 1.0));
            var b = new Vertex(new Vector3(0.5, -0.5, 1.0));
            var c = new Vertex(new Vector3(0, 0.5, 1.0));
            var t = new Triangle(a, b, c);
            var r = new Ray(Vector3.Origin, Vector3.XAxis);
            Vector3 xsect;
            Assert.False(r.Intersects(t, out xsect));
        }

        [Fact]
        public void RayIntersectsTopography()
        {
            this.Name = "RayIntersectTopo";

            var elevations = new double[100];

            int e = 0;
            for (var x = 0; x < 10; x++)
            {
                for (var y = 0; y < 10; y++)
                {
                    elevations[e] = Math.Sin(((double)x / 10.0) * Math.PI) * 5;
                    e++;
                }
            }
            var topo = new Topography(Vector3.Origin, 10, elevations)
            {
                Material = new Material("topo", new Color(0.5, 0.5, 0.5, 0.5)),
                Transform = new Transform(0, 0, 2)
            };
            this.Model.AddElement(topo);
            this.Model.AddElements(new Transform().ToModelCurves());
            for (int i = 1; i < 9; i++)
            {
                for (int j = 1; j < 9; j++)
                {
                    var newRay = new Ray(new Vector3(i, j, 40), Vector3.ZAxis.Negate());
                    var intersect = newRay.Intersects(topo, out var result2);
                    Assert.True(intersect);
                    var line = new Line(result2, newRay.Origin);
                    Model.AddElement(new ModelCurve(line, BuiltInMaterials.XAxis));
                    Assert.True(result2.Z > elevations.Min() + 2 && result2.Z < elevations.Max() + 2);
                }
            }
        }

        [Fact]
        public void RayIntersectsSolidOperation()
        {
            var polygon = new Polygon(new[]
            {
                new Vector3(0,0,0),
                new Vector3(4,0,0),
                new Vector3(0,4,0)
            });
            var extrude = new solids.Extrude(polygon, 10, new Vector3(1, 1, 1), false);

            var ray = new Ray(new Vector3(-2, 0, 3), new Vector3(2, 1, 0));
            var doesIntersect = ray.Intersects(extrude, out List<Vector3> result);
            Assert.True(doesIntersect);
            Assert.Equal(new Vector3(4, 3, 3), result[0]);

        }

        [Fact]
        public void RayDoesNotIntersectWhenPointingAwayFromSolid()
        {
            var polygon = new Polygon(new[]
         {
                new Vector3(0,0,0),
                new Vector3(4,0,0),
                new Vector3(0,4,0)
            });
            var extrude = new solids.Extrude(polygon, 10, new Vector3(1, 1, 1), false);

            var ray = new Ray(new Vector3(6, 6, 0), new Vector3(1, 0, 0));
            var doesIntersect = ray.Intersects(extrude, out List<Vector3> result3);
            Assert.False(doesIntersect);
        }

        [Fact]
        public void RayIntersectsAlongEdge()
        {
            var polygon = new Polygon(new[]
          {
                new Vector3(0,0,0),
                new Vector3(4,0,0),
                new Vector3(0,4,0)
            });
            var extrude = new solids.Extrude(polygon, 10, new Vector3(1, 1, 1), false);

            var ray2 = new Ray(new Vector3(6, 6, 6), new Vector3(1, 1, 1));
            var doesIntersect2 = ray2.Intersects(extrude, out List<Vector3> result2);
            Assert.True(doesIntersect2);
        }

        [Fact]
        public void RayIntersectsFromInsideSolid()
        {
            var polygon = new Polygon(new[]
            {
                new Vector3(0,0,0),
                new Vector3(4,0,0),
                new Vector3(0,4,0)
            });
            var extrude = new solids.Extrude(polygon, 10, new Vector3(1, 1, 1), false);

            var ray = new Ray(new Vector3(3, 3, 2), new Vector3(0, 1, 0));
            var doesIntersect = ray.Intersects(extrude, out List<Vector3> result);
            Assert.True(doesIntersect);
            Assert.Equal(new Vector3(3, 5, 2), result[0]);
        }

        [Fact]
        public void RayIntersectsSolidAtVertex()
        {
            var polygon = new Polygon(new[]
            {
                new Vector3(0,0,0),
                new Vector3(4,0,0),
                new Vector3(0,4,0)
            });
            var extrude = new solids.Extrude(polygon, 10, new Vector3(1, 1, 1), false);

            var ray = new Ray(new Vector3(4, 0, 0), new Vector3(1, 0, 0));
            var doesIntersect = ray.Intersects(extrude, out List<Vector3> result);
            Assert.True(doesIntersect);
            Assert.Equal(new Vector3(4, 0, 0), result[0]);
        }

        [Fact]
        public void RayIntersectsWhenOriginLiesOnFace()
        {
            var polygon = new Polygon(new[]
            {
                new Vector3(0,0,0),
                new Vector3(4,0,0),
                new Vector3(0,4,0)
            });
            var extrude = new solids.Extrude(polygon, 10, new Vector3(1, 1, 1), false);
            var ray = new Ray(new Vector3(2.88675, 4.88675, 2.88675), new Vector3(-1, 0, 1));
            var doesIntersect = ray.Intersects(extrude, out List<Vector3> result);
            Assert.True(doesIntersect);
            Assert.Equal(ray.Origin, result[0]);
        }

        [Fact]
        public void IntersectRay()
        {
            var ray1 = new Ray(Vector3.Origin, Vector3.XAxis);
            var ray2 = new Ray(new Vector3(5, -5), Vector3.YAxis);
            ray1.Intersects(ray2, out Vector3 result);
            Assert.True(result.Equals(new Vector3(5, 0)));

            ray2 = new Ray(new Vector3(5, -5), Vector3.YAxis.Negate());
            Assert.False(ray1.Intersects(ray2, out result));

            Assert.True(ray1.Intersects(ray2, out result, true));
        }

        [Fact]
        public void CoincidentRays()
        {
            // Coincident, staggered rays.
            var ray1 = new Ray(Vector3.Origin, Vector3.XAxis);

            var ray2 = new Ray(new Vector3(-1, 0, 0), Vector3.XAxis);
            var intersection = ray1.Intersects(ray2, out _, out var intersectionType);
            Assert.True(intersection);
            Assert.Equal(RayIntersectionResult.Coincident, intersectionType);

            ray2 = new Ray(Vector3.Origin, Vector3.XAxis.Negate());
            intersection = ray1.Intersects(ray2, out _, out intersectionType);
            Assert.True(intersection);
            Assert.Equal(RayIntersectionResult.Coincident, intersectionType);
        }

        [Fact]
        public void ParallelRays()
        {
            var ray1 = new Ray(Vector3.Origin, Vector3.XAxis);
            var ray2 = new Ray(new Vector3(0, 0, 1), Vector3.XAxis);
            var intersection = ray1.Intersects(ray2, out _, out var intersectionType);
            Assert.False(intersection);
            Assert.Equal(RayIntersectionResult.Parallel, intersectionType);
        }

        [Fact]
        public void IntersectingRays()
        {
            var ray1 = new Ray(Vector3.Origin, Vector3.XAxis);
            var ray2 = new Ray(new Vector3(5, -5), Vector3.YAxis);
            var intersection = ray1.Intersects(ray2, out _, out var intersectionType);
            Assert.True(intersection);
            Assert.Equal(RayIntersectionResult.Intersect, intersectionType);
        }

        [Fact]
        public void SkewedRays()
        {
            var ray1 = new Ray(Vector3.Origin, Vector3.XAxis);
            var ray2 = new Ray(new Vector3(5, 5), Vector3.ZAxis);
            var intersection = ray1.Intersects(ray2, out _, out var intersectionType);
            Assert.False(intersection);
            Assert.Equal(RayIntersectionResult.None, intersectionType);
        }

        [Fact]
        public void IntersectingRaysIgnoringDirection()
        {
            var ray1 = new Ray(Vector3.Origin, Vector3.XAxis);
            var ray2 = new Ray(new Vector3(5, -5), Vector3.YAxis.Negate());
            var intersection = ray1.Intersects(ray2, out _, out var intersectionType, true);
            Assert.True(intersection);
            Assert.Equal(RayIntersectionResult.Intersect, intersectionType);

            intersection = ray1.Intersects(ray2, out _, out intersectionType, false);
            Assert.False(intersection);
            Assert.Equal(RayIntersectionResult.None, intersectionType);
        }

        [Fact]
        public void ParallelRayPointingInOppositeDirection()
        {
            var ray = new Ray(Vector3.Origin, Vector3.XAxis.Negate());
            Assert.False(ray.Intersects(new Vector3(1, 0, 0), new Vector3(3, 0, 0), out _, out _));
        }

        [Fact]
        public void DistanceToRay()
        {
            // A ray pointing along the negative X axis.
            var ray = new Ray(Vector3.Origin, Vector3.XAxis.Negate());

            // A point "behind" the ray.
            Assert.Equal(0, new Vector3(1, 0, 0).DistanceTo(ray));

            // A point at the ray's origin.
            Assert.Equal(0, new Vector3(0, 0, 0).DistanceTo(ray));

            // A point 1 unit from the ray's origin.
            Assert.Equal(1, new Vector3(0, 1, 0).DistanceTo(ray));

            // A point -5 units along the ray
            Assert.Equal(0, new Vector3(-5, 0, 0).DistanceTo(ray));
        }

        [Fact]
        public void RayShadowTest()
        {
            this.Name = "RayShadowTest";

            var outer = Polygon.Rectangle(3, 3);
            var inner = Polygon.Rectangle(1.5, 1.5);

            var mass = new Mass(new Profile(outer, inner.Reversed()), 2);
            mass.Transform.Move(new Vector3(0, 0, 1));
            mass.Transform.Rotate(Vector3.ZAxis, 45);
            mass.UpdateRepresentations();

            var light = new Vector3(4, 4, 10);
            var colorScale = new ColorScale(new List<Color> { Colors.White, Colors.Darkgray });
            var analyze = new Func<Vector3, double>((v) =>
            {
                var ray = new Ray(v, (light - v).Unitized());
                if (ray.Intersects(mass, out List<Vector3> results))
                {
                    var hit = results[results.Count - 1];
                    if (!v.Equals(hit))
                    {
                        var hitLine = new ModelCurve(new Line(light, hit), BuiltInMaterials.XAxis);
                        this.Model.AddElement(hitLine);
                    }
                    return 1.0;
                }
                return 0.0;
            });

            var sw = new Stopwatch();
            sw.Start();
            var analysisMesh = new AnalysisMesh(Polygon.Rectangle(10, 10), 0.5, 0.5, colorScale, analyze);
            sw.Stop();
            this._output.WriteLine($"Analysis mesh constructed in {sw.Elapsed.TotalMilliseconds}ms.");
            sw.Reset();

            sw.Start();
            analysisMesh.Analyze();
            sw.Stop();
            this._output.WriteLine($"Shot {analysisMesh.TotalAnalysisLocations} rays in {sw.Elapsed.TotalMilliseconds}ms.");

            this.Model.AddElements(new Element[] { mass, analysisMesh });
        }

        [Fact]
        private static void RayIntersectsNewlyGeneratedElement()
        {
            var wall = new StandardWall(new Line(Vector3.Origin, new Vector3(10, 0, 0)), 0.3, 3);
            var ray = new Ray(new Vector3(5, 5, 1), new Vector3(0, -1, 0));
            var doesIntersect = ray.Intersects(wall, out var result);
            Assert.True(doesIntersect);
        }

        [Fact]
        private static void RayIntersectsPolygon()
        {
            var min = new Vector3();
            var max = new Vector3(5, 5);
            var polygon = Polygon.Rectangle(min, max);
            Assert.True(new Ray(new Vector3(0, 0, -1), new Vector3(0, 0, 1)).Intersects(polygon, out var _, out _));
            Assert.True(new Ray(new Vector3(2.5, 2.5, -1), new Vector3(0, 0, 1)).Intersects(polygon, out var _, out _));
            Assert.False(new Ray(new Vector3(-1, -1, -1), new Vector3(0, 0, 1)).Intersects(polygon, out var _, out _));
        }

        [Fact]
        private static void RayIntersectsGeometryWithTransformation()
        {
            var outer = Polygon.Rectangle(6, 6);
            var mass = new Mass(new Profile(outer), 2);
            var ray = new Ray(new Vector3(3.5, 0, -4), new Vector3(0, 0, 1));
            //The mass (-3,-3,0);(3,3,2) not intersects with the ray
            Assert.False(ray.Intersects(mass, out var _));
            //Rotated mass (0,-3,3);(2,3,-3)
            mass.Transform.Rotate(Vector3.YAxis, 90);
            //Translated mass (2,-3,3);(4,3,-3) and it crosses the ray
            mass.Transform.Move(new Vector3(2, 0, 0));
            mass.UpdateRepresentations();
            Assert.True(ray.Intersects(mass, out var _));
        }

        [Fact]
        private void RayNearbyPoints()
        {
            Name = nameof(RayNearbyPoints);
            var points = new List<Vector3>();
            var random = new Random(1);
            for (int i = 0; i < 1000; i++)
            {
                var point = new Vector3(random.NextDouble() * 10, random.NextDouble() * 10, random.NextDouble() * 10);
                points.Add(point);
            }
            var modelpts = new ModelPoints(points, BuiltInMaterials.ZAxis);
            Model.AddElement(modelpts);
            var ray = new Ray((0, 0, 0), new Vector3(1, 1, 1));
            var nearbyPoints = ray.NearbyPoints(points, 1);
            var rayAsLine = new Line((0, 0, 0), (10, 10, 10));
            Model.AddElement(rayAsLine);
            foreach (var p in nearbyPoints)
            {
                var distance = p.DistanceTo(rayAsLine, out var pt);
                var line = new Line(p, pt);
                var mc = new ModelCurve(line, BuiltInMaterials.XAxis);
                Model.AddElement(mc);
                Assert.True(distance < 1);
            }

        }

        private static Vector3 Center(Triangle t)
        {
            return new Vector3[] { t.Vertices[0].Position, t.Vertices[1].Position, t.Vertices[2].Position }.Average();
        }

        private static ModelCurve ModelCurveFromRay(Ray r)
        {
            var line = new Line(r.Origin, r.Origin + r.Direction * 20);
            return new ModelCurve(line);
        }

        private static ModelPoints ModelPointsFromIntersection(Vector3 xsect)
        {
            return new ModelPoints(new List<Vector3>() { xsect });
        }
    }
}