
using System;
using Elements.GeoJSON;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class MercatorProjectionTests
    {
        [Fact]
        public void DistancesAreValid()
        {
            var p1 = new Position(40.703895532669854, -73.98860211355571);
            var p2 = new Position(40.70974164658192, -73.9918450388025);
            var xy = p2.ToVectorMeters(p1);
            var distance = xy.Length();
            // values measured by hand on google maps
            Assert.True(Math.Abs(712 - distance) < 10);
        }

        [Fact]
        public void CoordinatesConvertBothWays()
        {
            var p1 = new Position(40.703895532669854, -73.98860211355571);
            var p2 = new Position(40.70974164658192, -73.9918450388025);
            var xy = p2.ToVectorMeters(p1);
            var p2ConvertedBack = Position.FromVectorMeters(p1, xy);
            Assert.Equal(p2.Latitude, p2ConvertedBack.Latitude, 2);
            Assert.Equal(p2.Longitude, p2ConvertedBack.Longitude, 2);
        }
    }
}