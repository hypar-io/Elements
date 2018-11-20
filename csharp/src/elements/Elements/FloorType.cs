using Newtonsoft.Json;
using System;

namespace Hypar.Elements
{
    /// <summary>
    /// A container for properties common to Floors.
    /// </summary>
    public class FloorType : ElementType
    {
        /// <summary>
        /// The thickness of the Floor.
        /// </summary>
        [JsonProperty("thickness")]
        public double Thickness{get;}

        /// <summary>
        /// The type of the FloorType.
        /// </summary>
        public override string Type
        {
            get{return "floor_type";}
        }

        /// <summary>
        /// Construct a FloorType.
        /// </summary>
        /// <param name="name">The name of the FloorType.</param>
        /// <param name="thickness">The thickness of the associated floor.</param>
        /// <param name="description">A description of the FloorType.</param>
        public FloorType(string name, double thickness, string description = null) : base(name, description)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("thickness", "The floor type thickness must be greater than 0.0.");
            }

            this.Thickness = thickness;
        }
    }
}