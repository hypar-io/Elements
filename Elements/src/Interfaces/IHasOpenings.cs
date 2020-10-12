#pragma warning disable CS1591

using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Interfaces
{
    public interface IHasOpenings
    {
        /// <summary>
        /// A collection of openings.
        /// </summary>
        List<Opening> Openings { get; }

        /// <summary>
        /// Add an opening.
        /// </summary>
        /// <param name="width">The width of the opening.</param>
        /// <param name="height">The height of the opening.</param>
        /// <param name="x">The distance to the center of the opening along the host's x axis.</param>
        /// <param name="y">The distance to the center of the opening along the host's y axis.</param>
        /// <param name="depthFront">The depth of the opening along the opening's +Z axis.</param>
        /// <param name="depthBack">The depth of the opening along the opening's -Z axis.</param>
        void AddOpening(double width, double height, double x, double y, double depthFront = 1.0, double depthBack = 1.0);

        /// <summary>
        /// Add an opening in the wall.
        /// </summary>
        /// <param name="perimeter">The perimeter of the opening.</param>
        /// <param name="x">The distance to the origin of the perimeter along the host's x axis.</param>
        /// <param name="y">The height to the origin of the perimeter along the host's y axis.</param>
        /// <param name="depthFront">The depth of the opening along the opening's +Z axis.</param>
        /// <param name="depthBack">The depth of the opening along the opening's -Z axis.</param>
        void AddOpening(Polygon perimeter, double x, double y, double depthFront = 1.0, double depthBack = 1.0);
    }
}