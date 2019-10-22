using Elements.Serialization.JSON;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// The base class for all operations which create solids.
    /// </summary>
    [JsonInheritanceAttribute("Elements.Geometry.Solids.Sweep", typeof(Sweep))]
    [JsonInheritanceAttribute("Elements.Geometry.Solids.Extrude", typeof(Extrude))]
    [JsonInheritanceAttribute("Elements.Geometry.Solids.Lamina", typeof(Lamina))]
    public abstract partial class SolidOperation
    {
        /// <summary>
        /// Create a solid operation.
        /// </summary>
        /// <param name="isVoid">Is the solid operation a void operation?</param>
        public SolidOperation(bool isVoid)
        {
            this.IsVoid = isVoid;
        }

        /// <summary>
        /// Get the updated solid for this operation.
        /// </summary>
        internal virtual Solid GetSolid()
        {
            // Override in derived classes.
            return null;
        }
    }
}