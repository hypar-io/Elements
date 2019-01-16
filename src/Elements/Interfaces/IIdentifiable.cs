#pragma warning disable CS1591

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
        long Id { get; }

        /// <summary>
        /// A human-readable name for the Element.
        /// </summary>
        string Name { get; }
    }
}