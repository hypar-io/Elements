using System;
using System.Collections.Generic;
using Elements.Geometry.Solids;

namespace Elements.Geometry
{
    public partial class Representation
    {
        /// <summary>A collection of solid operations.</summary>
        [Newtonsoft.Json.JsonProperty("SolidOperations", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Obsolete("Use SolidRepresentation instead.")]
        public IList<SolidOperation> SolidOperations { get; set; } = new List<SolidOperation>();

        /// <summary>
        /// Create a representation with a default id.
        /// </summary>
        // public Representation(Material material) : base(Guid.NewGuid(), null)
        // {
        //     this.Material = material;
        // }
    }
}