using Hypar.Geometry;

namespace Hypar.Elements
{
    public interface IMeshProvider
    {
        Mesh Tessellate();
    }
}