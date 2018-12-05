#pragma warning disable CS1591

using Elements.Geometry;

namespace Elements.Interfaces
{
    public interface IExtrudeAlongCurve : IGeometry3D, IProfileProvider
    {
        double StartSetback{get;}
        double EndSetback{get;}
        ICurve Curve{get;}
    }
}