using Newtonsoft.Json;
using System;

namespace Elements
{
    /// <summary>
    /// A container for properties common to walls.
    /// </summary>
    public class WallType : ElementType
    {
        /// <summary>
        /// The thickness of the Wall.
        /// </summary>
        [JsonProperty("thickness")]
        public double Thickness { get; }

        /// <summary>
        /// The type of the wall type.
        /// </summary>
        public override string Type
        {
            get { return "wall_type"; }
        }

        /// <summary>
        /// Construct a wall type.
        /// </summary>
        /// <param name="name">The name of the wall type.</param>
        /// <param name="thickness">The thickness for all walls of this wall type.</param>
        /// <param name="description">The description of the wall type.</param>
        /// <returns></returns>
        public WallType(string name, double thickness, string description = null) : base(name, description)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The WallType could not be created. The thickness of the WallType must be greater than 0.0.");
            }
            this.Thickness = thickness;
        }
    }
}