#pragma warning disable CS1591

namespace Elements.Interfaces
{
    public interface IExtrude: IGeometry3D, IProfileProvider
    {
        double Thickness{get;}
    }
}