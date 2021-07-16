using System;
using System.Collections.Generic;
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
                this.Ids = new List<Guid>();
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
            /// The linewight of the layer, in 1/100s of a millimeter.
            /// </summary>
            public int Lineweight { get; set; }

            /// <summary>
            /// The IDs of specific elements to be included on this layer.
            /// </summary>
            /// <value></value>
            public List<Guid> Ids { get; set; }

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
    }
}