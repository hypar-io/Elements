//----------------------
// <auto-generated>
//     Generated using the NJsonSchema v10.1.21.0 (Newtonsoft.Json v13.0.0.0) (http://NJsonSchema.org)
// </auto-generated>
//----------------------
using Elements;
using Elements.GeoJSON;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Spatial;
using Elements.Validators;
using Elements.Serialization.JSON;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Line = Elements.Geometry.Line;
using Polygon = Elements.Geometry.Polygon;

namespace Elements.Fittings
{
    #pragma warning disable // Disable all warnings

    /// <summary>Dimensions of port extra properties</summary>
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.1.21.0 (Newtonsoft.Json v13.0.0.0)")]
    public partial class PortDimensions 
    {
        [JsonConstructor]
        public PortDimensions(double @extension, double @bodyDiameter, double @bodyLength)
        {
            this.Extension = @extension;
            this.BodyDiameter = @bodyDiameter;
            this.BodyLength = @bodyLength;
            }
        
        // Empty constructor
        public PortDimensions()
        {
        }
    
        /// <summary>Length that the fitting extends beyond the port.</summary>
        [JsonProperty("Extension", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double Extension { get; set; }
    
        /// <summary>Diameter of the  “body” of the connector, expected to be larger than the diameter of the connector.</summary>
        [JsonProperty("Body Diameter", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double BodyDiameter { get; set; }
    
        /// <summary>Length of the body of the connector, length of the “thicker” part of the connector.</summary>
        [JsonProperty("Body Length", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double BodyLength { get; set; }
    
    
    }
}