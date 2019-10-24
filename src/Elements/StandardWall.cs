using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Interfaces;

namespace Elements
{
    /// <summary>
    /// A wall defined by a planar curve, a height, and a thickness.
    /// </summary>
    /// <example>
    /// <code source="../../test/Examples/WallExample.cs"/>
    /// </example>
    [UserElement]
    public class StandardWall : Wall, IHasOpenings
    {
        /// <summary>
        /// The center line of the wall.
        /// </summary>
        public Line CenterLine { get; }

        /// <summary>
        /// The thickness of the wall.
        /// </summary>
        public double Thickness { get; protected set;}

        /// <summary>
        /// A collection of openings in the floor.
        /// </summary>
        public List<Opening> Openings{ get; } = new List<Opening>();
        
        /// <summary>
        /// Construct a wall along a line.
        /// </summary>
        /// <param name="centerLine">The center line of the wall.</param>
        /// <param name="thickness">The thickness of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="material">The wall's material.</param>
        /// <param name="transform">The transform of the wall.
        /// This transform will be concatenated to the transform created to describe the wall in 2D.</param>
        /// <param name="id">The id of the wall.</param>
        /// <param name="name">The name of the wall.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the height of the wall is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the Z components of wall's start and end points are not the same.</exception>
        public StandardWall(Line centerLine,
                            double thickness,
                            double height,
                            Material material = null,
                            Transform transform = null,
                            Guid id = default(Guid),
                            string name = null) : base(id, name, transform)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The wall could not be created. The height of the wall provided, {height}, must be greater than 0.0.");
            }

            if (centerLine.Start.Z != centerLine.End.Z)
            {
                throw new ArgumentException("The wall could not be created. The Z component of the start and end points of the wall's center line must be the same.");
            }

            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The provided thickness ({thickness}) was less than or equal to zero.");
            }

            this.CenterLine = centerLine;
            this.Height = height;

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
            this.Thickness = thickness;
            this.Material = material != null ? material : BuiltInMaterials.Concrete;
        }

        /// <summary>
        /// Update solid operations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            if(this.Openings.Count > 0)
            {
                this.Openings.ForEach(o=>o.UpdateRepresentations());
                
                // Find all the void ops which point in the same direction.
                var holes = this.Openings.SelectMany(o=>o.Representation.SolidOperations.
                                                        Where(op=>op is Extrude && op.IsVoid == true).
                                                        Cast<Extrude>().
                                                        Where(ex=>ex.Direction.IsAlmostEqualTo(Vector3.ZAxis)));
                if(holes.Any())
                {
                    var holeProfiles = holes.Select(ex=>ex.Profile);
                    this.Profile.Clip(holeProfiles);
                }
            }
            this.Representation.SolidOperations.Clear();
            this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, Vector3.ZAxis));
        }
    }
}