using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hypar.Tests
{
    public class EllipseTests
    {
        [Fact]
        public void EllipseIntersectsLine()
        {
            var origin = new Vector3(0, 0, 2);
            Transform t = new Transform(new Vector3(0, 0, 2), new Vector3(-1, 0, 1));
            var ellipse = new Ellipse(t, 4, 2);
            var majorOffset =  ellipse.Transform.XAxis * ellipse.MajorAxis;
            var minorOffset = ellipse.Transform.YAxis * ellipse.MinorAxis;

            // Planar intersecting
            var line = new InfiniteLine(origin, ellipse.Transform.XAxis);
            Assert.True(ellipse.Intersects(line, out var results));
            Assert.Equal(2, results.Count());
            Assert.Contains(origin + majorOffset, results);
            Assert.Contains(origin - majorOffset, results);

            // Planar touching
            var p = origin + majorOffset + minorOffset;
            line = new InfiniteLine(p, ellipse.Transform.XAxis);
            Assert.True(ellipse.Intersects(line, out results));
            Assert.Single(results);
            Assert.Contains(origin + minorOffset, results);

            // Planar non-intersecting
            p = origin + minorOffset * 2;
            line = new InfiniteLine(p, ellipse.Transform.XAxis);
            Assert.False(ellipse.Intersects(line, out _));

            // Non-planar touching
            p = ellipse.PointAt(Math.PI - Math.PI / 4);
            line = new InfiniteLine(new Vector3(p.X, p.Y, 0), Vector3.ZAxis);
            Assert.True(ellipse.Intersects(line, out results));
            Assert.Single(results);
            Assert.Contains(p, results);

            // Non-planar non-intersecting
            line = new InfiniteLine(origin, Vector3.ZAxis);
            Assert.False(ellipse.Intersects(line, out _));
        }

        [Fact]
        public void EllipseIntersectsCircle()
        {
            // Planar intersection 4 intersections.
            Ellipse ellipse = new Ellipse(new Vector3(1, 1), 2, 4);
            Circle circle = new Circle(new Vector3(1, 1, 0), 3);
            Assert.True(ellipse.Intersects(circle, out var results));
            Assert.Equal(4, results.Count());

            // Planar intersection 3 intersections.
            ellipse = new Ellipse(new Vector3(1, 1), 2, 6);
            circle = new Circle(new Vector3(-2, 1, 0), 5);
            Assert.True(ellipse.Intersects(circle, out results));
            Assert.Equal(3, results.Count());

            // Planar intersection 2 intersections.
            ellipse = new Ellipse(new Vector3(1, 1), 2, 6);
            circle = new Circle(new Vector3(-2, 1, 0), 3);
            Assert.True(ellipse.Intersects(circle, out results));
            Assert.Equal(2, results.Count());

            // Planar intersection 1 intersection.
            ellipse = new Ellipse(new Vector3(1, 1), 2, 6);
            circle = new Circle(new Vector3(0, 1, 0), 1);
            Assert.True(ellipse.Intersects(circle, out results));
            Assert.Single(results);

            // Planar no intersection
            ellipse = new Ellipse(new Vector3(1, 1), 2, 6);
            circle = new Circle(new Vector3(0, 1, 0), 0.5);
            Assert.False(ellipse.Intersects(circle, out results));

            // Overlapping 
            ellipse = new Ellipse(new Vector3(1, 1), 2, 2);
            circle = new Circle(new Vector3(1, 1), 2);
            Assert.False(ellipse.Intersects(circle, out results));

            // Non-planar intersection 2 intersections.
            Transform t = new Transform(new Vector3(2, 0, 2), Vector3.ZAxis, Vector3.XAxis);
            ellipse = new Ellipse(t, 4, 2);
            circle = new Circle(new Vector3(2, 0, 2), 2);
            Assert.True(ellipse.Intersects(circle, out results));
            Assert.Equal(2, results.Count());

            // Non-planar intersection 1 intersection.
            ellipse = new Ellipse(t, 2, 4);
            circle = new Circle(new Vector3(0, 0, 0), 2);
            Assert.True(ellipse.Intersects(circle, out results));
            Assert.Single(results);

            // Non-planar no intersection
            ellipse = new Ellipse(t, 4, 2);
            circle = new Circle(new Vector3(0, 0, 0), 2);
            Assert.False(ellipse.Intersects(circle, out results));
        }

        [Fact]
        public void EllipseIntersectsEllipse()
        {
            // Planar intersection 4 intersections.
            Ellipse ellipse = new Ellipse(new Vector3(1, 1), 2, 4);
            Ellipse other = new Ellipse(new Vector3(1, 1), 4, 2);
            Assert.True(ellipse.Intersects(other, out var results));
            Assert.Equal(4, results.Count());

            // Planar intersection 3 intersections.
            ellipse = new Ellipse(new Vector3(1, 1), 2, 6);
            other = new Ellipse(new Vector3(-2, 1, 0), 5, 2);
            Assert.True(ellipse.Intersects(other, out results));
            Assert.Equal(3, results.Count());

            // Planar intersection 2 intersections.
            ellipse = new Ellipse(new Vector3(1, 1), 2, 6);
            other = new Ellipse(new Vector3(-2, 1, 0), 4, 3);
            Assert.True(ellipse.Intersects(other, out results));
            Assert.Equal(2, results.Count());

            // Planar intersection 1 intersection.
            ellipse = new Ellipse(new Vector3(1, 1), 2, 6);
            other = new Ellipse(new Vector3(1, 8), 2, 1);
            Assert.True(ellipse.Intersects(other, out results));
            Assert.Single(results);

            // Planar no intersection
            ellipse = new Ellipse(new Vector3(1, 1), 2, 6);
            other = new Ellipse(new Vector3(0, 1, 0), 1.5, 0.5);
            Assert.False(ellipse.Intersects(other, out results));

            // Overlapping 
            ellipse = new Ellipse(new Vector3(1, 1), 2, 4);
            Transform t = new Transform(new Vector3(1, 1), Vector3.YAxis, Vector3.ZAxis);
            other = new Ellipse(t, 4, 2);
            Assert.False(ellipse.Intersects(other, out results));

            // Non-planar intersection 2 intersections.
            t = new Transform(new Vector3(2, 0, 2), Vector3.ZAxis, Vector3.XAxis);
            ellipse = new Ellipse(t, 4, 2);
            other = new Ellipse(new Vector3(2, 0, 2), 3, 2);
            Assert.True(ellipse.Intersects(other, out results));
            Assert.Equal(2, results.Count());

            // Non-planar intersection 1 intersection.
            ellipse = new Ellipse(t, 2, 4);
            other = new Ellipse(new Vector3(0, 0, 0), 2, 3);
            Assert.True(ellipse.Intersects(other, out results));
            Assert.Single(results);

            // Non-planar no intersection
            ellipse = new Ellipse(t, 4, 2);
            other = new Ellipse(new Vector3(0, 0, 0), 3, 2);
            Assert.False(ellipse.Intersects(other, out results));
        }
    }
}
