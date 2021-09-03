using Elements.Spatial;

namespace Elements
{
    /// <summary>Just a test</summary>
    [Newtonsoft.Json.JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public partial class Grid2dElement : Element
    {
        /// <summary>contains a grid</summary>
        [Newtonsoft.Json.JsonProperty("Grid", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Grid2d Grid { get; set; }

        [Newtonsoft.Json.JsonConstructor]
        public Grid2dElement(Grid2d @grid, System.Guid @id, string @name)
            : base(id, name)
        {
            this.Grid = @grid;
        }
    }
}