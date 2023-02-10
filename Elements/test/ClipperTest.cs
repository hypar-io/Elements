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
        /// <summary>
        /// This test highlights properties of clipper:
        /// - It modifies input data due to integer nature, but not more than half of provided tolerance per coordinate.
        /// - Overall deviation is no more than tolerance.
        /// - Coordinate is ignored, result point will have z set to 0.
        /// If any of the properties are changed this test will react and fail.
        /// </summary>
        [Fact]
        public void ClipperChangesInputNumbers()
        {
            var range = Enumerable.Range(0, 100);
            var vertices = new List<Vector3>();

            //Use twice the tolerance to emphasize that number can be modified to up to half of it.
            var tolerance = Vector3.EPSILON * 2;
            Func<double, double> noise = (r) => { return (r + 1) / 100.0 * tolerance; };

            var z = 1.23456789;
            vertices.AddRange(range.SkipLast(1).Select(
                r => new Vector3(r + noise(r), 0, z)));
            vertices.AddRange(range.SkipLast(1).Select(
                r => new Vector3(100, r + noise(r), z)));
            vertices.AddRange(range.Reverse().SkipLast(1).Select(
                r => new Vector3(r + noise(r), 100, z)));
            vertices.AddRange(range.Reverse().SkipLast(1).Select(
                r => new Vector3(0, r + noise(r), z)));

            Polygon polygon = new Polygon(vertices);
            polygon = polygon.TransformedPolygon(new Transform().Rotated(Vector3.ZAxis, 25));

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
