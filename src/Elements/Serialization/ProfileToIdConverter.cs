#pragma warning disable CS1591

using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements.Serialization
{
    public class ProfileToIdConverter : JsonConverter
    {
        private Dictionary<long, IProfile> _profiles;

        public ProfileToIdConverter(Dictionary<long, IProfile> profiles)
        {
            this._profiles = profiles;
        }

        public override bool CanConvert(Type objectType)
        {
            var convert = typeof(IProfile).IsAssignableFrom(objectType);
            return convert;
        }

        public override bool CanRead
        {
            get{return true;}
        }

        public override bool CanWrite
        {
            get{return true;}
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var id = (long)reader.Value;
            
            if(this._profiles.ContainsKey(id))
            {
                return this._profiles[id]; 
            }
            else
            {
                throw new Exception($"The specified profile, {id}, cannot be found.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var profile = (IProfile)value;
            writer.WriteValue(profile.Id);
        }
    }
}