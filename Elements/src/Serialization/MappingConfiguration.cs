using System.Collections.Generic;
using System.Text.Json.Serialization;
using Elements.Geometry;

namespace Elements.Serialization
{
    /// <summary>
    /// Additional metadata to guide layer creation for various output
    /// formats, such as DXF. 
    /// </summary>
    public class MappingConfiguration
    {
        /// <summary>
        /// Create a new MappingConfiguration
        /// /// </summary>
        public MappingConfiguration()
        {
            this.Layers = new List<Layer>();
        }

        /// <summary>
        /// The layer configurations for this model.
        /// </summary>
        /// <value></value>
        public List<Layer> Layers { get; set; }

        /// <summary>
        /// Represents the configuration of a Layer for export.
        /// </summary>
        public class Layer
        {
            /// <summary>
            /// Create a new Layer configuration.
            /// </summary>
            public Layer()
            {
            }

            /// <summary>
            /// The name of the layer.
            /// </summary>
            public string LayerName { get; set; }

            /// <summary>
            /// The display color of the layer.
            /// </summary>
            /// <value></value>
            public Color LayerColor { get; set; }

            /// <summary>
            /// How items on this layer should have their colors determined.
            /// </summary>
            [JsonConverter(typeof(JsonStringEnumConverter))]

            public ElementColorSetting ElementColorSetting { get; set; } = ElementColorSetting.ByLayer;

            /// <summary>
            /// The linewight of the layer, in 1/100s of a millimeter.
            /// </summary>
            public int Lineweight { get; set; }

            /// <summary>
            /// The type names (FullNames) of element types that should be mapped to this layer.
            /// </summary>
            /// <value></value>
            public List<string> Types { get; set; }
        }

        /// <summary>
        /// Merge another export configuration into this one.
        /// </summary>
        /// <param name="other"></param>
        public void Merge(MappingConfiguration other)
        {
            //TODO: handle resolving duplication
            this.Layers.AddRange(other.Layers);
        }

        /// <summary>
        /// How an item on a layer should have its color determined.
        /// </summary>
        public enum ElementColorSetting
        {
            /// <summary>
            /// Use the color of the item's layer (The default setting).
            /// </summary>
            ByLayer,
            /// <summary>
            /// Attempt to set the item's color based on its material.
            /// </summary>
            TryGetColorFromMaterial
        }
    }
}