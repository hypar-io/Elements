using Elements.ElementTypes;
using Elements.Geometry;
using Elements.Interfaces;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// A collection of beams within a perimeter.
    /// </summary>
    public class BeamSystem : IAggregateElements
    {
        /// <summary>
        /// The Beams that make up the BeamSystem.
        /// </summary>
        public List<Element> Elements { get; }

        /// <summary>
        /// Construct a BeamSystem between two edges.
        /// </summary>
        /// <param name="count">The number of beams to create.</param>
        /// <param name="framingType">The structural framing type to be used for all beams.</param>
        /// <param name="edge1">The first edge of the system.</param>
        /// <param name="edge2">The second edge of the system.</param>
        public BeamSystem(int count, StructuralFramingType framingType, Line edge1, Line edge2)
        {
            this.Elements = new List<Element>();
            CreateBeamsBetweenEdges(edge1, edge2, count, framingType);
        }

        /// <summary>
        /// Construct a beam system under a slab.
        /// </summary>
        /// <param name="floor">The floor under which to create beams.</param>
        /// <param name="count">The number of beams to create.</param>
        /// <param name="framingType">The structural framing type to be used for all beams.</param>
        public BeamSystem(Floor floor, int count, StructuralFramingType framingType)
        {
            this.Elements = new List<Element>();
            var edges = floor.Profile.Perimeter.Segments();
            var e1 = edges[0];
            var e2 = edges[2].Reversed();
            var bbox = new BBox3(framingType.Profile);
            var depth = bbox.Max.Y - bbox.Min.Y;
            var edge1 = new Line(new Vector3(e1.Start.X, e1.Start.Y, floor.Elevation - depth / 2), new Vector3(e1.End.X, e1.End.Y, floor.Elevation - depth / 2));
            var edge2 = new Line(new Vector3(e2.Start.X, e2.Start.Y, floor.Elevation - depth / 2), new Vector3(e2.End.X, e2.End.Y, floor.Elevation - depth / 2));
            CreateBeamsBetweenEdges(edge1, edge2, count, framingType);
        }

        private void CreateBeamsBetweenEdges(Line edge1, Line edge2, int count, StructuralFramingType framingType)
        {
            var div = 1.0 / ((double)count + 1);
            for (var i = 0; i < count; i++)
            {
                var t = i * div + div;
                var a = edge1.PointAt(t);
                var b = edge2.PointAt(t);
                var line = new Line(a, b);
                
                var beam = new Beam(line, framingType);
                this.Elements.Add(beam);
            }
        }
    }
}