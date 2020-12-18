using System;

namespace Elements.Geometry
{
    public abstract partial class Representation
    {
        /// <summary>
        /// Create a representation with a default id.
        /// </summary>
        public Representation(Material material) : base(Guid.NewGuid(), null)
        {
            this.Material = material;
        }
    }
}