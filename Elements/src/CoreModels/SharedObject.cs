using System;
using System.Text.Json.Serialization;

namespace Elements
{
    /// <summary>
    /// An object with a unique identifier which is shared by one or more elements, and excluded from element serialization.
    /// </summary>
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
        [JsonPropertyName("Id")]
        [JsonInclude]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public Guid Id
        {
            get => _id;
            set { _id = value; }
        }
    }
}