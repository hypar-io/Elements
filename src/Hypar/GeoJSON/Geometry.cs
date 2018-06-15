using Hypar.Geometry;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// https://tools.ietf.org/html/rfc7946#section-5
/// </summary>
namespace Hypar.GeoJSON
{
    public abstract class Geometry
    {
        [JsonProperty("type")]
        public virtual string Type
        {
            get{return GetType().Name;}
        }
    }

    [JsonConverter(typeof(PositionConverter))]
    public class Position
    {
        public double Latitude {get;}
        public double Longitude{get;}
        public Position(double lon, double lat)
        {
            this.Latitude = lat;
            this.Longitude = lon;
        }

        public override bool Equals(object obj)
        {
            if(obj.GetType() != GetType())
            {
                return false;
            }
            var p = (Position)obj;
            return p.Longitude == Longitude && p.Latitude == Latitude;
        }

        public override int GetHashCode()
        {
            return Longitude.GetHashCode() + ",".GetHashCode() + Latitude.GetHashCode();
        }

        public Vector3 ToVectorMeters()
        {
            return new Vector3(MercatorProjection.lonToX(Longitude), MercatorProjection.latToY(Latitude));
        }
    }

    public class Point : Geometry
    {
        [JsonProperty("coordinates")]
        public Position Coordinates{get;}

        public Point(Position coordinates)
        {
            if(coordinates == null)
            {
                throw new ArgumentNullException("coordinates");
            }
            this.Coordinates = coordinates;
        }
    }

    public class Line : Geometry
    {
        [JsonProperty("coordinates")]
        public Position[] Coordinates{get;}

        public Line(Position[] coordinates)
        {
            if(coordinates == null || coordinates.Length != 2)
            {
                throw new Exception("A line type must have exactly two points.");
            }
            this.Coordinates = coordinates;
        }
    }

    public class MultiPoint : Geometry
    {
        [JsonProperty("coordinates")]
        public Position[] Coordinates{get;}

        public MultiPoint(Position[] coordinates)
        {
            if(coordinates == null || coordinates.Length <= 1)
            {
                throw new Exception("A multipoint type must have more than one point.");
            }
            this.Coordinates = coordinates;
        }
    }

    public class LineString : Geometry
    {
        [JsonProperty("coordinates")]
        public Position[] Coordinates{get;}

        public LineString(Position[] coordinates)
        {
            CheckLineString(coordinates);
            this.Coordinates = coordinates;
        }

        internal static void CheckLineString(Position[] coordinates)
        {
            if(coordinates.Length <= 2)
            {
                throw new Exception("A linestring must have more than two points.");
            }
        }
    }

    public class MultiLineString : Geometry
    {
        [JsonProperty("coordinates")]
        public Position[][] Coordinates{get;}

        public MultiLineString(Position[][] coordinates)
        {
            foreach(var lineString in coordinates)
            {
                LineString.CheckLineString(lineString);
            }
            this.Coordinates = coordinates;
        }
    }

    public class Polygon : Geometry
    {
        [JsonProperty("coordinates")]
        public Position[][] Coordinates{get;}

        public Polygon(Position[][] coordinates)
        {
            foreach(var p in coordinates)
            {
                CheckPoly(p);
            }
            this.Coordinates = coordinates;
        }

        internal static void CheckPoly(Position[] coordinates)
        {
            if(coordinates.Length <= 2)
            {
                throw new Exception("A polygon must have more than two points.");
            }
            var a = coordinates[0];
            var b = coordinates[coordinates.Length-1];
            if(a.Longitude != b.Longitude || a.Latitude != b.Latitude)
            {
                throw new Exception("The first and last points of the polygon must coincide.");
            }
        }

        /// <summary>
        /// Convert the coordinate array to a collection of polylines.
        /// The last position of the polygon is dropped.
        /// </summary>
        /// <returns></returns>
        public Polyline[] ToPolylines()
        {
            var plineArr = new Polyline[Coordinates.Length];
            for(var i=0; i<plineArr.Length; i++)
            {
                var coords = this.Coordinates[i];
                var verts = new List<Vector3>();
                // Drop the last position.
                for(var j=0; j<coords.Length-1; j++)
                {
                    verts.Add(coords[j].ToVectorMeters());
                }
                var pline = new Polyline(verts);
                plineArr[i] = pline;
            }
            return plineArr;
        }
    }

    public class MultiPolygon : Geometry
    {
        [JsonProperty("coordinates")]
        public Position[][] Coordinates{get;}
        public MultiPolygon(Position[][] coordinates)
        {
            foreach(var poly in coordinates)
            {
                Polygon.CheckPoly(poly);
            }
            this.Coordinates = coordinates;
        }
    }

    public class GeometryCollection
    {
        Geometry[] Geometries{get;}

        public GeometryCollection(Geometry[] geometries)
        {
            this.Geometries = geometries;
        }
    }

    class PositionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if(objectType == typeof(Position))
            {
                return true;
            }
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var lon = reader.ReadAsDouble();
            var lat = reader.ReadAsDouble();
            reader.Read();
            return new Position(lon.Value,lat.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var p = (Position)value;
            writer.WriteStartArray();
            writer.WriteValue(p.Longitude);
            writer.WriteValue(p.Latitude);
            writer.WriteEndArray();
        }
    }

    
    class GeometryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if(typeof(Geometry).IsAssignableFrom(objectType))
            {
                return true;
            }
            return false;
        }

        public override bool CanWrite
        {
            get{return false;}
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            string typeName = (jsonObject["type"]).ToString();
            switch(typeName)
            {
                case "Point":
                    return jsonObject.ToObject<Point>();
                case "Line":
                    return jsonObject.ToObject<Line>();
                case "MultiPoint":
                    return jsonObject.ToObject<MultiPoint>();
                case "LineString":
                    return jsonObject.ToObject<LineString>();
                case "MultiLineString":
                    return jsonObject.ToObject<MultiLineString>();
                case "Polygon":
                    Console.WriteLine("Deserializing polygon.");
                    return jsonObject.ToObject<Polygon>();
                case "MultiPolygon":
                    return jsonObject.ToObject<MultiPolygon>();
                case "GeometryCollection":
                    return jsonObject.ToObject<GeometryCollection>();
                default:
                    throw new Exception($"The type found in the GeoJSON, {typeName}, could not be resolved.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}