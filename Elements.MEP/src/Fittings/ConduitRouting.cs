using System;
using Elements.Flow;
using Elements.Geometry;

namespace Elements.Fittings
{
    public class ConduitRouting : FittingTreeRouting
    {
        private const double lengthMultiplier = 0.1;

        public ConduitRouting(Tree collection) : base(collection) { }

        public ConduitRouting Clone()
        {
            return MemberwiseClone() as ConduitRouting;
        }

        /// <inheritdoc/>
        public override Fitting ChangeDirection(Connection incoming, Connection outgoing)
        {
            var larger = Math.Max(incoming.Diameter, outgoing.Diameter);
            var diameter = !larger.ApproximatelyEquals(0) ? larger : DefaultDiameter;
            return CreateElbow(diameter, incoming.End.Position, incoming.Direction().Negate(), outgoing.Direction());
        }

        public override Elbow CreateElbow(double diameter, Vector3 position, Vector3 startDirection, Vector3 endDirection)
        {
            return new Elbow(position, startDirection, endDirection, diameter * lengthMultiplier, diameter, DefaultFittingMaterial, diameter);
        }
    }
}