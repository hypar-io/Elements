using System.Text.Json.Serialization;
using Elements.Serialization.JSON;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// Create a custom SolidOperation from imported geometry.
    /// </summary>
    public class ConstructedSolid : SolidOperation
    {
        /// <summary>
        /// Create an import solid.
        /// </summary>
        [JsonConstructor]
        public ConstructedSolid() : base(false)
        {
        }

        /// <summary>
        /// The constructed solid.
        /// </summary>
        // This is hack to enable serialization of the normally
        // hidden solid property.
        public new Solid Solid
        {
            get { return _solid; }
            set { _solid = value; }
        }

        /// <summary>
        /// Create an import solid.
        /// </summary>
        /// <param name="solid">The solid which was imported.</param>
        /// <param name="isVoid">Is the operation a void?</param>
        public ConstructedSolid(Solid solid, bool isVoid = false) : base(isVoid)
        {
            _solid = solid;
        }
    }
}