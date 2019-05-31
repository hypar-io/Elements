using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Hypar.Elements.Interfaces;

namespace Elements
{
    /// <summary>
    /// A wall defined by a planar curve, a height, and a thickness.
    /// </summary>
    public class StandardWall : Wall, IHasOpenings
    {
        /// <summary>
        /// The center line of the wall.
        /// </summary>
        public Line CenterLine { get; }

        /// <summary>
        /// An array of openings in the wall.
        /// </summary>
        public List<Opening> Openings{ get; protected set;}

        /// <summary>
        /// Extrude to both sides?
        /// </summary>
        public override bool BothSides => true;

        /// <summary>
        /// Construct a wall along a line.
        /// </summary>
        /// <param name="centerLine">The center line of the wall.</param>
        /// <param name="elementType">The wall type of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="openings">A collection of Openings in the wall.</param>
        /// <param name="transform">The transform of the wall.
        /// This transform will be concatenated to the transform created to describe the wall in 2D.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the height of the wall is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the Z components of wall's start and end points are not the same.</exception>
        public StandardWall(Line centerLine, WallType elementType, double height, List<Opening> openings = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The wall could not be created. The height of the wall provided, {height}, must be greater than 0.0.");
            }

            if (centerLine.Start.Z != centerLine.End.Z)
            {
                throw new ArgumentException("The wall could not be created. The Z component of the start and end points of the wall's center line must be the same.");
            }

            this.CenterLine = centerLine;
            this.Height = height;
            this.ElementType = elementType;
            this.Openings = openings != null ? openings : new List<Opening>();
            
            // Construct a transform whose X axis is the centerline of the wall.
            // The wall is described as if it's lying flat in the XY plane of that Transform.
            var d = centerLine.Direction();
            var z = d.Cross(Vector3.ZAxis);
            var wallTransform = new Transform(centerLine.Start, d, z);
            this.Transform = wallTransform;
            if(transform != null) 
            {
                wallTransform.Concatenate(transform);
            }

            this.Profile = new Profile(Polygon.Rectangle(Vector3.Origin, new Vector3(centerLine.Length(), height)));
            this.ExtrudeDepth = this.Thickness();
            this.ExtrudeDirection = Vector3.ZAxis;
        }
    }
}