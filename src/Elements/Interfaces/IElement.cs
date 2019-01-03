#pragma warning disable CS1591
using Elements.Geometry;
using Elements.Geometry.Interfaces;

namespace Elements.Interfaces
{
    public interface IElement : IIdentifiable, ITransformable, IPropertySet
    {
        /// <summary>
        /// A type descriptor for use in deserialization.
        /// </summary>
        string Type { get; }
    }
}