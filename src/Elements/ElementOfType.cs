using Elements.Serialization;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// Base class for all Elements which have an ElementType.
    /// </summary>
    public abstract class ElementOfType<TElementType> : Element
    {  
        /// <summary>
        /// The ElementType of the Element.
        /// </summary>
        public TElementType ElementType {get; protected set;}
    }
}