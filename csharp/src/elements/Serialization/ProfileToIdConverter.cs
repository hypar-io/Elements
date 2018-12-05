#pragma warning disable CS1591

using Elements.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements.Serialization
{
    public class ProfileToIdConverter : JsonConverter
    {
        private Dictionary<long, Profile> _profiles;

        public ProfileToIdConverter(Dictionary<long, Profile> profiles)
        {
            this._profiles = profiles;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Profile).IsAssignableFrom(objectType);
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
            var profile = (Profile)value;
            writer.WriteValue(profile.Id);
        }
    }
}