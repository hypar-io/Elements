using Hypar.Elements;

namespace Hypar.Geometry
{
    /// <summary>
    /// Interface implemented by classes which provide and ElementType.
    /// </summary>
    /// <typeparam name="TElementType"></typeparam>
    public interface IElementTypeProvider<TElementType>
    {
        /// <summary>
        /// The ElementType provided by this instance.
        /// </summary>
        TElementType ElementType{get;}
    }
}