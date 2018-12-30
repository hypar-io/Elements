#pragma warning disable CS1591

namespace Elements.Geometry.Interfaces
{
    public interface IExtrudeAlongCurve : IProfileProvider, IBRep
    {
        double StartSetback{get;}
        double EndSetback{get;}
        ICurve Curve{get;}
    }
}