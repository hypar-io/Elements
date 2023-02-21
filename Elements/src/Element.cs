using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An object which is identified with a unique identifier and a name.
    /// </summary>
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
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
        public Element(System.Guid @id = default(Guid), string @name = null, Dictionary<MappingContext, MappingBase> @mappings = null)
        {
            this._id = @id;
            this._name = @name;

            if (this._id == default(Guid))
            {
                this._id = System.Guid.NewGuid();
            }
            _mappings = @mappings;
        }

        /// <summary>A unique id.</summary>
        [JsonProperty("Id", Required = Required.Always)]
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
        [JsonProperty("Name", Required = Required.AllowNull)]
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
        [Newtonsoft.Json.JsonExtensionData]
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
        [JsonProperty("Mappings", Required = Required.Default)]
        internal Dictionary<MappingContext, MappingBase> _mappings { get; set; } = null;

        /// <summary>
        /// The method used to set a mapping for a given context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mapping"></param>
        public void SetMapping(MappingContext context, MappingBase mapping)
        {
            if (this._mappings == null)
            {
                this._mappings = new Dictionary<MappingContext, MappingBase>();
            }
            this._mappings[context] = mapping;
        }

        /// <summary>
        /// Retrieve a mapping for a given context.
        /// </summary>
        /// <param name="context">The context of the mapping being requested.</param>
        /// <returns>The mapping if it exists, null if not.</returns>
        public MappingBase GetMapping(MappingContext context)
        {
            if (this._mappings == null)
            {
                return null;
            }
            if (this._mappings.ContainsKey(context))
            {
                return this._mappings[context];
            }
            return null;
        }

        /// <summary>
        /// Retrieve a mapping for a given context.
        /// </summary>
        /// <typeparam name="T">The Type of mapping expected.</typeparam>
        /// <param name="context">The context of the mapping being requested.</param>
        /// <returns></returns>
        public T GetMapping<T>(MappingContext context) where T : MappingBase
        {
            return this.GetMapping(context) as T;
        }
    }
}
