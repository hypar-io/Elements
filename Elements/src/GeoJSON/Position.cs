using Elements.Geometry;
using Elements.Spatial;
using System.Text.Json.Serialization;

namespace Elements.GeoJSON
{
    /// <summary>
    /// A GeoJSON position specified by longitude and latitude.
    /// </summary>
    [JsonConverter(typeof(PositionConverter))]
    public class Position
    {
        /// <summary>The latitude in decimal degrees.</summary>
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        /// <summary>The longitude in decimal degrees.</summary>
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        /// <summary>
        /// Construct a position.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        [JsonConstructor]
        public Position(double @latitude, double @longitude)
        {
            this.Latitude = @latitude;
            this.Longitude = @longitude;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
            {
                return false;
            }
            var p = (Position)obj;
            return p.Longitude == Longitude && p.Latitude == Latitude;
        }

        /// <summary>
        /// 
        /// </summary>
        public override int GetHashCode()
        {
            return Longitude.GetHashCode() + ",".GetHashCode() + Latitude.GetHashCode();
        }

        /// <summary>
        /// Convert the position to a vector in meters relative to an origin position.
        /// </summary>
        /// <param name="relativeToOrigin">A position marking the latitude and longitude of (0,0)</param>
        public Vector3 ToVectorMeters(Position relativeToOrigin)
        {
            return MercatorProjection.LatLonToMeters(relativeToOrigin, Latitude, Longitude);
        }

        /// <summary>
        /// Convert the position to a vector in meters relative to an origin position.
        /// </summary>
        /// <param name="relativeToOrigin">A position marking the latitude and longitude of (0,0)</param>
        /// <param name="location">The position to convert to latitude and longitude.</param>
        public static Position FromVectorMeters(Position relativeToOrigin, Vector3 location)
        {
            return MercatorProjection.MetersToLatLon(relativeToOrigin, location);
        }
    }
}