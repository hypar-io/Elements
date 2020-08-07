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
    /// [!code-csharp[Main](../../test/Elements.Tests/WallTests.cs?name=example)]
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
        public double Thickness { get; set;}

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
        /// <param name="representation">The wall's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the wall.</param>
        /// <param name="name">The name of the wall.</param>
        /// <exception>Thrown when the height of the wall is less than or equal to zero.</exception>
        /// <exception>Thrown when the Z components of wall's start and end points are not the same.</exception>
        public StandardWall(Line centerLine,
                            double thickness,
                            double height,
                            Material material = null,
                            Transform transform = null,
                            Representation representation = null,
                            bool isElementDefinition = false,
                            Guid id = default(Guid),
                            string name = null) : base(transform != null ? transform : new Transform(),
                                                       material != null ? material : BuiltInMaterials.Concrete,
                                                       representation != null ? representation : new Representation(new List<SolidOperation>()),
                                                       isElementDefinition,
                                                       id != default(Guid) ? id : Guid.NewGuid(),
                                                       name)
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
            this.Thickness = thickness;
        }

        /// <summary>
        /// Update solid operations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            // Construct a transform whose X axis is the centerline of the wall.
            // The wall is described as if it's lying flat in the XY plane of that Transform.
            var d = this.CenterLine.Direction();
            var z = d.Cross(Vector3.ZAxis);
            var wallTransform = new Transform(this.CenterLine.Start, d, z);

            var wallProfile = new Profile(Polygon.Rectangle(Vector3.Origin, new Vector3(this.CenterLine.Length(), this.Height)));

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
                    wallProfile.Clip(holeProfiles);
                }
            }

            // Set the wall's profile to the wallProfile created here
            // as we will use it for the solid op below. 
            this.Profile = wallTransform.OfProfile(wallProfile);
            this.Representation.SolidOperations.Clear();

            // Transform the wall profile to be "standing up".
            this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, z, false));
        }
    }
}