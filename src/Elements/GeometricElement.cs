using System;
using Elements.Geometry;

namespace Elements
{
    public abstract partial class GeometricElement
    {
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
        /// Instances will point to the same 
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="name"></param>
        public ElementInstance CreateInstance(Transform transform, string name)
        {
            return new ElementInstance(this, transform, name, Guid.NewGuid());
        }
    }
}