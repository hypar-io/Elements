using System;
namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// An object that can return a transformed copy of itself 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITransformable<T>
    {
        /// <summary>
        /// Create a transformed copy of this ITransformable
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        T Transformed(Transform transform);
    }
}
