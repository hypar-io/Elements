using Elements.Geometry;
using System.Collections.Generic;

namespace Elements.Interfaces
{
    public interface IPanel : IGeometry3D
    {
        IList<Vector3> Perimeter{get;}
    }
}