#pragma warning disable CS1591

namespace Elements.Geometry.Interfaces
{
    public interface IExtrudeAlongCurve : IBRep, IProfileProvider
    {
        double StartSetback{get;}
        double EndSetback{get;}
        ICurve Curve{get;}
    }
}