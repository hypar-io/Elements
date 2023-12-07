using System;
using System.Collections.Generic;
using Elements.Serialization.JSON;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Elements
{
    /// <summary>
    /// An object which is identified with a unique identifier and a name.
    /// </summary>
    [JsonConverter(typeof(ElementConverter<Element>))]
    public class Element : System.ComponentModel.INotifyPropertyChanged
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
        [JsonPropertyName("Name")]
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
        /// An optional dictionary of mappings.
        /// </summary>
        [JsonPropertyName("Mappings")]
        internal Dictionary<string, MappingBase> Mappings { get; set; } = null;

        /// <summary>
        /// The method used to set a mapping for a given context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mapping"></param>
        public void SetMapping(string context, MappingBase mapping)
        {
            if (this.Mappings == null)
            {
                this.Mappings = new Dictionary<string, MappingBase>();
            }
            this.Mappings[context] = mapping;
        }

        /// <summary>
        /// Retrieve a mapping for a given context.
        /// </summary>
        /// <param name="context">The context of the mapping being requested.</param>
        /// <returns>The mapping if it exists, null if not.</returns>
        public MappingBase GetMapping(string context)
        {
            if (this.Mappings == null)
            {
                return null;
            }
            if (this.Mappings.ContainsKey(context))
            {
                return this.Mappings[context];
            }
            return null;
        }

        /// <summary>
        /// Retrieve a mapping for a given context.
        /// </summary>
        /// <typeparam name="T">The Type of mapping expected.</typeparam>
        /// <param name="context">The context of the mapping being requested.</param>
        /// <returns></returns>
        public T GetMapping<T>(string context) where T : MappingBase
        {
            return this.GetMapping(context) as T;
        }

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
