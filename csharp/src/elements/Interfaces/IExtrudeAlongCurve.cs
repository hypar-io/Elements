using Hypar.Geometry;

namespace Hypar.Interfaces
{
    public interface IExtrudeAlongCurve : IGeometry3D, IProfileProvider
    {
        double StartSetback{get;}
        double EndSetback{get;}
        ICurve Curve{get;}
    }
}