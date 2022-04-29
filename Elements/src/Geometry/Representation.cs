using System.Collections.Generic;
using Elements.Geometry.Solids;
using System.Text.Json.Serialization;

namespace Elements.Geometry
{
    /// <summary>The representation of an element.</summary>
    public class Representation
    {
        /// <summary>A collection of solid operations.</summary>
        [System.ComponentModel.DataAnnotations.Required]
        public IList<SolidOperation> SolidOperations { get; set; } = new List<SolidOperation>();

        /// <summary>
        /// Construct a representation.
        /// </summary>
        /// <param name="solidOperations">A collection of solid operations.</param>
        [JsonConstructor]
        public Representation(IList<SolidOperation> @solidOperations)
        {
            this.SolidOperations = @solidOperations;
        }

        /// <summary>
        /// Construct a Representation from SolidOperations. This is a convenience constructor
        /// that can be used like this: `new Representation(new Extrude(...))`
        /// </summary>
        /// <param name="solidOperations">The solid operations composing this representation.</param>
        public Representation(params SolidOperation[] solidOperations) : this(new List<SolidOperation>(solidOperations))
        {

        }

        /// <summary>
        /// Automatically convert a single solid operation into a representation containing that operation.
        /// </summary>
        /// <param name="solidOperation">The solid operation composing this Representation.</param>
        public static implicit operator Representation(SolidOperation solidOperation)
        {
            return new Representation(solidOperation);
        }

        /// <summary>
        /// A flag to disable CSG operations on this representation. Instead,
        /// all solids will be meshed, and all voids will be ignored.
        /// </summary>
        [JsonIgnore]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool SkipCSGUnion { get; set; } = false;
    }
}