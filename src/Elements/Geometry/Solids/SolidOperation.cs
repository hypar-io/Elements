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
        /// 
        /// </summary>
        /// <param name="isVoid"></param>
        public static void ValidateConstructorParameters(bool isVoid)
        {
            return;
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