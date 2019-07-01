#pragma warning disable CS1591

using Elements.Geometry.Solids;

namespace Elements.Geometry.Interfaces
{
    public interface ISolid
    {
        Solid GetUpdatedSolid();
    }
}