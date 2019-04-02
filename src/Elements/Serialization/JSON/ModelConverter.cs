using Elements.Geometry;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;

namespace Elements.Serialization.JSON
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
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var materials = JsonConvert.DeserializeObject<Dictionary<long, Material>>(obj.GetValue("materials").ToString());
            var profiles = JsonConvert.DeserializeObject<Dictionary<long, Profile>>(obj.GetValue("profiles").ToString(),
                                new[] { new IProfileConverter() });
            var elementTypes = JsonConvert.DeserializeObject<Dictionary<long, ElementType>>(obj.GetValue("element_types").ToString(),
                                new JsonConverter[] { new ElementTypeConverter(), new ProfileToIdConverter(profiles) });
            var extensions = JsonConvert.DeserializeObject<List<string>>(obj.GetValue("extensions").ToString());
            var elements = JsonConvert.DeserializeObject<Dictionary<long, Element>>(obj.GetValue("elements").ToString(),
                            new JsonSerializerSettings()
                            {
                                Converters = new JsonConverter[]
                                                {
                                                    new ElementConverter(extensions),
                                                    new MaterialToIdConverter(materials),
                                                    new ElementTypeToIdConverter(elementTypes),
                                                    new ProfileToIdConverter(profiles),
                                                    new SolidConverter(materials)
                                                },
                                NullValueHandling = NullValueHandling.Ignore
                            });
            return new Model(elements, materials, elementTypes, profiles, extensions);
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

            var settings = new JsonSerializerSettings(){
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            // Write extensions
            writer.WritePropertyName("extensions");
            writer.WriteRawValue(JsonConvert.SerializeObject(model.Extensions));

            // Write materials
            writer.WritePropertyName("materials");
            writer.WriteRawValue(JsonConvert.SerializeObject(model.Materials, settings));

            // Write element types
            writer.WritePropertyName("element_types");
            var etSettings = new JsonSerializerSettings(){
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new JsonConverter[]{
                    new ProfileToIdConverter(model.Profiles)
                }
            };
            writer.WriteRawValue(JsonConvert.SerializeObject(model.ElementTypes, etSettings));

            // Write profiles
            writer.WritePropertyName("profiles");
            writer.WriteRawValue(JsonConvert.SerializeObject(model.Profiles, settings));

            // Write elements
            writer.WritePropertyName("elements");
            writer.WriteRawValue(JsonConvert.SerializeObject(model.Elements, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Converters = new JsonConverter[]
                    {
                        new MaterialToIdConverter(model.Materials),
                        new ElementTypeToIdConverter(model.ElementTypes),
                        new ProfileToIdConverter(model.Profiles),
                        new SolidConverter(model.Materials)
                    },
                NullValueHandling = NullValueHandling.Ignore
            }));

            // Serialize materials
            writer.WriteEndObject();
        }
    }
}