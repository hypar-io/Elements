using System;
using Elements.Serialization.JSON;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Elements
{
    /// <summary>
    /// An object which is identified with a unique identifier and a name.
    /// </summary>
    [JsonConverter(typeof(ElementConverter<Element>))]
    public abstract class Element : System.ComponentModel.INotifyPropertyChanged
    {
        private System.Guid _id;
        private string _name;

        /// <summary>
        /// Construct an element.
        /// </summary>
        /// <param name="id">The unique id of the element.</param>
        /// <param name="name">The name of the element.</param>
        [JsonConstructor]
        public Element(System.Guid @id = default(Guid), string @name = null)
        {
            this._id = @id;
            this._name = @name;

            if (this._id == default(Guid))
            {
                this._id = System.Guid.NewGuid();
            }
        }

        /// <summary>A unique id.</summary>
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public System.Guid Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>A name.</summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private System.Collections.Generic.IDictionary<string, object> _additionalProperties = new System.Collections.Generic.Dictionary<string, object>();

        /// <summary>
        /// A collection of additional properties.
        /// </summary>
        // [JsonExtensionData]
        [JsonConverter(typeof(AdditionalPropertiesConverter))]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }

        /// <summary>
        /// An event raised when a property is changed.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise a property change event.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Deserialize an element of type T using reference resolution
        /// and discriminated type construction.
        /// Do not use this method for the deserialization large numbers of
        /// elements, as it needs to construct a reference handler and type cache
        /// for each invocation.
        /// </summary>
        /// <param name="json">The JSON of the element.</param>
        /// <returns>An element of type T.</returns>
        public static T Deserialize<T>(string json)
        {
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                var options = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    IncludeFields = true
                };
                var typeCache = AppDomainTypeCache.BuildAppDomainTypeCache(out _);
                var refHandler = new ElementReferenceHandler(typeCache, root);
                options.ReferenceHandler = refHandler;
                return JsonSerializer.Deserialize<T>(doc, options);
            }
        }
    }
}