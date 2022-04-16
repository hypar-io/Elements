using Elements.Geometry;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

using System;

// https://tools.ietf.org/html/rfc7946#section-5
namespace Elements.GeoJSON
{
    /// <summary>
    /// The base class for all GeoJSON geometry types.
    /// </summary>
    public abstract class Geometry
    {
        /// <summary>
        /// The type of the geometry.
        /// </summary>
        [JsonProperty("type")]
        public virtual string Type
        {
            get { return GetType().Name; }
        }
    }

    /// <summary>
    /// A GeoJSON point.
    /// </summary>
    public class Point : Geometry
    {
        /// <summary>
        /// The coordinates of the geometry.
        /// </summary>
        [JsonProperty("coordinates")]
        public Position Coordinates { get; }

        /// <summary>
        /// Construct a Point.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <exception>Thrown when the provided coordinates are null.</exception>
        public Point(Position coordinates)
        {
            if (coordinates == null)
            {
                throw new ArgumentNullException("coordinates");
            }
            this.Coordinates = coordinates;
        }
    }

    /// <summary>
    /// A GeoJSON line.
    /// </summary>
    public class Line : Geometry
    {
        /// <summary>
        /// The coordinates of the geometry.
        /// </summary>
        [JsonProperty("coordinates")]
        public Position[] Coordinates { get; }

        /// <summary>
        /// Construct a Line.
        /// </summary>
        /// <param name="coordinates"></param>
        ///<exception>Thrown when the coordinates provides does not contain 2 items.</exception>
        public Line(Position[] coordinates)
        {
            if (coordinates == null || coordinates.Length != 2)
            {
                throw new ArgumentException("A line type must have exactly two points.");
            }
            this.Coordinates = coordinates;
        }
    }

    /// <summary>
    /// A GeoJSON multipoint.
    /// </summary>
    public class MultiPoint : Geometry
    {
        /// <summary>
        /// The coordinates of the geometry.
        /// </summary>
        [JsonProperty("coordinates")]
        public Position[] Coordinates { get; }

        /// <summary>
        /// Construct a MultiPoint.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <exception>Thrown when the coordinates provided contains 1 item or less.</exception>
        public MultiPoint(Position[] coordinates)
        {
            if (coordinates == null || coordinates.Length <= 1)
            {
                throw new ArgumentException("A multipoint type must have more than one point.");
            }
            this.Coordinates = coordinates;
        }
    }

    /// <summary>
    /// A GeoJSON linestring.
    /// </summary>
    public class LineString : Geometry
    {
        /// <summary>
        /// The coordinates of the geometry.
        /// </summary>
        [JsonProperty("coordinates")]
        public Position[] Coordinates { get; }

        /// <summary>
        /// Construct a LineString.
        /// </summary>
        /// <param name="coordinates"></param>
        public LineString(Position[] coordinates)
        {
            CheckLineString(coordinates);
            this.Coordinates = coordinates;
        }

        internal static void CheckLineString(Position[] coordinates)
        {
            if (coordinates.Length <= 2)
            {
                throw new Exception("A linestring must have more than two points.");
            }
        }
    }

    /// <summary>
    /// A GeoJSON multi line string.
    /// </summary>
    public class MultiLineString : Geometry
    {
        /// <summary>
        /// The coordinates of the geometry.
        /// </summary>
        [JsonProperty("coordinates")]
        public Position[][] Coordinates { get; }

        /// <summary>
        /// Construct a MultiLineString.
        /// </summary>
        /// <param name="coordinates"></param>
        public MultiLineString(Position[][] coordinates)
        {
            foreach (var lineString in coordinates)
            {
                LineString.CheckLineString(lineString);
            }
            this.Coordinates = coordinates;
        }
    }

    /// <summary>
    /// A GeoJSON polygon.
    /// </summary>
    public class Polygon : Geometry
    {
        /// <summary>
        /// The coordinates of the geometry.
        /// </summary>
        [JsonProperty("coordinates")]
        public Position[][] Coordinates { get; }

        /// <summary>
        /// Construct a Polygon.
        /// </summary>
        /// <param name="coordinates"></param>
        public Polygon(Position[][] coordinates)
        {
            foreach (var p in coordinates)
            {
                CheckPoly(p);
            }
            this.Coordinates = coordinates;
        }

        internal static void CheckPoly(Position[] coordinates)
        {
            if (coordinates.Length <= 2)
            {
                throw new Exception("A polygon must have more than two points.");
            }
            var a = coordinates[0];
            var b = coordinates[coordinates.Length - 1];
            if (a.Longitude != b.Longitude || a.Latitude != b.Latitude)
            {
                throw new Exception("The first and last points of the polygon must coincide.");
            }
        }

        /// <summary>
        /// Convert the coordinate array to a collection of polygons.
        /// The last position of the polygon is dropped.
        /// </summary>
        /// <returns></returns>
        public Elements.Geometry.Polygon[] ToPolygons(Position relativeToOrigin)
        {
            var plineArr = new Elements.Geometry.Polygon[Coordinates.Length];
            for (var i = 0; i < plineArr.Length; i++)
            {
                var coords = this.Coordinates[i];
                var verts = new Vector3[coords.Length - 1];
                // Drop the last position.
                for (var j = 0; j < coords.Length - 1; j++)
                {
                    verts[j] = coords[j].ToVectorMeters(relativeToOrigin);
                }
                var pline = new Elements.Geometry.Polygon(verts);
                plineArr[i] = pline;
            }
            return plineArr;
        }
    }

    /// <summary>
    /// A GeoJSON multi polygon.
    /// </summary>
    public class MultiPolygon : Geometry
    {
        /// <summary>
        /// The coordinates of the geometry.
        /// </summary>
        [JsonProperty("coordinates")]
        public Position[][] Coordinates { get; }

        /// <summary>
        /// Construct a MultiPolygon.
        /// </summary>
        /// <param name="coordinates"></param>
        public MultiPolygon(Position[][] coordinates)
        {
            foreach (var poly in coordinates)
            {
                Polygon.CheckPoly(poly);
            }
            this.Coordinates = coordinates;
        }
    }

    /// <summary>
    /// A GeoJSON geometry collection.
    /// </summary>
    public class GeometryCollection
    {
        [JsonProperty("geometries")]
        Geometry[] Geometries { get; }

        /// <summary>
        /// Construct a geometry collection.
        /// </summary>
        /// <param name="geometries">An array of geometries.</param>
        public GeometryCollection(Geometry[] geometries)
        {
            this.Geometries = geometries;
        }
    }

    class PositionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Position))
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
            return new Position(lat.Value, lon.Value);
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
            if (typeof(Geometry).IsAssignableFrom(objectType))
            {
                return true;
            }
            return false;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            string typeName = (jsonObject["type"]).ToString();
            switch (typeName)
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