using Elements.Geometry;
using Elements.Spatial;
using Newtonsoft.Json;

namespace Elements.GeoJSON
{
    /// <summary>
    /// A GeoJSON position specified by longitude and latitude.
    /// </summary>
    [JsonConverter(typeof(PositionConverter))]
    public class Position
    {
        /// <summary>The latitude in decimal degrees.</summary>
        [Newtonsoft.Json.JsonProperty("latitude", Required = Newtonsoft.Json.Required.Always)]
        public double Latitude { get; set; }

        /// <summary>The longitude in decimal degrees.</summary>
        [Newtonsoft.Json.JsonProperty("longitude", Required = Newtonsoft.Json.Required.Always)]
        public double Longitude { get; set; }

        /// <summary>
        /// Construct a position.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        [Newtonsoft.Json.JsonConstructor]
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
        /// Convert the position to a vector.
        /// </summary>
        public Vector3 ToVectorMeters()
        {
            return MercatorProjection.LatLonToVector3(Latitude, Longitude);
        }
    }
}