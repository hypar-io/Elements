using System;
using Elements.Geometry;

namespace Elements
{
    public partial class GeometricElement
    {
        /// <summary>
        /// The geometric element's representation.
        /// </summary>
        [Obsolete("Geometric elements can now store multiple representations. Use representations instead.")]
        public SolidRepresentation Representation
        {
            // Geometric elements previously only stored one type of representation,
            // a type akin to a SolidRepresentation. We serialize a solid representation,
            // if one exists, in order to make JSON created from this version
            // compatible with previous versions. If the incoming JSON contains
            // a representation we do similarly, setting it to the first representation.
            get
            {
                if (this.Representations != null && this.Representations.Count >= 1 && this.Representations[0].GetType() == typeof(SolidRepresentation))
                {
                    return (SolidRepresentation)this.Representations[0];
                }
                return null;
            }
            set
            {
                if (this.Representations != null && value.GetType() == typeof(SolidRepresentation))
                {
                    this.Representations[0] = value;
                }
            }
        }

        /// <summary>
        /// This method provides an opportunity for geometric elements
        /// to adjust their solid operations before tesselation. As an example,
        /// a floor might want to clip its opening profiles out of 
        /// the profile of the floor.
        /// </summary>
        public virtual void UpdateRepresentations()
        {
            // Override in derived classes.
        }

        /// <summary>
        /// Create an instance of this element.
        /// Instances will point to the same instance of an element.
        /// </summary>
        /// <param name="transform">The transform for this element instance.</param>
        /// <param name="name">The name of this element instance.</param>
        public ElementInstance CreateInstance(Transform transform, string name)
        {
            if (!this.IsElementDefinition)
            {
                throw new Exception($"An instance cannot be created of the type {this.GetType().Name} because it is not marked as an element definition. Set the IsElementDefinition flag to true.");
            }

            return new ElementInstance(this, transform, name, Guid.NewGuid());
        }
    }
}