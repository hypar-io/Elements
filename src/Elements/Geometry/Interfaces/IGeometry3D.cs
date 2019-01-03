#pragma warning disable CS1591

using Elements.Geometry;
using System.Collections.Generic;

namespace Elements.Geometry.Interfaces
{
    public interface IGeometry3D
    {
        IBRep[] Geometry { get; }
    }
}