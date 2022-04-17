using Elements.GeoJSON;
using System;
using Xunit;
using Line = Elements.GeoJSON.Line;
using System.Text.Json;

namespace Elements.Tests
{

    public class GeoJSONTests
    {
        private const string feature = @"
[
  {
    ""geometry"": {
      ""type"": ""Polygon"",
      ""coordinates"": [
        [
          [
            -118.40609282255173,
            34.005940499249576
          ],
          [
            -118.40582929551601,
            34.005800418852644
          ],
          [
            -118.40580448508263,
            34.00578763372555
          ],
          [
            -118.40591177344322,
            34.005647553076486
          ],
          [
            -118.40594664216042,
            34.00564310606795
          ],
          [
            -118.40619809925556,
            34.005805421727956
          ],
          [
            -118.40609282255173,
            34.005940499249576
          ]
        ]
      ]
    },
    ""type"": ""Feature"",
    ""properties"": {
      ""ain"": ""4210033009"",
      ""situsaddr"": ""11061 BARMAN AVE"",
      ""situscity"": ""CULVER CITY CA"",
      ""situszip_5"": ""90230""
    }
  }
]";

        [Fact]
        public void Position_Serialize_Valid()
        {
            var p = new Position(10.0, 5.0);
            var json = JsonSerializer.Serialize(p);
            var newP = JsonSerializer.Deserialize<Position>(json);
            Assert.Equal(5.0, newP.Longitude);
            Assert.Equal(10.0, newP.Latitude);
        }

        [Fact]
        public void Point_Serialize_Valid()
        {
            var p = new Point(new Position(10.0, 5.0));
            var json = JsonSerializer.Serialize(p);
            var newP = JsonSerializer.Deserialize<Point>(json);
            Assert.Equal("Point", newP.Type);
            Assert.Equal(new Position(10.0, 5.0), newP.Coordinates);
        }

        [Fact]
        public void Line_Serialize_Valid()
        {
            var a = new Position(0, 0);
            var b = new Position(5, 5);
            var l = new Line(new[] { a, b });
            var json = JsonSerializer.Serialize(l);
            var newL = JsonSerializer.Deserialize<Line>(json);
            Assert.Equal(a, newL.Coordinates[0]);
            Assert.Equal(b, newL.Coordinates[1]);
            Assert.Equal("Line", newL.Type);
        }

        [Fact]
        public void LineString_Serialize_Valid()
        {
            var a = new Position(0, 0);
            var b = new Position(5, 5);
            var c = new Position(10, 10);
            var ls = new LineString(new[] { a, b, c });
            var json = JsonSerializer.Serialize(ls);
            var newLs = JsonSerializer.Deserialize<LineString>(json);
            Assert.Equal(a, newLs.Coordinates[0]);
            Assert.Equal(b, newLs.Coordinates[1]);
            Assert.Equal(c, newLs.Coordinates[2]);
            Assert.Equal("LineString", newLs.Type);
        }

        [Fact]
        public void Polygon_Serialize_Valid()
        {
            var a = new Position(0, 0);
            var b = new Position(5, 5);
            var c = new Position(10, 10);
            var p = new Polygon(new[] { new[] { a, b, c, a } });
            var json = JsonSerializer.Serialize(p);

            Assert.Throws<Exception>(() =>
            {
                // End point coincidence test.
                var pFail = new Polygon(new[] { new[] { a, b, c } });
            });
        }

        [Fact]
        public void MultiLineString_Serialize_Valid()
        {
            var a = new Position(0, 0);
            var b = new Position(5, 5);
            var c = new Position(10, 10);
            var mls = new MultiLineString(new[] { new[] { a, b, c }, new[] { c, b, a } });
            var json = JsonSerializer.Serialize(mls);
            var newMls = JsonSerializer.Deserialize<MultiLineString>(json);
            Assert.Equal(2, newMls.Coordinates.GetLength(0));
            Assert.Equal(3, newMls.Coordinates[0].Length);
            Assert.Equal(3, newMls.Coordinates[1].Length);
        }

        [Fact]
        public void FeatureCollection_SerializeValid()
        {
            var p = new Point(new Position(0, 0));
            var l = new Line(new[] { new Position(0, 0), new Position(10, 10) });
            var f1 = new Feature(p, null);
            var f2 = new Feature(l, null);
            var fc = new FeatureCollection(new[] { f1, f2 });
            f1.Properties.Add("foo", "This is foo.");
            f2.Properties.Add("bar", 42);
            var json = JsonSerializer.Serialize(fc);
        }

        // [Fact]
        // public void LatLon_ToMeters_Valid()
        // {
        //     // Culver City
        //     // 34.006074, -118.405970

        //     //https://epsg.io/transform#s_srs=4326&t_srs=3857&x=-118.4059700&y=34.0060740
        //     // X -13180892.29 Y 4029617.65
        //     var lat = 34.006074;
        //     var lon = -118.405970;
        //     var utm = new Vector3(MercatorProjection.lonToX(lon), MercatorProjection.latToY(lat), 0);
        //     Assert.Equal(4029617.65, utm.Y, 2);
        //     Assert.Equal(-13180892.29, utm.X, 2);
        // }

        // [Fact]
        // public void Meters_ToLatLon_Valid()
        // {
        //     var x = 4029617.65;
        //     var y = -13180892.29;
        //     var latlon = WebMercator.MetersToLatLon(new Geometry.Vector3(x,y));
        //     Assert.Equal(34.006074, latlon.X);
        //     Assert.Equal(-118.405970, latlon.Y);
        // }

        [Fact]
        public void Feature_Deserialize_Valid()
        {
            var f = JsonSerializer.Deserialize<Feature[]>(feature);
            var p = (Polygon)f[0].Geometry;
            Assert.Equal(7, p.Coordinates[0].Length);
            Assert.Equal("Feature", f[0].Type);
            Assert.Equal(4, f[0].Properties.Count);
            Assert.Equal("11061 BARMAN AVE", f[0].Properties["situsaddr"]);
        }
    }
}