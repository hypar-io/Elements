using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Flow
{
    public partial class Connection
    {
        public const double DIAMETER_INSET = 0.001;
        public const double DEFAULT_CONNECTION_DIAMETER = 0.01;

        public ConnectionLocator ComponentLocator { get; set; }

        public bool? IsLoop { get; set; }

        public override void UpdateRepresentations()
        {
            if (this.Representation == null)
            {
                this.Representation = new Representation(new List<SolidOperation>());
            }
            this.Representation.SolidOperations = new List<SolidOperation>();
            var circle = new Circle(Diameter > 0 ? (Diameter - DIAMETER_INSET) / 2 : DEFAULT_CONNECTION_DIAMETER).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);

            var s = new Sweep(circle, Path(), 0, 0, 0, false);
            this.Representation.SolidOperations.Add(s);
        }

        public Connection(Node start, Node end, Guid id, string name)
            : this(start, end, 0, 0)
        {
            Id = id;
            Name = name;
        }

        public Vector3 Direction()
        {
            return (this.End.Position - this.Start.Position).Unitized();
        }

        public double Length()
        {
            return (this.End.Position - this.Start.Position).Length();
        }

        public Line Path()
        {
            return new Line(this.Start.Position, this.End.Position);
        }

        public override string ToString()
        {
            return $"Connection-Diameter: {this.Diameter}m Start: {this.Start.Position} Direction: {this.Direction()} End: {this.End.Position}";
        }
    }
}