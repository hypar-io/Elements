namespace Elements.Interfaces
{
    /// <summary>
    /// Implemented by classes which have references that they would
    /// like to serialize by their id.
    /// </summary>
    /// <typeparam name="T">The type of the instance to be referenced.</typeparam>
    public interface IReference<T>
    {   
        /// <summary>
        /// Set the reference.
        /// </summary>
        /// <param name="obj">The object which is referenced.</param>
        void SetReference(T obj);
    }
}