using System;
using System.Collections.Generic;
using Elements.Analysis;
using Elements.Geometry;
using Xunit;



namespace Elements.Tests
{
    public class ConvexHullTests : ModelTest
    {
        [Fact]
        public void ConvexHullOrthogonalWithDuplicateVertices()
        {
            var points = new List<Vector3>()
            {
                new Vector3(51.8160, 6.0960, 45.7200),
                new Vector3(0.0000, 6.0960, 45.7200),
                new Vector3(0.0000, 24.3840, 45.7200),
                new Vector3(51.8160, 24.3840, 45.7200),
                new Vector3(51.8160, 24.3840, 45.7200),
                new Vector3(51.8160, 6.0960, 45.7200)
            };
            var hull = ConvexHull.FromPoints(points);
            Assert.Equal(4, hull.Segments().Length);
        }
    }
}