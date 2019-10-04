#pragma warning disable CS1591

using System;
using Elements.ElementTypes;

namespace Elements.Interfaces
{
    /// <summary>
    /// Interface implemented by classes which use an element type.
    /// </summary>
    public interface IElementType<T>: IReference<T>
        where T: ElementType 
    {
        /// <summary>
        /// The element type used by this instance.
        /// </summary>
        T ElementType{get;}

        /// <summary>
        /// The id of the element type.
        /// </summary>
        Guid ElementTypeId{get;}
    }
}