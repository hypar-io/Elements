using Hypar.Geometry;

namespace Hypar.Elements
{
    public interface IRepresent<T>: ITessellate<T>
    {
        /// <summary>
        /// The element's material.
        /// </summary>
        /// <value></value>
        Material Material{get;}
        
        /// <summary>
        /// The element's transform.
        /// </summary>
        /// <value></value>
        Transform Transform {get;}
        
        T Tessellate();
    }
}