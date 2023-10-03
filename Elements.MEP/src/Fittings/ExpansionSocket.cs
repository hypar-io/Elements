using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Fittings
{
    public class ExpansionSocket : Coupler
    {
        public ExpansionSocket(Vector3 position,
                               Vector3 direction,
                               double length,
                               double diameter,
                               double insertionDepth,
                               Material material = null) :
            base("Expansion Socket",
                 position + direction * insertionDepth, //Shift Start connector down by collar length
                 direction,
                 length - insertionDepth, //End is calculated from length, adjust together with Start
                 diameter,
                 material)
        {
            if (length < insertionDepth)
            {
                throw new Exception("Socket length is smaller than insertion depth");
            }
            this.InsertionDepth = insertionDepth;
        }

        [Newtonsoft.Json.JsonConstructor]
        public ExpansionSocket(double @insertionDepth,
                               Port @start,
                               Port @end,
                               double @diameter,
                               PressureCalculationCoupler @pressureCalculations,
                               string @couplerType,
                               bool @canBeMoved,
                               FittingLocator @componentLocator,
                               Transform @transform,
                               Material @material,
                               Representation @representation,
                               bool @isElementDefinition,
                               System.Guid @id,
                               string @name) :
            base(@start,
                 @end,
                 @diameter,
                 @pressureCalculations,
                 @couplerType, @canBeMoved,
                 @componentLocator, @transform,
                 @material,
                 @representation,
                 @isElementDefinition,
                 @id,
                 @name)
        {
            this.InsertionDepth = @insertionDepth;
        }

        public double InsertionDepth { get; private set; }

        public override void UpdateRepresentations()
        {
            var radius = Start.Diameter / 2;
            var localStart = Start.Position - Transform.Origin;
            var localEnd = End.Position - Transform.Origin;

            var line = new Line(localEnd, localStart);
            var profile = new Circle(Vector3.Origin, radius).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var main = new Sweep(profile, line, 0, 0, 0, false);

            var direction = (localStart - localEnd).Unitized();
            line = new Line(localStart, localStart + direction * InsertionDepth);
            var outetProfile = new Circle(Vector3.Origin, radius * 1.2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var collarProfile = new Profile(outetProfile, profile);
            var collar = new Sweep(collarProfile, line, 0, 0, 0, false);

            var arrows = this.Start.GetArrow(this.Transform.Origin).Concat(End.GetArrow(Transform.Origin));
            Representation = new Representation(new List<SolidOperation> { main, collar }.Concat(arrows).Concat(GetExtensions()).ToList());
        }
    }
}
