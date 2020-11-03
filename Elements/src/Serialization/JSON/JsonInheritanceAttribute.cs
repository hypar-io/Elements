namespace Elements.Serialization.JSON
{
    /// <summary>
    /// The attribute used to store classes that inherit from this class and 
    /// make that information availabe during de/serialization.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.24.0 (Newtonsoft.Json v9.0.0.0)")]
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class JsonInheritanceAttribute : System.Attribute
    {
        /// <summary>
        /// Construct the attribute from a string key and the type.
        /// </summary>
        /// <param name="key">The discriminator of the type being de/serialized.</param>
        /// <param name="type">The type to pair with the key.</param>
        public JsonInheritanceAttribute(string key, System.Type type)
        {
            Key = key;
            Type = type;
        }

        /// <summary>
        /// The key (discriminator) of the type that inherits from this class.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The type that inherits from this class with the given key.
        /// </summary>
        public System.Type Type { get; }
    }
}