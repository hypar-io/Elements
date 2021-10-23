using System;

namespace Elements
{
    /// <summary>
    /// This attribute is used to mark a property of an element as having
    /// come from a dependency.
    /// </summary>
    public class DependencySourceAttribute : Attribute
    {
        /// <summary>
        /// The source of the property.
        /// </summary>
        public string source;

        /// <summary>
        /// Construct an attribute with the given dependency source.
        /// </summary>
        /// <param name="source">The dependency this property comes from.</param>
        public DependencySourceAttribute(string source)
        {
            this.source = source;
        }
    }
}