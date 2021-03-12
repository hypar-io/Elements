using Elements.Geometry;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A unique vertex in a cell complex
    /// </summary>
    public class Vertex : CellChild
    {
        /// <summary>
        /// Location in space
        /// </summary>
        public Vector3 Value;

        /// <summary>
        /// Some optional name, if we want it
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// All segments connected to this Vertex
        /// </summary>
        [JsonIgnore]
        public HashSet<Segment> Segments { get; internal set; } = new HashSet<Segment>();

        /// <summary>
        /// Represents a unique Vertex within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to</param>
        /// <param name="id"></param>
        /// <param name="point">Location of the vertex</param>
        /// <param name="name">Optional name</param>
        internal Vertex(CellComplex cellComplex, long id, Vector3 point, string name = null) : base(id, cellComplex)
        {
            this.Value = point;
            this.Name = name;
        }

        [JsonConstructor]
        internal Vertex(long id, Vector3 point, string name = null) : base(id, null)
        {
            this.Value = point;
            this.Name = name;
        }

        /// <summary>
        /// Get associated Segments
        /// </summary>
        /// <returns></returns>
        public List<Segment> GetSegments()
        {
            return this.Segments.ToList();
        }

        /// <summary>
        /// Get associated DirectedSegments
        /// </summary>
        /// <returns></returns>
        public List<DirectedSegment> GetDirectedSegments()
        {
            return this.GetSegments().Select(segment => segment.GetDirectedSegments()).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get associated Faces
        /// </summary>
        /// <returns></returns>
        public List<Face> GetFaces()
        {
            return this.GetDirectedSegments().Select(ds => ds.GetFaces()).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get associated Cells
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetCells()
        {
            return this.GetFaces().Select(face => face.GetCells()).SelectMany(x => x).Distinct().ToList();
        }
    }
}