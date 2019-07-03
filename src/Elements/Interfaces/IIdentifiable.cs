#pragma warning disable CS1591

using System;

namespace Elements.Interfaces
{
    /// <summary>
    /// The interface for all elements which can be identified with a unique identifier.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// The unique identifier of the Element.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// A human-readable name for the Element.
        /// </summary>
        string Name { get; }
    }
}