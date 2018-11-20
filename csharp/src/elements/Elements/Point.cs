using Hypar.Geometry;
using Newtonsoft.Json;

namespace Hypar.Elements
{
    /// <summary>
    /// A Point represents a location in space.
    /// </summary>
    public class Point : Element, IPoint
    {
        /// <summary>
        /// The type of the Point.
        /// </summary>
        [JsonProperty("type")]
        public override string Type
        {
            get{return "point";}
        }

        /// <summary>
        /// A text description of the Point.
        /// </summary>
        [JsonProperty("label")]
        public string Label{get;}
        
        /// <summary>
        /// The location of the Point.
        /// </summary>
        [JsonProperty("location")]
        public Vector3 Location{get;}

        /// <summary>
        /// Construct a point.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="label"></param>
        public Point(Vector3 location, string label=null)
        {
            this.Location = location;
            this.Label = label;
        }
    }
}