using Hypar.Geometry;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Elements
{
    /// <summary>
    /// BeamSystem represents a collection of beams within a perimeter.
    /// </summary>
    public class BeamSystem : IEnumerable<Beam>
    {
        private List<Beam> _beams = new List<Beam>();

        /// <summary>
        /// A collection of Beams contained in the system.
        /// </summary>
        /// <returns></returns>
        public IList<Beam> Beams
        {
            get{return _beams;}
        }

        /// <summary>
        /// Construct a BeamSystem between two edges.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="profile"></param>
        /// <param name="edge1"></param>
        /// <param name="edge2"></param>
        /// <param name="material"></param>
        public BeamSystem(int count, IList<Polygon> profile, Line edge1, Line edge2, Material material)
        {
            CreateBeamsBetweenEdges(edge1, edge2, count, profile, material);
        }

        /// <summary>
        /// Construct a beam system under a slab.
        /// </summary>
        /// <param name="floor"></param>
        /// <param name="count"></param>
        /// <param name="profile"></param>
        /// <param name="material"></param>
        public BeamSystem(Floor floor, int count, IList<Polygon> profile, Material material)
        {
            var edges = floor.Perimeter.Segments().ToArray();
            var e1 = edges[0];
            var e2 = edges[2].Reversed();
            var bbox = new BBox3(profile);
            var depth = bbox.Max.Y - bbox.Min.Y;
            var edge1 = new Line(new Vector3(e1.Start.X, e1.Start.Y, floor.Elevation - depth/2), new Vector3(e1.End.X, e1.End.Y, floor.Elevation - depth/2));
            var edge2 = new Line(new Vector3(e2.Start.X, e2.Start.Y, floor.Elevation - depth/2), new Vector3(e2.End.X, e2.End.Y, floor.Elevation - depth/2));
            CreateBeamsBetweenEdges(edge1, edge2, count, profile, material);
        }

        private void CreateBeamsBetweenEdges(Line edge1, Line edge2, int count, IList<Polygon> profile, Material material)
        {
            var div = 1.0/((double)count + 1);
            for(var i=0; i<count; i++)
            {
                var t = i*div + div;
                var a = edge1.PointAt(t);
                var b = edge2.PointAt(t);
                var line = new Line(a, b);
                var beam = new Beam(line, profile, material, null);
                this._beams.Add(beam);
            }
        }

        /// <summary>
        /// Get the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Beam> GetEnumerator()
        {
            return ((IEnumerable<Beam>)_beams).GetEnumerator();
        }

        /// <summary>
        /// Get the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Beam>)_beams).GetEnumerator();
        }
    }
}