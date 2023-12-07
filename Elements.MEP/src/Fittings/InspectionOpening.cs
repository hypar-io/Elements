using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Fittings
{
    public class InspectionOpening : Coupler
    {
        public InspectionOpening(Vector3 position,
                                 Vector3 direction,
                                 Vector3 sideDirection,
                                 double length,
                                 double topLength,
                                 double diameter,
                                 Material material = null) :
            base("Inspection Opening", position, direction, length, diameter, material)
        {
            if (topLength > length)
            {
                throw new Exception("Opening length is not inside coupler range.");
            }

            //Align the side direction with inspection opening main direction
            var cross = direction.Cross(sideDirection);
            var alignedSideDirection = cross.Cross(direction);
            this.SideDirection = alignedSideDirection;
            this.TopLength = topLength;
        }

        [JsonConstructor]
        public InspectionOpening(Vector3 @sideDirection,
                                 double @topLength,
                                 Port @start,
                                 Port @end,
                                 double @diameter,
                                 PressureCalculationCoupler @pressureCalculations,
                                 string @couplerType, bool @canBeMoved,
                                 FittingLocator @componentLocator,
                                 Transform @transform, Material @material,
                                 Representation @representation,
                                 bool @isElementDefinition,
                                 System.Guid @id,
                                 string @name) :
            base(@start,
                 @end,
                 @diameter,
                 @pressureCalculations,
                 @couplerType, @canBeMoved,
                 @componentLocator,
                 @transform,
                 @material,
                 @representation,
                 @isElementDefinition,
                 @id,
                 @name)
        {
            this.SideDirection = @sideDirection;
            this.TopLength = @topLength;
        }

        public Vector3 SideDirection { get; private set; }

        public double TopLength { get; private set; }

        public override void UpdateRepresentations()
        {
            var radius = Start.Diameter / 2;

            var line = new Line(End.Position - Transform.Origin, Start.Position - Transform.Origin);
            var profile = new Circle(Vector3.Origin, radius).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var main = new Sweep(profile, line, 0, 0, 0, false);

            var middlePoint = line.Mid();
            var sideLine = new Line(middlePoint, middlePoint + SideDirection * radius * 1.5);
            var side = new Sweep(profile, sideLine, 0, 0, 0, false);

            var arrows = this.Start.GetArrow(this.Transform.Origin).Concat(End.GetArrow(Transform.Origin));
            Representation = new Representation(new List<SolidOperation> { main, side }.Concat(arrows).Concat(GetExtensions()).ToList());
        }

        public override Transform GetRotatedTransform()
        {
            var zAxis = End.Direction.Cross(SideDirection).Unitized();
            var t = new Transform(Vector3.Origin, End.Direction, zAxis);
            return t;
        }
    }
}