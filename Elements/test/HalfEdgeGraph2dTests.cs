using System;
using Elements.Geometry;
using Xunit;
using System.Linq;
using System.Collections.Generic;
using Elements.Spatial;
using Elements.Serialization.JSON;
using System.IO;
using Newtonsoft.Json;

namespace Elements.Tests
{
    public class HalfEdgeGraph2dTests : ModelTest
    {
        // see also: Profile.Split (which calls into the HalfEdgeGraph2d API and has many of its own tests)

        [Fact]
        public void HalfEdgeGraphFromLines()
        {
            Name = nameof(HalfEdgeGraphFromLines);
            // not in order, but clockwise-wound
            var lines = new List<Line> {
                new Line((0,0), (10,0)),
                new Line((10,10), (5,15)),
                new Line((10,0), (10,10)),
                new Line((0,10), (0,0)),
                new Line((5,15), (0,10)),
            };
            var heg = HalfEdgeGraph2d.Construct(lines);
            var polygons = heg.Polygonize();
            Assert.Single(polygons);
            Model.AddElement(polygons[0]);
        }

        [Fact]
        public void HalfEdgeGraphFromLinesBothWays()
        {
            Name = nameof(HalfEdgeGraphFromLinesBothWays);
            // not in order and inconsistently wound
            var lines = new List<Line> {
                new Line((0,0), (10,0)),
                new Line((10,10), (5,15)),
                new Line((10,0), (10,10)),
                new Line((0,0), (0,10)),
                new Line((0,10), (5,15)),
            };
            var heg = HalfEdgeGraph2d.Construct(lines, true);
            var polygons = heg.Polygonize();
            Assert.Equal(2, polygons.Count);
            Assert.Single(polygons.Where(p => p.Normal().Dot(Vector3.ZAxis) > 0));
        }

        [Fact]
        public void HalfEdgeGraphUsingPolylinize()
        {
            Name = nameof(HalfEdgeGraphUsingPolylinize);
            // not in order and inconsistently wound
            var lines = new List<Line> {
                new Line((0,0), (10,0)),
                new Line((10,0), (4,4)),
                new Line((10,0), (10,10)),
            };
            var heg = HalfEdgeGraph2d.Construct(lines, true);
            var polygons = heg.Polylinize();
            Assert.Single(polygons);
            Model.AddElement(polygons[0]);
        }
    }
}