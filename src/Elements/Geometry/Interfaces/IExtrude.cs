#pragma warning disable CS1591

namespace Elements.Geometry.Interfaces
{
    public interface IExtrude: IBRep, IProfileProvider
    {
        double Depth{get;}
    }
}