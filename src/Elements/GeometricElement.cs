using Elements.Geometry;

namespace Elements
{
    public abstract partial class GeometricElement
    {
        internal static void ValidateConstructorParameters(Transform @transform, Material @material, Representation @representation, System.Guid @id, string @name)
        {
            return;
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
    }
}