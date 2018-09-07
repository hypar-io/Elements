using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// Extrude represents a surface extruded from a curve.
    /// </summary>
    public class Extrude : ITessellate<Mesh>
    {
        private ICurve _curve;
        private Vector3 _direction;
        private double _distance;

        /// <summary>
        /// Extrude a curve by distance in a direction.
        /// </summary>
        /// <param name="curve">The curve to extrude.</param>
        /// <param name="direction">The direction along which to extrude.</param>
        /// <param name="distance">The distance to extrude.</param>
        public Extrude(ICurve curve, Vector3 direction, double distance)
        {
            this._curve = curve;
            this._direction = direction;
            this._distance = distance;
        }

        /// <summary>
        /// Tessellate the surface.
        /// </summary>
        /// <returns>A mesh representing the surface.</returns>
        public Mesh Tessellate()
        {
            var tess = this._curve.Tessellate().ToArray();
            var offset = new Vector3[tess.Length];
            for (var i = 0; i < tess.Length; i++)
            {
                offset[i] = tess[i] + this._direction * _distance;
            }
            var mesh = new Mesh();
            for (var i = 0; i < tess.Length - 1; i++)
            {
                var a = tess[i];
                var b = tess[i + 1];
                var c = offset[i + 1];
                var d = offset[i];
                mesh.AddQuad(new[] { a, b, c, d });
            }
            return mesh;
        }
    }
}