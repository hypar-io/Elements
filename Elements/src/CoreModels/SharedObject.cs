using System;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An object which is identified with a unique identifier.
    /// </summary>
    [JsonConverter(typeof(Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public class SharedObject
    {
        private Guid _id;

        /// <summary>
        /// Initializes a new instance of SharedObject.
        /// </summary>
        /// <param name="id">The unique id of the object.</param> 
        [JsonConstructor]
        public SharedObject(Guid id = default)
        {
            _id = id;

            if (_id == default)
            {
                _id = Guid.NewGuid();
            }
        }

        /// <summary>
        /// A unique object id.
        /// </summary>
        [JsonProperty("Id", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public Guid Id
        {
            get => _id;
            set { _id = value; }
        }
    }
}