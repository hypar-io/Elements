namespace Hypar.Geometry
{
    /// <summary>
    /// The interface for all elements which can be identified with a string identifier.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// The unique identifier of the element.
        /// </summary>
        string Id{get;}
    }
}