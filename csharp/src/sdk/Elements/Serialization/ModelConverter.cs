using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hypar.Elements.Serialization
{
    /// <summary>
    /// Convert a Model.
    /// </summary>
    public class ModelConverter : JsonConverter
    {
        /// <summary>
        /// Can this converter convert and object of type objectType?
        /// </summary>
        /// <param name="objectType"></param>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Model);
        }

        /// <summary>
        /// Read json.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var materials = JsonConvert.DeserializeObject<Dictionary<string,Material>>(obj.GetValue("materials").ToString());
            var elementTypes = JsonConvert.DeserializeObject<Dictionary<string,ElementType>>(obj.GetValue("element_types").ToString(), 
                                new []{new ElementTypeConverter()});
            var elements = JsonConvert.DeserializeObject<Dictionary<string,Element>>(obj.GetValue("elements").ToString(),
                            new JsonSerializerSettings()
                            {
                                Converters = new JsonConverter[]
                                                {
                                                    new ElementConverter(),
                                                    new MaterialToIdConverter(materials),
                                                    new ElementTypeToIdConverter(elementTypes)
                                                },
                                Formatting = Formatting.Indented
                            });                                                
            return new Model(elements, materials, elementTypes);
        }

        /// <summary>
        /// Write json.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var model = (Model)value;
            writer.WriteStartObject();

            // Write materials
            writer.WritePropertyName("materials");
            writer.WriteRawValue(JsonConvert.SerializeObject(model.Materials, new JsonSerializerSettings(){
                Formatting = Formatting.Indented
            }));

            // Write element types
            writer.WritePropertyName("element_types");
            writer.WriteRawValue(JsonConvert.SerializeObject(model.ElementTypes, new JsonSerializerSettings(){
                Formatting = Formatting.Indented
            }));

            // Write elements
            writer.WritePropertyName("elements");
            writer.WriteRawValue(JsonConvert.SerializeObject(model.Elements, new JsonSerializerSettings(){
                Formatting = Formatting.Indented,
                Converters = new JsonConverter[]
                            {
                                new ElementConverter(), 
                                new MaterialToIdConverter(model.Materials),
                                new ElementTypeToIdConverter(model.ElementTypes)
                            }
            }));
            
            // Serialize materials
            writer.WriteEndObject();
        }
    }
}