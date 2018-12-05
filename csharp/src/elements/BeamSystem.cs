using Elements.Geometry;
using Elements.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Elements
{
    /// <summary>
    /// BeamSystem represents a collection of beams within a perimeter.
    /// </summary>
    public class BeamSystem: IAggregateElement
    {
        /// <summary>
        /// The Beams that make up the BeamSystem.
        /// </summary>
        public List<Element> Elements{get;}

        /// <summary>
        /// Construct a BeamSystem between two edges.
        /// </summary>
        /// <param name="count">The number of Beams to create.</param>
        /// <param name="profile">The Profile to be used for all Beams.</param>
        /// <param name="edge1">The first edge of the system.</param>
        /// <param name="edge2">The second edge of the system.</param>
        /// <param name="material">The Beam material.</param>
        public BeamSystem(int count, Profile profile, Line edge1, Line edge2, Material material = null)
        {
            this.Elements = new List<Element>();
            CreateBeamsBetweenEdges(edge1, edge2, count, profile, material);
        }

        /// <summary>
        /// Construct a beam system under a slab.
        /// </summary>
        /// <param name="floor">The Floor under which to create Beams.</param>
        /// <param name="count">The number of Beams to create.</param>
        /// <param name="profile">The Profile to be used for all Beams.</param>
        /// <param name="material">The Beam material.</param>
        public BeamSystem(Floor floor, int count, Profile profile, Material material = null)
        {
            this.Elements = new List<Element>();
            var edges = floor.Profile.Perimeter.Segments();
            var e1 = edges[0];
            var e2 = edges[2].Reversed();
            var bbox = new BBox3(profile);
            var depth = bbox.Max.Y - bbox.Min.Y;
            var edge1 = new Line(new Vector3(e1.Start.X, e1.Start.Y, floor.Elevation - depth/2), new Vector3(e1.End.X, e1.End.Y, floor.Elevation - depth/2));
            var edge2 = new Line(new Vector3(e2.Start.X, e2.Start.Y, floor.Elevation - depth/2), new Vector3(e2.End.X, e2.End.Y, floor.Elevation - depth/2));
            CreateBeamsBetweenEdges(edge1, edge2, count, profile, material);
        }

        private void CreateBeamsBetweenEdges(Line edge1, Line edge2, int count, Profile profile, Material material)
        {
            var div = 1.0/((double)count + 1);
            for(var i=0; i<count; i++)
            {
                var t = i*div + div;
                var a = edge1.PointAt(t);
                var b = edge2.PointAt(t);
                var line = new Line(a, b);
                var beam = new Beam(line, profile, material, null);
                this.Elements.Add(beam);
            }
        }
    }
}