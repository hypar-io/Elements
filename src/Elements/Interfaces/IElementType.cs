#pragma warning disable CS1591

namespace Elements.Interfaces
{
    /// <summary>
    /// Interface implemented by classes which provide an element type.
    /// </summary>
    /// <typeparam name="TElementType"></typeparam>
    public interface IElementType<TElementType>
    {
        /// <summary>
        /// The ElementType provided by this instance.
        /// </summary>
        TElementType ElementType{get;}
    }
}