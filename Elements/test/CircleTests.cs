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
        public void DivideIntoEqualSegments()
        {
            var l = new Line(Vector3.Origin, new Vector3(100, 0));
            var segments = l.DivideIntoEqualSegments(41);
            var len = l.Length();
            Assert.Equal(41, segments.Count);
            foreach (var s in segments)
            {
                Assert.Equal(s.Length(), len / 41, 5);
            }
        }

        [Fact]
        public void DivideIntoEqualSegmentsSingle()
        {
            var l = new Line(Vector3.Origin, new Vector3(100, 0));
            var segments = l.DivideIntoEqualSegments(1);
            Assert.Single(segments);
            Assert.True(segments.First().Start.IsAlmostEqualTo(l.Start, 1e-10));
            Assert.True(segments.First().End.IsAlmostEqualTo(l.End, 1e-10));
        }

        [Fact]
        public void DivideByLength()
        {
            var l = new Line(Vector3.Origin, new Vector3(5, 0));
            var segments = l.DivideByLength(1.1);
            Assert.Equal(6, segments.Count());

            var segments1 = l.DivideByLength(2);
            Assert.Equal(4, segments1.Count());
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
        public void PointOnLine()
        {
            Circle circle = new Circle(Vector3.Origin, 5.0);

            Assert.False(Circle.PointOnCircle(Vector3.Origin, circle));
            Assert.False(Circle.PointOnCircle(new Vector3(4, 0, 0), circle));
            Assert.True(Circle.PointOnCircle(circle.PointAtNormalizedLength(0.5), circle));
        }
    }
}
