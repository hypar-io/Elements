using System;
using Elements.Geometry;

namespace Elements.Flow
{
    public partial class Node
    {
        public override string ToString()
        {
            return $"Node:{Position.X.ToString("0.0")}, {Position.Y.ToString("0.0")}, {Position.Z.ToString("0.0")}";
        }
    }
}