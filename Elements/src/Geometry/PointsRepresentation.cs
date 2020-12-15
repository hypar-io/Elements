using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    public partial class PointsRepresentation
    {
        /// <summary>
        /// Create a points representation.
        /// </summary>
        /// <param name="points">A collection of points.</param>
        /// <param name="material">The material to apply to the points.</param>
        public PointsRepresentation(IList<Vector3> points, Material material) : base(material, Guid.NewGuid(), null)
        {
            this.Points = points;
        }

        /// <summary>
        /// Create a points representation.
        /// </summary>
        /// <param name="material">The material to apply to the points.</param>
        public PointsRepresentation(Material material) : base(material, Guid.NewGuid(), null) { }
    }
}