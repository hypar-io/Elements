using Newtonsoft.Json;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// The base class for all operations which create solids.
    /// </summary>
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public abstract class SolidOperation
    {
        internal Solid _solid;

        /// <summary>
        /// The local transform of the operation.
        /// </summary>
        public Transform LocalTransform { get; set; }

        /// <summary>
        /// The solid operation's solid.
        /// </summary>
        [JsonIgnore]
        public Solid Solid
        {
            get { return _solid; }
        }

        /// <summary>Is the solid operation a void operation?</summary>
        [JsonProperty("IsVoid", Required = Required.Always)]
        public bool IsVoid { get; set; } = false;

        /// <summary>
        /// Construct a solid operation.
        /// </summary>
        /// <param name="isVoid"></param>
        [JsonConstructor]
        public SolidOperation(bool @isVoid)
        {
            this.IsVoid = @isVoid;
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
    }
}