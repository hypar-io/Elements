using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elements.Tests;
using Newtonsoft.Json;
using Xunit;

namespace Elements.Geometry.Tests
{
    public class CircleTests : ModelTest
    {
        public CircleTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void CircleExample()
        {
            this.Name = "Elements_Geometry_Circle";

            // <example>
            var a = new Vector3();
            var b = 1.0;
            var c = new Circle(a, b);
            // </example>

            this.Model.AddElement(c);
        }

        [Fact]
        public void Equality()
        {
            var p = 1.0;
            var circleA = new Circle(Vector3.Origin, p);
            var circleB = new Circle(Vector3.Origin, p + 1E-4);
            var circleC = new Circle(Vector3.Origin, p + 1E-6);

            Assert.False(circleA.IsAlmostEqualTo(circleB));
            Assert.True(circleA.IsAlmostEqualTo(circleB, 1E-3));
            Assert.True(circleA.IsAlmostEqualTo(circleC));
        }

        [Fact]
        public void Construct()
        {
            var a = new Vector3();
            var b = 1.0;
            var c = new Circle(a, b);
            Assert.Equal(1.0, c.Radius);
            Assert.Equal(new Vector3(0, 0), c.Center);
            Assert.Equal(a + new Vector3(b, 0, 0), c.PointAt(-1e-10));
        }

        [Fact]
        public void ZeroRadius_ThrowsException()
        {
            var a = new Vector3();
            Assert.Throws<ArgumentException>(() => new Circle(a, 0));
        }

        [Fact]
        public void GetParameterAt()
        {
            var center = Vector3.Origin;
            var radius = 5.0;
            var circle = new Circle(center, radius);
            var start = new Vector3(5.0, 0.0, 0.0);
            var mid = new Vector3(-5.0, 0.0, 0.0);

            Assert.Equal(0, circle.GetParameterAt(start));

            var almostEqualStart = new Vector3(5.000001, 0.000005, 0);
            Assert.True(start.IsAlmostEqualTo(almostEqualStart));

            Assert.True(Math.Abs(circle.GetParameterAt(mid) - Math.PI) < double.Epsilon ? true : false);

            Assert.Equal(circle.Circumference, 2 * Math.PI * radius);
            var vector = new Vector3(3.535533, 3.535533, 0.0);
            var uValue = circle.GetParameterAt(vector);
            var expectedVector = circle.PointAt(uValue);
            Assert.InRange(uValue, 0, circle.Circumference);
            Assert.True(vector.IsAlmostEqualTo(expectedVector));

            var parameter = 0.5;
            var testParameterMidpoint = circle.PointAtNormalizedLength(parameter);
            Assert.True(testParameterMidpoint.IsAlmostEqualTo(mid));

            var midlength = circle.Circumference * parameter;
            var testLengthMidpoint = circle.PointAtLength(midlength);
            Assert.True(testLengthMidpoint.IsAlmostEqualTo(testParameterMidpoint));

            var midpoint = circle.MidPoint();
            Assert.True(midpoint.IsAlmostEqualTo(testLengthMidpoint));
        }

        [Fact]
        public void PointOnCircle()
        {
            Circle circle = new Circle(Vector3.Origin, 5.0);

            Assert.False(Circle.PointOnCircle(Vector3.Origin, circle));
            Assert.False(Circle.PointOnCircle(new Vector3(4, 0, 0), circle));
            Assert.True(Circle.PointOnCircle(circle.PointAtNormalizedLength(0.5), circle));
        }

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
