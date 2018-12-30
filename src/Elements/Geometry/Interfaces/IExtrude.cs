#pragma warning disable CS1591

namespace Elements.Geometry.Interfaces
{
    public interface IExtrude: IProfileProvider, IBRep
    {
        double Thickness{get;}
    }
}