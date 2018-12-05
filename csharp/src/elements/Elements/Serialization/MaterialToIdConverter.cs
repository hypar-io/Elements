using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hypar.Elements.Serialization
{
    /// <summary>
    /// Converts a Material to its identifier and back.
    /// </summary>
    public class MaterialToIdConverter : JsonConverter
    {
        private Dictionary<long, Material> _materials;

        /// <summary>
        /// Construct a MaterialConverter.
        /// </summary>
        /// <param name="materials">A collection of Materials.</param>
        public MaterialToIdConverter(Dictionary<long, Material> materials)
        {
            this._materials = materials;
        }

        /// <summary>
        /// Can this converter convert an object of type objectType?
        /// </summary>
        /// <param name="objectType"></param>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Material);
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
            var id = (long)reader.Value;
            if(this._materials.ContainsKey(id))
            {
                return this._materials[id]; 
            }
            else
            {
                throw new Exception($"The specified material, {id}, cannot be found.");
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
            var material = (Material)value;
            writer.WriteValue(material.Id);
        }
    }
}