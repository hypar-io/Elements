using System;
namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// An object that can return a transformed copy of itself 
    /// </summary>
    /// <typeparam name="T">The type of object to be transformed</typeparam>
    public interface ITransformable<T>
    {
        /// <summary>
        /// Create a transformed copy of this ITransformable
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>A transformed copy of the object</returns>
        T Transformed(Transform transform);
    }
}
