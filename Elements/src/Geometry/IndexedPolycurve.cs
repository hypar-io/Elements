using Newtonsoft.Json;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// 
    /// </summary>
    public class IndexedPolycurve : BoundedCurve
    {
        /// <summary>
        /// A bounding box created once during the polyline's construction.
        /// This will not be updated when a polyline's vertices are changed.
        /// </summary>
        internal BBox3 _bounds;

        /// <summary>
        /// A collection of collections of indices of polycurve segments.
        /// Line segments are represented with two indices.
        /// Arc segments are represented with three indices.
        /// </summary>
        /// <returns></returns>
        public IList<IList<uint>> Segments { get; set; } = new List<IList<uint>>();

        /// <summary>The vertices of the polygon.</summary>
        [JsonProperty("Vertices", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(2)]
        public IList<Vector3> Vertices { get; set; } = new List<Vector3>();

        public override BBox3 Bounds()
        {
            throw new System.NotImplementedException();
        }

        public override double[] GetSubdivisionParameters(double startSetbackDistance = 0, double endSetbackDistance = 0)
        {
            throw new System.NotImplementedException();
        }

        public override double Length()
        {
            throw new System.NotImplementedException();
        }

        public override double ParameterAtDistanceFromParameter(double distance, double start)
        {
            throw new System.NotImplementedException();
        }

        public override Vector3 PointAt(double u)
        {
            throw new System.NotImplementedException();
        }

        public override Transform TransformAt(double u)
        {
            throw new System.NotImplementedException();
        }

        public override Curve Transformed(Transform transform)
        {
            throw new System.NotImplementedException();
        }
    }
}