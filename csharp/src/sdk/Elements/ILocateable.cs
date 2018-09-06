namespace Hypar.Elements
{
    /// <summary>
    /// Represents an object which has geometry which defines its location.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILocateable<T>
    {
        /// <summary>
        /// The location of the object.
        /// </summary>
        /// <value></value>
        T Location{get;}
    }
}