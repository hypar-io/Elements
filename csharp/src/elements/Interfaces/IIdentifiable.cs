namespace Elements.Interfaces
{
    /// <summary>
    /// The interface for all elements which can be identified with a unique identifier.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// The unique identifier of the element.
        /// </summary>
        long Id{get;}
    }
}