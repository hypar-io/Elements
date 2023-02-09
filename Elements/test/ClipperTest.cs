using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Elements.Geometry;
using Xunit;

namespace Elements
{
    public class ClipperTest
    {
        [Fact]
        public void CliperChangesInputNumbers()
        {
            var range = Enumerable.Range(1, 100);
            var vertices = new List<Vector3>();
            var random = new Random();
            var z = 1.23456789;
            random.NextDouble();
            vertices.AddRange(range.Select(r => new Vector3(
                r + (random.NextDouble() - 0.5) * Vector3.EPSILON, 0, z)));
            vertices.AddRange(range.Select(r => new Vector3(
                100, r + (random.NextDouble() - 0.5) * Vector3.EPSILON, z)));
            vertices.AddRange(range.Reverse().Select(r => new Vector3(
                r + (random.NextDouble() - 0.5) * Vector3.EPSILON, 100, z)));
            vertices.AddRange(range.Reverse().Select(r => new Vector3(
                0, r + (random.NextDouble() - 0.5) * Vector3.EPSILON, z)));

            Polygon polygon = new Polygon(vertices);
            polygon = polygon.TransformedPolygon(new Transform().Rotated(Vector3.ZAxis, 25));

            var tolerance = Vector3.EPSILON * 2;
            var clipperPath = polygon.ToClipperPath(tolerance);
            var changedPolygon = clipperPath.ToPolygon(tolerance);
            Assert.Equal(polygon.Vertices.Count, changedPolygon.Vertices.Count);

            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                var dx = polygon.Vertices[i].X - changedPolygon.Vertices[i].X;
                var dy = polygon.Vertices[i].Y - changedPolygon.Vertices[i].Y;
                Assert.True(polygon.Vertices[i].X != changedPolygon.Vertices[i].X);
                Assert.True(polygon.Vertices[i].Y != changedPolygon.Vertices[i].Y);
                Assert.True(Math.Abs(dx) <= tolerance / 2);
                Assert.True(Math.Abs(dy) <= tolerance / 2);
                Assert.True(changedPolygon.Vertices[i].Z == 0d);

                var v2d = new Vector3(polygon.Vertices[i].X, polygon.Vertices[i].Y);
                var dp = v2d.DistanceTo(changedPolygon.Vertices[i]);
                Assert.True(dp < tolerance);
            }
        }
    }
}
