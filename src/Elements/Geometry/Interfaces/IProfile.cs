using Elements.Geometry;
using Elements.Interfaces;
using System.Collections.Generic;

namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// A Profile consisting of an outer Perimeter and a collection of Voids.
    /// </summary>
    public interface IProfile : IIdentifiable
    {
        /// <summary>
        /// The outer perimeter of the Profile.
        /// </summary>
        Polygon Perimeter { get; }

        /// <summary>
        /// A collection of Polygons representing voids in the Profile.
        /// </summary>
        Polygon[] Voids { get; }

        /// <summary>
        /// The area of the Profile.
        /// </summary>
        double Area();

        /// <summary>
        /// Get a new Profile which is the reverse of this Profile.
        /// </summary>
        IProfile Reversed();
    }
}