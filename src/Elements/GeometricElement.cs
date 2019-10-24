using System;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    public abstract partial class GeometricElement
    {
        /// <summary>
        /// Construct a geometric element.
        /// </summary>
        /// <param name="material">The element's material.</param>
        /// <param name="id">The unique identifer of the element.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="transform">The element's transform.</param>
        [JsonConstructor]
        public GeometricElement(Material material = null, Transform transform = null, Guid id = default(Guid), string name = null): base(id, name)
        {
            this.Transform = transform ?? new Transform();
            this.Material = material ?? BuiltInMaterials.Default;
        }

        /// <summary>
        /// This method provides an opportunity for geometric elements
        /// to adjust their solid operations before tesselation. As an example,
        /// a floor might want to clip its opening profiles out of 
        /// the profile of the floor.
        /// </summary>
        public abstract void UpdateRepresentations();
    }
}