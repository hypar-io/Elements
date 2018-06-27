using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Hypar.Geometry;
using Hypar.GeoJSON;

namespace Hypar.Configuration
{
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
        public virtual string Type{get;set;}

        /// <summary>
        /// Construct a ParameterData.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="type"></param>
        public ParameterData(string description, string type)
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
        /// <param name="description"></param>
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
        /// <returns></returns>
        [JsonProperty("type")]
        public string Type{get;set;}
    }

    /// <summary>
    /// Converter for types which inherit from ParameterData
    /// </summary>
    public class ParameterDataConverter : JsonConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ParameterData);
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
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
        /// 
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