using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Hypar.Geometry;
using Hypar.GeoJSON;

namespace Hypar.Configuration
{
    /// <summary>
    /// An enumeration of possible parameter types.
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// A numeric parameter.
        /// </summary>
        [EnumMember(Value = "number")]
        Number,
        /// <summary>
        /// A location parameter.
        /// </summary>
        [EnumMember(Value = "location")]
        Location, 
        /// <summary>
        /// A point parameter.
        /// </summary>
        [EnumMember(Value = "point")]
        Point
    }

    /// <summary>
    /// Base class for Hypar configuration input parameters.
    /// </summary>
    public abstract class ParameterData
    {
        /// <summary>
        /// A description of the parameter.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("description")]
        public string Description{get;set;}

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual ParameterType Type{get;set;}

        /// <summary>
        /// Construct a ParameterData.
        /// </summary>
        /// <param name="description">The description of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        public ParameterData(string description, ParameterType type)
        {
            this.Description = description;
            this.Type = type;
        }
    }

    /// <summary>
    /// A point parameter.
    /// </summary>
    public class PointParameter: ParameterData
    {
        /// <summary>
        /// Construct a point parameter.
        /// </summary>
        /// <param name="description">The description of the point.</param>
        public PointParameter(string description):base(description, "point"){}
    }

    /// <summary>
    /// A location parameter.
    /// </summary>
    public class LocationParameter: ParameterData
    {
        /// <summary>
        /// Construct a location parameter.
        /// </summary>
        /// <param name="description"></param>
        public LocationParameter(string description):base(description, "location"){}
    }

    /// <summary>
    /// A numeric parameter.
    /// </summary>
    public class NumberParameter: ParameterData
    {
        /// <summary>
        /// The minimum value of the parameter.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("min")]
        public double Min{get;set;}

        /// <summary>
        /// The maximum value of the parameter.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("max")]
        public double Max{get;set;}

        /// <summary>
        /// The step of the parameter.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("step")]
        public double Step{get;set;}

        /// <summary>
        /// Construct a NumberParameter.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public NumberParameter(string description, double min, double max, double step) : base(description, "number")
        {
            if(min > max)
            {
                throw new ArgumentException($"The number parameter could not be created. The min value, {min}, cannot be greater than the max value, {max}.");
            }

            if(step <= 0.0)
            {
                throw new ArgumentException($"The number parameter could not be created. The step value must greater than 0.0");
            }
            this.Min = min;
            this.Max = max;
            this.Step = step;
        }
    }

    /// <summary>
    /// Return data for a Hypar function.
    /// </summary>
    public class ReturnData
    {
        /// <summary>
        /// A description of the return value.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("description")]
        public string Description{get;set;}

        /// <summary>
        /// The type of the return value.
        /// </summary>
        [JsonProperty("type")]
        public string Type{get;set;}
    }

    /// <summary>
    /// Converter for types which inherit from ParameterData
    /// </summary>
    public class ParameterDataConverter : JsonConverter
    {
        /// <summary>
        /// Can this converter convert an object of the specified type?
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ParameterData);
        }

        /// <summary>
        /// Can this converter write json?
        /// </summary>
        public override bool CanWrite
        {
            get{return false;}
        }

        /// <summary>
        /// Read json.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            string typeName = (jsonObject["type"]).ToString();
            switch(typeName)
            {
                case "number":
                    return jsonObject.ToObject<NumberParameter>(serializer);
                case "point":
                    return jsonObject.ToObject<PointParameter>(serializer);
                case "location":
                    return jsonObject.ToObject<LocationParameter>(serializer);
                default:
                    return jsonObject.ToObject<ParameterData>(serializer);
            }
        }

        /// <summary>
        /// Write json.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}