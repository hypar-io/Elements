using Elements.Geometry;
using Elements.Geometry.Solids;
using System.Collections.Generic;
using Polygon = Elements.Geometry.Polygon;

namespace Elements
{
    /// <summary>A list of elements that are grouped together.</summary>
    public partial class MergeGroup : GeometricElement
    {
        [Newtonsoft.Json.JsonConstructor]
        public MergeGroup(IList<string> @elementIds, Polygon @boundary, Transform @transform = null, Material @material = null, Representation @representation = null, bool @isElementDefinition = false, System.Guid @id = default, string @name = null)
            : base(transform, material, representation, isElementDefinition, id, name)
        {
            this.ElementIds = @elementIds;
            this.Boundary = @boundary;
        }

        /// <summary>The guids of the elements that are in this group.</summary>
        [Newtonsoft.Json.JsonProperty("ElementIds", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<string> ElementIds { get; set; }

        /// <summary>The boundary of the space.</summary>
        [Newtonsoft.Json.JsonProperty("Boundary", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Polygon Boundary { get; set; }

        public override void UpdateRepresentations()
        {
            Representation = new Representation(new[] { new Lamina(Boundary, false) });
        }
    }
}