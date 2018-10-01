using System.Collections.Generic;

namespace Hypar.Geometry
{
    public interface ITessellateCurves
    {
        IList<IList<Vector3>> Curves();
    }
}