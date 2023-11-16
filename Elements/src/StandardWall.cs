using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A wall defined by a planar curve, a height, and a thickness.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/WallTests.cs?name=example)]
    /// </example>
    public class StandardWall : Wall
    {
        /// <summary>
        /// The center line of the wall.
        /// </summary>
        public Line CenterLine { get; }

        /// <summary>
        /// The thickness of the wall.
        /// </summary>
        public double Thickness { get; set; }

        /// <summary>
        /// The height of the wall.
        /// </summary>
        public new double Height { get; protected set; }


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
        /// Add an Opening in the Wall. The Opening x and y sets the position relative to the Wall's Centerline.Start point
        /// in the wall elevation coordinate system.  The x and y will position origin of the rectangular opening.
        /// </summary>
        /// <param name="width">The width of the opening.</param>
        /// <param name="height">The height of the opening.</param>
        /// <param name="x">The distance to the center of the opening along the center line of the wall.</param>
        /// <param name="y">The height to the center of the opening along the center line of the wall.</param>
        /// <param name="depthFront">The depth of the opening along the opening's +Z axis.</param>
        /// <param name="depthBack">The depth of the opening along the opening's -Z axis.</param>
        public Opening AddOpening(double width, double height, double x, double y, double depthFront = 1.0, double depthBack = 1.0)
        {
            var openingTransform = GetOpeningTransform(x, y);
            var o = new Opening(Polygon.Rectangle(width, height), Vector3.ZAxis, depthFront, depthBack, openingTransform);
            this.Openings.Add(o);
            return o;
        }

        /// <summary>
        /// Add an Opening in the Wall. The Opening x and y sets the position relative to the Wall's Centerline.Start point
        /// in the wall elevation coordinate system.  The x and y will position the origin of the polygon opening.
        /// </summary>
        /// <param name="perimeter">The perimeter of the opening.</param>
        /// <param name="x">The distance to the origin of the perimeter opening along the center line of the wall.</param>
        /// <param name="y">The height to the origin of the perimeter along the center line of the wall.</param>
        /// <param name="depthFront">The depth of the opening along the opening's +Z axis.</param>
        /// <param name="depthBack">The depth of the opening along the opening's -Z axis.</param>
        public Opening AddOpening(Polygon perimeter, double x, double y, double depthFront = 1.0, double depthBack = 1.0)
        {
            var openingTransform = GetOpeningTransform(x, y);
            var o = new Opening(perimeter, Vector3.ZAxis, depthFront, depthBack, openingTransform);
            this.Openings.Add(o);
            return o;
        }

        private Transform GetOpeningTransform(double x, double y)
        {
            var xAxis = this.CenterLine.Direction();
            var outOfPlane = xAxis.Cross(Vector3.ZAxis);
            var openingTransform = new Transform(this.CenterLine.Start + xAxis * x + Vector3.ZAxis * y - outOfPlane, xAxis, xAxis.Cross(Vector3.ZAxis));
            return openingTransform;
        }

        /// <summary>
        /// Update solid operations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();
            var e1 = this.CenterLine.Offset(this.Thickness / 2, false);
            var e2 = this.CenterLine.Offset(this.Thickness / 2, true);
            var profile = new Polygon(new[] { e1.Start, e1.End, e2.End, e2.Start });
            this.Representation.SolidOperations.Add(new Extrude(profile, this.Height, Vector3.ZAxis, false));
        }

        /// <summary>
        /// Creates an opening that suits <paramref name="door"/>.
        /// </summary>
        /// <param name="door">Properties of <paramref name="door"/> will be used to create an opening.</param>
        public void AddDoorOpening(Door door)
        {
            var halfThickness = 0.5 * Thickness;
            var opening = door.CreateDoorOpening(halfThickness, halfThickness, false);
            Openings.Add(opening);
        }
    }
}