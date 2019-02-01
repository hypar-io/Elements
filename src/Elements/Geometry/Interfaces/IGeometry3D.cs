#pragma warning disable CS1591

using Elements.Geometry.Solids;

namespace Elements.Geometry.Interfaces
{
    public interface IGeometry3D
    {
        Solid[] Geometry { get; }
    }
}