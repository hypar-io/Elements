using System;

namespace Elements.Serialization
{
    /// <summary>
    /// The type that deserializer will try to use if it knows nothing about elements type. It should be one of the types that are included
    /// to the Elements library.
    ///
    /// Use this attribute if your class inherits one of the Elements library classes
    /// </summary>
    public class DiscriminatorFallbackAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of DiscriminatorFallbackAttribute class
        /// </summary>
        /// <param name="type">The type that will be used during deserialization</param>
        public DiscriminatorFallbackAttribute(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets type that can be used during deserialization as base type
        /// </summary>
        public Type Type { get; }
    }
}