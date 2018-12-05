using Elements.Geometry;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization
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
            var materials = JsonConvert.DeserializeObject<Dictionary<long,Material>>(obj.GetValue("materials").ToString());
            var elementTypes = JsonConvert.DeserializeObject<Dictionary<long,ElementType>>(obj.GetValue("element_types").ToString(), 
                                new []{new ElementTypeConverter()});
            var profiles = JsonConvert.DeserializeObject<Dictionary<long,Profile>>(obj.GetValue("profiles").ToString());
            var elements = JsonConvert.DeserializeObject<Dictionary<long,Element>>(obj.GetValue("elements").ToString(),
                            new JsonSerializerSettings()
                            {
                                Converters = new JsonConverter[]
                                                {
                                                    new ElementConverter(),
                                                    new MaterialToIdConverter(materials),
                                                    new ElementTypeToIdConverter(elementTypes),
                                                    new ProfileToIdConverter(profiles),
                                                },
                                NullValueHandling = NullValueHandling.Ignore
                            });                                                
            return new Model(elements, materials, elementTypes, profiles);
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

            // Write profiles
            writer.WritePropertyName("profiles");
            writer.WriteRawValue(JsonConvert.SerializeObject(model.Profiles, new JsonSerializerSettings(){
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            }));

            // Write elements
            writer.WritePropertyName("elements");
            writer.WriteRawValue(JsonConvert.SerializeObject(model.Elements, new JsonSerializerSettings(){
                Formatting = Formatting.Indented,
                Converters = new JsonConverter[]
                    {
                        new MaterialToIdConverter(model.Materials),
                        new ElementTypeToIdConverter(model.ElementTypes),
                        new ProfileToIdConverter(model.Profiles)
                    },
                NullValueHandling = NullValueHandling.Ignore
            }));
            
            // Serialize materials
            writer.WriteEndObject();
        }
    }
}