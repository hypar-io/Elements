using System;

namespace Elements.Annotations
{
    /// <summary>
    /// An annotation element which may be linked to the value of a property of
    /// another element.
    /// </summary>
    public interface IOverrideLinked
    {
        /// <summary>
        /// The property to which this annotation is linked.
        /// </summary>
        /// <value>Null if not linked, otherwise, a LinkedPropertyInfo
        /// describing the linked element, property, and override
        /// details.</value>
        LinkedPropertyInfo LinkedProperty { get; set; }
    }

    /// <summary>
    /// Information describing a linked property for this annotation. For
    /// instance, a dimension which represents the width of a mass would store
    /// linked property info describing the box element's "Width" property.
    /// </summary>
    public class LinkedPropertyInfo
    {
        /// <summary>
        /// The id of the element that this property is linked to. 
        /// </summary>
        public Guid ElementId { get; set; }

        /// <summary>
        /// The name of the override containing the linked property.
        /// </summary>
        public string OverrideName { get; set; }

        /// <summary>
        /// The name of the linked property.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// If true, this annotation should only be visible when the linked element is selected.
        /// </summary>

        public bool VisibleOnlyOnSelection { get; set; }

        /// <summary>
        /// If the linked property is an array, this is the index of the value
        /// in the array this annotation represents.
        /// </summary>
        public int? Index { get; set; }
    }
}