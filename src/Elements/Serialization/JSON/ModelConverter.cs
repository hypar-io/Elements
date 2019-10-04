using Elements.Geometry;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Elements.ElementTypes;

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
            var materials = JsonConvert.DeserializeObject<Dictionary<Guid, Material>>(obj.GetValue("materials").ToString());
            var profiles = JsonConvert.DeserializeObject<Dictionary<Guid, Profile>>(obj.GetValue("profiles").ToString());
                                // new[] { new IProfileConverter() });
            var elementTypes = JsonConvert.DeserializeObject<Dictionary<Guid, ElementType>>(obj.GetValue("elementTypes").ToString(),
                                new JsonConverter[] { new ElementTypeConverter(), new ProfileToIdConverter(profiles) });
            var elements = JsonConvert.DeserializeObject<Dictionary<Guid, Element>>(obj.GetValue("elements").ToString(),
                new JsonSerializerSettings()
                {
                    Converters = new JsonConverter[]
                                    {
                                        new MaterialToIdConverter(materials),
                                        // new ElementTypeToIdConverter(elementTypes),
                                        new ProfileToIdConverter(profiles),
                                        new SolidConverter(materials)
                                    },
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver() 
                });
            var origin = JsonConvert.DeserializeObject<GeoJSON.Position>(obj.GetValue("origin").ToString());
            var model = new Model(elements, materials, elementTypes, profiles){
                Origin = origin
            };
            return model;
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
                NullValueHandling = NullValueHandling.Ignore,
                // ContractResolver = new CamelCasePropertyNamesContractResolver() 
            };

            // Write materials
            writer.WritePropertyName("materials");
            writer.WriteRawValue(JsonConvert.SerializeObject(model.Materials, settings));

            // Write element types
            writer.WritePropertyName("elementTypes");
            var etSettings = new JsonSerializerSettings(){
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new JsonConverter[]{
                    new MaterialToIdConverter(model.Materials),
                    new ProfileToIdConverter(model.Profiles)
                },
                // ContractResolver = new CamelCasePropertyNamesContractResolver() 
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
                        // new ElementTypeToIdConverter(model.ElementTypes),
                        new ProfileToIdConverter(model.Profiles),
                        new SolidConverter(model.Materials)
                    },
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver() 
            }));

            // Write origin
            writer.WritePropertyName("origin");
            writer.WriteRawValue(JsonConvert.SerializeObject(model.Origin, settings));

            writer.WriteEndObject();
        }
    }
}