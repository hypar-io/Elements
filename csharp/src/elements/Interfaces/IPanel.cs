using Hypar.Geometry;
using System.Collections.Generic;

namespace Hypar.Interfaces
{
    public interface IPanel : IGeometry3D
    {
        IList<Vector3> Perimeter{get;}
    }
}