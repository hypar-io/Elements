//----------------------
// <auto-generated>
//     Generated using the NJsonSchema v10.1.21.0 (Newtonsoft.Json v11.0.0.0) (http://NJsonSchema.org)
// </auto-generated>
//----------------------
using Elements;
using Elements.GeoJSON;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Spatial;
using Elements.Validators;
using Elements.Serialization.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using Line = Elements.Geometry.Line;
using Polygon = Elements.Geometry.Polygon;

namespace Elements.Geometry
{
    #pragma warning disable // Disable all warnings

    /// <summary>A line between two points. The line is parameterized from 0.0(start) to 1.0(end)</summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.1.21.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class Line : Curve
    {
        [Newtonsoft.Json.JsonConstructor]
        public Line(Vector3 @start, Vector3 @end)
            : base()
        {
            var validator = Validator.Instance.GetFirstValidatorForType<Line>();
            if(validator != null)
            {
                validator.PreConstruct(new object[]{ @start, @end});
            }
        
            this.Start = @start;
            this.End = @end;
            
            if(validator != null)
            {
                validator.PostConstruct(this);
            }
        }
    
        /// <summary>The start of the line.</summary>
        [Newtonsoft.Json.JsonProperty("Start", Required = Newtonsoft.Json.Required.AllowNull)]
        public Vector3 Start { get; set; }
    
        /// <summary>The end of the line.</summary>
        [Newtonsoft.Json.JsonProperty("End", Required = Newtonsoft.Json.Required.AllowNull)]
        public Vector3 End { get; set; }
    
    
    }
}