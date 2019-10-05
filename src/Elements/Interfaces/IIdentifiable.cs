#pragma warning disable CS1591

using System;

namespace Elements.Interfaces
{
    /// <summary>
    /// An object which is identified 
    /// with a unique identifier and a name.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// The unique identifier of the object.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// A name for the object.
        /// </summary>
        string Name { get; }
    }
}