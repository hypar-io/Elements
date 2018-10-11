using Newtonsoft.Json;
using System;

namespace Hypar.Elements
{
    /// <summary>
    /// A container for properties common to Walls.
    /// </summary>
    public class WallType : ElementType
    {
        /// <summary>
        /// The thickness of the Wall.
        /// </summary>
        [JsonProperty("thickness")]
        public double Thickness{get;}

        /// <summary>
        /// The Type of the ElementType.
        /// </summary>
        public override string Type
        {
            get{return "wall_type";}
        }

        /// <summary>
        /// Construct a WallType.
        /// </summary>
        /// <param name="name">The name of the WallType.</param>
        /// <param name="thickness">The thickness for all Walls of this WallType.</param>
        /// <param name="description">The description of the WallType.</param>
        /// <returns></returns>
        public WallType(string name, double thickness, string description = null) : base(name, description)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The WallType could not be constructed. The thickness of the WallType must be greater than 0.0.");
            }
            this.Thickness = thickness;
        }
    }
}