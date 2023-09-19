using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements
{
    public partial class RoofDrain
    {
        public double ConnectorLength { get; set; }
        public double ConnectorOuterDiameter { get; set; }
        public Vector3 HorizontalConnectorVector { get; set; }
        public override void UpdateRepresentations()
        {
            this.Representation = new Geometry.Representation(new List<SolidOperation>());
            var cylinder = new Extrude(new Circle(new Vector3(), Diameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS), .1, Vector3.ZAxis, false);
            this.Representation.SolidOperations.Add(cylinder);
            if (ConnectorOuterDiameter != 0 && ConnectorLength != 0)
            {
                var profile =
                    new Circle(Vector3.Origin, ConnectorOuterDiameter / 2).ToPolygon(
                        FlowSystemConstants.CIRCLE_SEGMENTS);
                var elbowPoint = Vector3.Origin + Vector3.ZAxis.Negate() * ConnectorLength;
                var connectorPoint = elbowPoint + HorizontalConnectorVector;

                var vertices = new List<Vector3>{Vector3.Origin, connectorPoint};
                
                if (HorizontalConnectorVector.Length() > 2 * Vector3.EPSILON)
                {
                    vertices.Insert(1, elbowPoint);
                }

                var polyline = new Polyline(vertices);
                var connectorPipe = new Sweep(profile, polyline, 0, 0, 0, false);
                
                Representation.SolidOperations.Add(connectorPipe);
            }
        }
    }
}