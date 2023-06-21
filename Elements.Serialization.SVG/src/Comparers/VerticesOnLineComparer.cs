using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    internal class VerticesOnVectorComparer : IComparer<VertexAdapter>
    {
        private Vector3 _vector;

        public VerticesOnVectorComparer(Vector3 vector)
        {
            _vector = vector;
        }

        public int Compare([AllowNull] VertexAdapter x, [AllowNull] VertexAdapter y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return 1;
            }

            if (y == null)
            {
                return -1;
            }

            var dif = (x.Projection - y.Projection).Unitized();
            if (dif.IsAlmostEqualTo(Vector3.Origin))
            {
                return 0;
            }

            if (dif.IsAlmostEqualTo(_vector))
            {
                return 1;
            }

            return -1;
        }
    }
}