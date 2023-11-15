
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements
{
    /// <summary>
    /// An alternate representation of an object.
    /// </summary>
    public class Symbol
    {
        /// <summary>
        /// The geometry of the symbol.
        /// </summary>
        [JsonProperty("Geometry", Required = Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public GeometryReference Geometry { get; set; }

        /// <summary>A named camera position for this representation, indicating the direction from which the camera is looking (a top view looks from top down, a north view looks from north to south.)</summary>
        [JsonProperty("CameraPosition", Required = Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public SymbolCameraPosition CameraPosition { get; set; }

        /// <summary>
        /// Construct a symbol.
        /// </summary>
        /// <param name="geometry">The geometry of the symbol.</param>
        /// <param name="cameraPosition">A named camera position for this representation.</param>
        [JsonConstructor]
        public Symbol(GeometryReference @geometry, SymbolCameraPosition @cameraPosition)
        {
            this.Geometry = @geometry;
            this.CameraPosition = @cameraPosition;
        }

        /// <summary>
        /// Get the geometry from this symbol.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<object>> GetGeometryAsync()
        {
            if (this.Geometry.InternalGeometry != null)
            {
                return this.Geometry.InternalGeometry;
            }
            else if (this.Geometry.GeometryUrl != null)
            {
                try
                {
                    var request = HttpWebRequest.Create(this.Geometry.GeometryUrl);
                    var response = await request.GetResponseAsync();
                    var stream = response.GetResponseStream();
                    var streamReader = new StreamReader(stream);
                    var json = streamReader.ReadToEnd();
                    var objects = JsonConvert.DeserializeObject<List<JObject>>(json);
                    var loadedTypes = typeof(Element).Assembly.GetTypes().ToList();
                    return objects.Select(obj =>
                    {
                        var discriminator = obj.Value<string>("discriminator");
                        var matchingType = loadedTypes.FirstOrDefault(t => t.FullName.Equals(discriminator));
                        return obj.ToObject(matchingType);
                    });
                }
                catch
                {
                    return new List<object>();
                }
            }
            return new List<object>();
        }
    }
}