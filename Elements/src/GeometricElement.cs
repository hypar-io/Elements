using System;
using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    public partial class GeometricElement
    {
        /// <summary>
        /// A collection of openings.
        /// </summary>
        public List<Opening> Openings { get; } = new List<Opening>();

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