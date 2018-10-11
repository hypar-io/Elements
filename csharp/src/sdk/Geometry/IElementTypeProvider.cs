using Hypar.Elements;

namespace Hypar.Geometry
{
    public interface IElementTypeProvider<TElementType>
    {
        /// <summary>
        /// The ElementType provided by this instance.
        /// </summary>
        TElementType ElementType{get;}
    }
}