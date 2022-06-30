
using System;
using System.Linq;
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
            Assert.Equal(p2.Latitude, p2ConvertedBack.Latitude, 3);
            Assert.Equal(p2.Longitude, p2ConvertedBack.Longitude, 3);

            var xy1 = new Vector3(50, 50, 0);
            var xy2 = new Vector3(100, 100, 0);
            var xy1pos = Position.FromVectorMeters(p1, xy1);
            var xy2pos = Position.FromVectorMeters(p1, xy2);
            var xy1ConvertedBack = xy1pos.ToVectorMeters(p1);
            var xy2ConvertedBack = xy2pos.ToVectorMeters(p1);
            // This tolerance seems large — we should investigate whether there
            // are more accurate projections / methods for these conversions.
            Assert.True(xy1.DistanceTo(xy1ConvertedBack) < 0.5);
            Assert.True(xy2.DistanceTo(xy2ConvertedBack) < 0.5);
        }
    }
}