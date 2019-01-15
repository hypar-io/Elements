#pragma warning disable CS1591

namespace Elements.Geometry.Interfaces
{
    public interface IExtrudeInDirection : IBRep, IProfileProvider
    {
        double Depth{get;}
        Vector3 Direction{get;}
    }
}