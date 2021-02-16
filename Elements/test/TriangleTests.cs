using System;
using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class TriangleTests
    {
        [Fact]
        public void BadTriangleValidationTest()
        {
            var a = new Vertex(new Vector3(0, 0, 0));
            var b = new Vertex(new Vector3(1, 1, 1));

            Assert.Throws<ArgumentException>(() => new Triangle(a, b, b));
            Assert.Throws<ArgumentException>(() => new Triangle(new List<Vertex> { a, b, b }, Vector3.ZAxis));

            var c = new Vertex(new Vector3(1, 2, 1));
            var valid = new Triangle(a, b, c);
            var valid2 = new Triangle(new List<Vertex> { a, b, c }, Vector3.ZAxis);
        }
    }
}