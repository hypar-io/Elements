using Elements;
using Elements.Geometry;
using Elements.Tests;
using System;
using System.Linq;
using System.Security.Cryptography;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Geometry.Tests
{
    public class CircleTests
    {
        [Fact]
        public void CirceIntersectsCircle()
        {
            // Planar intersecting circles 
            Circle c0 = new Circle(5);
            Circle c1 = new Circle(new Vector3(8, 0, 0), 5);
            Assert.True(c0.Intersects(c1, out var results));
            Assert.Equal(2, results.Count());
            Assert.Contains(new Vector3(4, 3, 0), results);
            Assert.Contains(new Vector3(4, -3, 0), results);
            c1 = new Circle(new Vector3(3, 0, 0), 5);
            Assert.True(c0.Intersects(c1, out results));
            Assert.Equal(2, results.Count());
            Assert.Contains(new Vector3(1.5, 4.769696, 0), results);
            Assert.Contains(new Vector3(1.5, -4.769696, 0), results);

            // Planar intersecting circles with opposite normals
            c1 = new Circle(new Transform(new Vector3(8, 0, 0), Vector3.ZAxis.Negate()), 5);
            Assert.True(c0.Intersects(c1, out results));
            Assert.Equal(2, results.Count());
            Assert.Contains(new Vector3(4, 3, 0), results);
            Assert.Contains(new Vector3(4, -3, 0), results);

            // Planar touching circles
            c1 = new Circle(new Vector3(8, 0, 0), 3);
            Assert.True(c0.Intersects(c1, out results));
            Assert.Single(results);
            Assert.Contains(new Vector3(5, 0, 0), results);
            c1 = new Circle(new Vector3(-3, 0, 0), 2);
            Assert.True(c0.Intersects(c1, out results));
            Assert.Single(results);
            Assert.Contains(new Vector3(-5, 0, 0), results);

            // Planar one circle inside other
            c1 = new Circle(new Vector3(1, 0, 0), 3);
            Assert.False(c0.Intersects(c1, out _));

            // Planar non-intersecting circles
            c1 = new Circle(new Vector3(10, 0, 0), 3);
            Assert.False(c0.Intersects(c1, out _));

            // Non-planar intersecting circles
            var t = new Transform(new Vector3(4, 0, 4), Vector3.XAxis);
            c1 = new Circle(t, 5);
            Assert.True(c0.Intersects(c1, out results));
            Assert.Equal(2, results.Count());
            Assert.Contains(new Vector3(4, 3, 0), results);
            Assert.Contains(new Vector3(4, -3, 0), results);

            // Non-planar touching circles
            t = new Transform(new Vector3(0, 7, 2), new Vector3(0, 1, -1).Unitized());
            c1 = new Circle(t, Math.Sqrt(2) * 2);
            Assert.True(c0.Intersects(c1, out results));
            Assert.Single(results);
            Assert.Contains(new Vector3(0, 5, 0), results);

            // Non-planar non-intersecting circles
            t = new Transform(new Vector3(0, 7, 2), new Vector3(0, -1, -1).Unitized());
            c1 = new Circle(t, 1);
            Assert.False(c0.Intersects(c1, out _));

            // Same normal different origins
            c1 = new Circle(new Vector3(0, 0, 10), 5);
            Assert.False(c0.Intersects(c1, out _));

            // Same circle
            c1 = new Circle(5);
            Assert.False(c0.Intersects(c1, out _));
        }

        [Fact]
        public void CircleIntersectsLine()
        {
            var circleDir = new Vector3(1, 0, 1);
            var t = new Transform(new Vector3(1, 1, 1), circleDir);
            var circle = new Circle(t, 2);

            // Planar intersecting
            var line = new InfiniteLine(new Vector3(2, 0, 0), Vector3.YAxis);
            Assert.True(circle.Intersects(line, out var results));
            Assert.Equal(2, results.Count());
            Assert.Contains(new Vector3(2, 1 + Math.Sqrt(2), 0), results);
            Assert.Contains(new Vector3(2, 1 - Math.Sqrt(2), 0), results);

            // Planar touching
            var dir = circleDir.Cross(Vector3.YAxis);
            line = new InfiniteLine(new Vector3(2, -1, 0), dir);
            Assert.True(circle.Intersects(line, out results));
            Assert.Single(results);
            Assert.Contains(new Vector3(1, -1, 1), results);

            // Planar non-intersecting
            line = new InfiniteLine(new Vector3(3, 0, 0), dir);
            Assert.False(circle.Intersects(line, out _));

            // Non-planar touching
            line = new InfiniteLine(new Vector3(1 + Math.Sqrt(2), 1, 3), Vector3.ZAxis);
            Assert.True(circle.Intersects(line, out results));
            Assert.Single(results);
            Assert.Contains(new Vector3(1 + Math.Sqrt(2), 1, 1 - Math.Sqrt(2)), results);

            // Non-planar non-intersecting
            line = new InfiniteLine(new Vector3(1, 1, 1), Vector3.ZAxis);
            Assert.False(circle.Intersects(line, out _));
        }

        [Fact]
        public void AdjustRadian()
        {
            double reference = Units.DegreesToRadians(45);
            Assert.Equal(Units.DegreesToRadians(385), Units.AdjustRadian(Units.DegreesToRadians(25), reference), 6);
            Assert.Equal(Units.DegreesToRadians(60), Units.AdjustRadian(Units.DegreesToRadians(420), reference), 6);
            Assert.Equal(Units.DegreesToRadians(260), Units.AdjustRadian(Units.DegreesToRadians(-100), reference), 6);

            reference = Units.DegreesToRadians(400);
            Assert.Equal(Units.DegreesToRadians(745), Units.AdjustRadian(Units.DegreesToRadians(25), reference), 6);
            Assert.Equal(Units.DegreesToRadians(400), Units.AdjustRadian(Units.DegreesToRadians(400), reference), 6);
            Assert.Equal(Units.DegreesToRadians(620), Units.AdjustRadian(Units.DegreesToRadians(-100), reference), 6);

            reference = Units.DegreesToRadians(-160);
            Assert.Equal(Units.DegreesToRadians(25), Units.AdjustRadian(Units.DegreesToRadians(25), reference), 6);
            Assert.Equal(Units.DegreesToRadians(60), Units.AdjustRadian(Units.DegreesToRadians(420), reference), 6);
            Assert.Equal(Units.DegreesToRadians(-100), Units.AdjustRadian(Units.DegreesToRadians(-100), reference), 6);
        }
    }
}
