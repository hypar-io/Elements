using Elements.Spatial;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>Just a test</summary>
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public partial class Grid2dElement : Element
    {
        /// <summary>contains a grid</summary>
        [JsonProperty("Grid", Required = Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Grid2d Grid { get; set; }

        [JsonConstructor]
        public Grid2dElement(Grid2d @grid, System.Guid @id, string @name)
            : base(id, name)
        {
            this.Grid = @grid;
        }
    }
}