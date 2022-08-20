using System.Collections.Generic;

namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// An object which provides tessellation targets for a csg solid.
    /// </summary>
    internal class CsgTessellationTargetProvider : ITessellationTargetProvider
    {
        private readonly Csg.Solid csg;

        /// <summary>
        /// Construct a CsgTessellationTargetProvider.
        /// </summary>
        /// <param name="csg"></param>
        public CsgTessellationTargetProvider(Csg.Solid csg)
        {
            this.csg = csg;
        }

        /// <summary>
        /// Get the tessellation targets.
        /// </summary>
        public IEnumerable<ITessAdapter> GetTessellationTargets()
        {
            var id = 0;
            foreach (var p in csg.Polygons)
            {
                yield return new CsgPolygonTessAdapter(p, id);
                id++;
            }
        }
    }
}