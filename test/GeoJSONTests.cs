using Xunit;
using Hypar.GeoJSON;
using Newtonsoft.Json;
using System;

namespace Hypar.Tests
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
            var json = JsonConvert.SerializeObject(p);
            Console.WriteLine(json);
            var newP = JsonConvert.DeserializeObject<Position>(json);
            Assert.Equal(10.0, newP.Longitude);
            Assert.Equal(5.0, newP.Latitude);
        }

        [Fact]
        public void Point_Serialize_Valid()
        {
            var p = new Point(new Position(10.0,5.0));
            var json = JsonConvert.SerializeObject(p);
            Console.WriteLine(json);
            var newP = JsonConvert.DeserializeObject<Point>(json);
            Assert.Equal("Point", newP.Type);
            Assert.Equal(new Position(10.0,5.0), newP.Coordinates);
        }

        [Fact]
        public void Line_Serialize_Valid()
        {
            var a = new Position(0,0);
            var b = new Position(5,5);
            var l = new Line(new[]{a,b});
            var json = JsonConvert.SerializeObject(l);
            Console.WriteLine(json);
            var newL = JsonConvert.DeserializeObject<Line>(json);
            Assert.Equal(a, newL.Coordinates[0]);
            Assert.Equal(b, newL.Coordinates[1]);
            Assert.Equal("Line", newL.Type);
        }

        [Fact]
        public void LineString_Serialize_Valid()
        {
            var a = new Position(0,0);
            var b = new Position(5,5);
            var c = new Position(10,10);
            var ls = new LineString(new[]{a,b,c});
            var json = JsonConvert.SerializeObject(ls);
            Console.WriteLine(json);
            var newLs = JsonConvert.DeserializeObject<LineString>(json);
            Assert.Equal(a, newLs.Coordinates[0]);
            Assert.Equal(b, newLs.Coordinates[1]);
            Assert.Equal(c, newLs.Coordinates[2]);
            Assert.Equal("LineString", newLs.Type);
        }

        [Fact]
        public void Polygon_Serialize_Valid()
        {
            var a = new Position(0,0);
            var b = new Position(5,5);
            var c = new Position(10,10);
            var p = new Polygon(new[]{new[]{a,b,c,a}});
            var json = JsonConvert.SerializeObject(p);
            Console.WriteLine(json);

            Assert.Throws<Exception>(()=>{
                // End point coincidence test.
                var pFail = new Polygon(new[]{new[]{a,b,c}});
            });
        }

        [Fact]
        public void MultiLineString_Serialize_Valid()
        {
            var a = new Position(0,0);
            var b = new Position(5,5);
            var c = new Position(10,10);
            var mls = new MultiLineString(new[]{new[]{a,b,c},new[]{c,b,a}});
            var json = JsonConvert.SerializeObject(mls);
            Console.WriteLine(json);
            var newMls = JsonConvert.DeserializeObject<MultiLineString>(json);
            Assert.Equal(2, newMls.Coordinates.GetLength(0));
            Assert.Equal(3, newMls.Coordinates[0].Length);
            Assert.Equal(3, newMls.Coordinates[1].Length);
        }

        [Fact]
        public void FeatureCollection_SerializeValid()
        {
            var p = new Point(new Position(0,0));
            var l = new Line(new []{new Position(0,0), new Position(10,10)});
            var f1 = new Feature(p,null);
            var f2 = new Feature(l,null);
            var fc = new FeatureCollection(new[]{f1,f2});
            var json = JsonConvert.SerializeObject(fc);
            Console.WriteLine(json);
        }

        [Fact]
        public void LatLon_ToMeters_Valid()
        {
            // Culver City
            // 34.006074, -118.405970

            //https://epsg.io/transform#s_srs=4326&t_srs=3857&x=-118.4059700&y=34.0060740
            // X -13180892.29 Y 4029617.65
            var lat = 34.006074;
            var lon = -118.405970;
            var utm = WebMercator.LatLonToMeters(lat, lon);
            Assert.Equal(utm.Y, 4029617.65, 2);
            Assert.Equal(utm.X, -13180892.29, 2);
        }

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
            var f = JsonConvert.DeserializeObject<Feature[]>(feature);
            var p = (Polygon)f[0].Geometry;
            Assert.Equal(7, p.Coordinates[0].Length);
            Assert.Equal("Feature", f[0].Type);
            Assert.Equal(4, f[0].Properties.Count);
            Assert.Equal("11061 BARMAN AVE", f[0].Properties["situsaddr"]);
        }
    }
}