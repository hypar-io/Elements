using Elements;
using Elements.Geometry;
using Elements.Tests;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Geometry.Tests
{
    public class InfiniteLineTests
    {
        [Fact]
        public void LineIntersectsLine()
        {
            // Parallel lines
            var l1 = new InfiniteLine(new Vector3(0, 0), Vector3.XAxis);
            var l2 = new InfiniteLine(new Vector3(0, 0, 5), Vector3.XAxis);
            Assert.False(l1.Intersects(l2, out _));

            // Not coplanar
            l2 = new InfiniteLine(new Vector3(0, 5), Vector3.ZAxis);
            Assert.False(l1.Intersects(l2, out _));

            // Same line
            l2 = new InfiniteLine(new Vector3(10, 0), Vector3.XAxis);
            Assert.False(l1.Intersects(l2, out _));

            // Intersecting lines
            l2 = new InfiniteLine(new Vector3(10, 5, 5), new Vector3(-1, -1, -1));
            Assert.True(l1.Intersects(l2, out var intersections));
            Assert.Single(intersections);
            Assert.True(intersections.First().Equals(new Vector3(5, 0)));
        }

        [Fact]
        public void LineIntersectsPlane()
        {
            var l = new InfiniteLine(new Vector3(5, 0, 5), new Vector3(1, 1, 1));
            var p = new Plane(Vector3.Origin, Vector3.ZAxis);
            Assert.True(l.Intersects(p, out var intersection));
            Assert.Equal(new Vector3(0, -5, 0), intersection);

            l = new InfiniteLine(Vector3.Origin, Vector3.XAxis);
            Assert.False(l.Intersects(p, out _));
        }
    }
}
