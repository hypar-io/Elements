using Hypar.Geometry;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Elements
{
    public class BeamSystem
    {
        private List<Beam> m_beams = new List<Beam>();

        /// <summary>
        /// A collection of Beams contained in the system.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Beam> Beams
        {
            get{return m_beams;}
        }

        /// <summary>
        /// Construct a BeamSystem between two edges.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="profile"></param>
        /// <param name="edge1"></param>
        /// <param name="edge2"></param>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        public BeamSystem(int count, StructuralProfile profile, Line edge1, Line edge2, Material material, Transform transform = null)
        {
            CreateBeamsBetweenEdges(edge1, edge2, count, profile, material, transform);
        }

        public BeamSystem(Slab slab, int count, StructuralProfile profile, Material material, Transform transform = null)
        {
            var edges = slab.Perimeter.Explode().ToArray();
            var e1 = edges[0];
            var e2 = edges[2].Reversed();
            var edge1 = new Line(new Vector3(e1.Start.X, e1.Start.Y, slab.Elevation - profile.Depth/2), new Vector3(e1.End.X, e1.End.Y, slab.Elevation - profile.Depth/2));
            var edge2 = new Line(new Vector3(e2.Start.X, e2.Start.Y, slab.Elevation - profile.Depth/2), new Vector3(e2.End.X, e2.End.Y, slab.Elevation - profile.Depth/2));
            CreateBeamsBetweenEdges(edge1, edge2, count, profile, material, transform);
        }

        private void CreateBeamsBetweenEdges(Line edge1, Line edge2, int count, StructuralProfile profile, Material material, Transform transform)
        {
            var div = 1.0/((double)count + 1);
            for(var i=0; i<count; i++)
            {
                var t = i*div + div;
                var a = edge1.PointAt(t);
                var b = edge2.PointAt(t);
                var line = new Line(a, b);
                var beam = new Beam(line, profile, material, null, transform);
                this.m_beams.Add(beam);
            }
        }
    }
}