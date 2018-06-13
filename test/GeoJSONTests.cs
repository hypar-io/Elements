using Xunit;
using Hypar.GeoJSON;
using Newtonsoft.Json;
using System;

namespace Hypar.Tests
{
    public class GeoJSONTests
    {   
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
            var p = new Polygon(new[]{a,b,c,a});
            var json = JsonConvert.SerializeObject(p);
            Console.WriteLine(json);

            Assert.Throws<Exception>(()=>{
                // End point coincidence test.
                var pFail = new Polygon(new[]{a,b,c});
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
            var f1 = new Feature(p);
            var f2 = new Feature(l);
            var fc = new FeatureCollection(new[]{f1,f2});
            var json = JsonConvert.SerializeObject(fc);
            Console.WriteLine(json);
        }
    }
}