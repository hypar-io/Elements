using System;
using System.Collections.Generic;
using Elements.Geometry.Solids;

namespace Elements.Geometry
{
    public partial class SolidRepresentation
    {
        /// <summary>
        /// Create a solid representation.
        /// </summary>
        public SolidRepresentation() : base(BuiltInMaterials.Default, Guid.NewGuid(), null) { }

        /// <summary>
        /// Create a solid representation.
        /// </summary>
        /// <param name="solidOperations">A collection of solid operations.</param>
        /// <param name="material">The material to apply to the resulting solid.</param>
        public SolidRepresentation(IList<SolidOperation> solidOperations, Material material) : base(material, Guid.NewGuid(), null)
        {
            this.SolidOperations = solidOperations;
        }

        /// <summary>
        /// Create a solid representation.
        /// </summary>
        /// <param name="solidOperations">A collection of solid operations.</param>
        public SolidRepresentation(IList<SolidOperation> solidOperations) : base(BuiltInMaterials.Default, Guid.NewGuid(), null)
        {
            this.SolidOperations = solidOperations;
        }

        /// <summary>
        /// Create a solid representation.
        /// </summary>
        /// <param name="material">The material to apply to the resulting solid.</param>
        public SolidRepresentation(Material material) : base(material, Guid.NewGuid(), null) { }
    }
}