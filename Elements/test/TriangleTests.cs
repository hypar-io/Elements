using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elements.Tests;
using Newtonsoft.Json;
using Xunit;

namespace Elements.Geometry.Tests
{
    public class TriangleTests
    {
        [Fact]
        public void Equality()
        {
            var v0 = new Vertex(Vector3.Origin);
            var v1 = new Vertex(new Vector3(1, 1, 0));
            var v2 = new Vertex(new Vector3(0, 2, 0));
            var v2SmallShift = new Vertex(v2.Position + new Vector3(0, 0, 1E-6));
            var v2BiggerShift = new Vertex(v2.Position + new Vector3(0, 0, 1E-4));

            var triangle = new Triangle(new[] { v0, v1, v2 }, Vector3.ZAxis);
            var triangleRotated = new Triangle(new[] { v1, v2, v0 }, Vector3.ZAxis);

            var tASmallShift = new Triangle(new[] { v0, v1, v2BiggerShift }, Vector3.ZAxis);
            var tATinyShift = new Triangle(new[] { v0, v1, v2SmallShift }, Vector3.ZAxis);

            var comparer = new TriangleComparer(false);
            Assert.Equal(triangle, triangleRotated, comparer);
            Assert.NotEqual(triangle, tASmallShift, comparer);
            Assert.Equal(triangle, tATinyShift, comparer);

            var pickyComparer = new TriangleComparer(true, 1E-7);
            Assert.NotEqual(triangle, triangleRotated, pickyComparer);
            Assert.NotEqual(triangle, tASmallShift);
            Assert.NotEqual(triangle, tATinyShift);

            // Check that comparer can create identical hashcodes for vertices equidistant from origin.
            var v3 = new Vertex(new Vector3(-1, 1, 0));
            var triangleSymmetric = new Triangle(new[] { v0, v1, v3 }, Vector3.ZAxis);
            var triangleSymmetricFlipped = new Triangle(new[] { v0, v3, v1 }, Vector3.ZAxis);
            Assert.Equal(triangleSymmetric.Vertices[1].Position.DistanceTo(Vector3.Origin), triangleSymmetric.Vertices[2].Position.DistanceTo(Vector3.Origin));
            Assert.Equal(triangleSymmetricFlipped.Vertices[1].Position.DistanceTo(Vector3.Origin), triangleSymmetricFlipped.Vertices[2].Position.DistanceTo(Vector3.Origin));

            var h1 = comparer.GetHashCode(triangleSymmetric);
            var h2 = comparer.GetHashCode(triangleSymmetricFlipped);
            Assert.Equal(h1, h2);
        }
    }
}