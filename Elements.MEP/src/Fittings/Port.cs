using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements.Fittings
{
    public partial class Port
    {
        public static bool ShowArrows = false;

        public bool IsTrunk { get; set; }

        // TODO deprecate and remove this field.  It was used to control StraightSegment diameter
        // but that is better handled by the Connection diameter with the new Routing.PipeSizeShouldMatchConnection option.
        public bool PreferReducer { get; set; } = false;

        public override string ToString()
        {
            return $"Diameter: {Diameter} Position: {Position.X.ToString("0.00")}, {Position.Y.ToString("0.00")}, {Position.Z.ToString("0.00")}";
        }

        public Port(Vector3 position, Vector3 direction, double diameter)
        {
            this.Position = position;
            this.Direction = direction;
            this.Diameter = diameter;
        }

        /// <summary>
        /// Searches the fitting or segment for the closest port to this one.
        /// </summary>
        /// <param name="fitting"></param>
        /// <param name="closestDistance"></param>
        /// <returns></returns>
        internal Port GetClosestPort(ComponentBase fitting, out double closestDistance)
        {
            Port best = null;
            closestDistance = double.MaxValue;
            foreach (var conn in fitting.GetPorts())
            {
                var currentDistance = conn.Position.DistanceTo(this.Position);
                if (currentDistance < closestDistance)
                {
                    best = conn;
                    closestDistance = currentDistance;
                }
            }
            return best;
        }

        public bool IsComplimentaryConnector(Port other, double positionTolerance = Vector3.EPSILON, double angleTolerance = 0.5)
        {
            if (!other.Position.IsAlmostEqualTo(Position, positionTolerance))
            {
                return false;
            }
            
            var angle = Direction.AngleTo(other.Direction);

            return angle.ApproximatelyEquals(180, angleTolerance);        
        }

        public bool IsIdenticalConnector(Port other, double positionTolerance = Vector3.EPSILON, double angleTolerance = 0.5)
        {
            if (!other.Position.IsAlmostEqualTo(Position, positionTolerance))
            {
                return false;
            }
            
            var angle = Direction.AngleTo(other.Direction);

            return angle.ApproximatelyEquals(0, angleTolerance);
        }

        public Sweep[] GetArrow(Vector3 relativeTo, double arrowLineLength = 0.1)
        {
            var arrayHeadLength = 0.01;
            if (ShowArrows)
            {
                var transformedOrigin = Position - relativeTo;
                var arrowProfile = new Circle(Vector3.Origin, 0.01).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
                var arrowLine = new Line(transformedOrigin, transformedOrigin + Direction * arrowLineLength);
                var headProfile = new Circle(Vector3.Origin, 0.02).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
                var headLine = new Line(transformedOrigin + Direction * arrowLineLength, transformedOrigin + Direction * (arrowLineLength + arrayHeadLength));
                var shaft = new Sweep(arrowProfile, arrowLine, 0, 0, 0, false);
                var head = new Sweep(headProfile, headLine, 0, 0, 0, false);
                return new Sweep[] { shaft, head };
            }
            else
            {
                return Array.Empty<Sweep>();
            }
        }

        //
        // Summary:
        //     A collection of additional properties.
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties { get; set; } = new Dictionary<string, object>();

        public Port Clone()
        {
            var clone = MemberwiseClone() as Port;
            var additionalPropertiesClone = new Dictionary<string, object>(clone.AdditionalProperties);
            clone.AdditionalProperties = additionalPropertiesClone;
            return clone;
        }

        internal void AddFlow(double leafFlow)
        {
            if (Flow == null)
            {
                Flow = new Flow(0, 0, 0, 0);
            }
            Flow.FlowRate += leafFlow;
        }
    }
}