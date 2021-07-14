#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization.JSON
{
    public class DiscriminatorTypeConverter : Newtonsoft.Json.JsonConverter
    {
        private List<Type> _loadedTypes;
        public DiscriminatorTypeConverter()
        {
            _loadedTypes = typeof(Element).Assembly.GetTypes().ToList();
        }
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            var discriminator = obj.Value<string>("discriminator");
            var matchingType = _loadedTypes.FirstOrDefault(t => t.FullName.Equals(discriminator));
            return JsonConvert.DeserializeObject(reader.ReadAsString(), matchingType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}