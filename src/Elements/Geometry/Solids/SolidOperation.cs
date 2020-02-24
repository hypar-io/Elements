using Elements.Serialization.JSON;
using Newtonsoft.Json;

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
        internal Solid _solid;

        /// <summary>
        /// The solid operation's solid. To update this
        /// cached representation, call GetSolid().
        /// </summary>
        [JsonIgnore]
        public Solid Solid
        {
            get {return _solid;}
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