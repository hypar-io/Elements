using System.Collections.Generic;
using System;
using System.Linq;

namespace Elements.Geometry.Solids
{
    internal enum LocalClassification
    {
        Outside, Inside
    }

    public static class SolidBoolean
    {
        public static Solid Union(Solid a, Solid b)
        {
            var r = new Random();
            var allFaces = new List<(Polygon, SetClassification)>();

            var aFaces = a.Faces.Select(f => f.Value.Outer.ToPolygon()).ToList();
            var bFaces = b.Faces.Select(f => f.Value.Outer.ToPolygon()).ToList();

            foreach (var af in aFaces)
            {
                var classification = IntersectAndClassify(r, af, bFaces);
                foreach (var c in classification)
                {
                    allFaces.Add((c.Item1, c.Item2 == LocalClassification.Outside ? SetClassification.AOutsideB : SetClassification.AInsideB));
                }
            }

            foreach (var bf in bFaces)
            {
                var classification = IntersectAndClassify(r, bf, aFaces);
                foreach (var c in classification)
                {
                    allFaces.Add((c.Item1, c.Item2 == LocalClassification.Outside ? SetClassification.BOutsideA : SetClassification.BInsideA));
                }
            }

            var s = new Solid();
            foreach (var p in allFaces.Where(o => o.Item2 == SetClassification.AOutsideB || o.Item2 == SetClassification.BOutsideA).Select(o => o.Item1))
            {
                s.AddFace(p);
            }

            return s;
        }

        private static List<(Polygon, LocalClassification)> IntersectAndClassify(Random r, Polygon a, List<Polygon> b)
        {
            var classifications = new List<(Polygon, LocalClassification)>();

            var trimFaces = a.IntersectOneToMany(b, out _, out List<(Vector3 from, Vector3 to, int? index)> trimEdges);

            if (trimFaces.Count == 1)
            {
                // If there's only one face, we ray cast to test
                // for inclusion.
                var tf = trimFaces[0];
                var intersections = 0;
                var ray = r.NextRay(tf.Vertices[0]);
                foreach (var bf in b)
                {
                    if (ray.Intersects(bf, out _))
                    {
                        intersections++;
                    }
                }
                classifications.Add((tf, intersections % 2 == 0 ? LocalClassification.Outside : LocalClassification.Inside));
            }
            else
            {
                // If there's more than one face, we test to see if
                // the face is "to the left" of one of the trimming polys.
                var compareEdge = trimEdges.First(e => e.index != -1);
                var trimPolyIndex = compareEdge.index.Value;
                var comparePoly = b[trimPolyIndex];
                var bn = comparePoly.Normal();
                var n = a.Normal();

                foreach (var tf in trimFaces)
                {
                    var matchEdge = tf.Edges().First(e => (e.from.IsAlmostEqualTo(compareEdge.from) && e.to.IsAlmostEqualTo(compareEdge.to)) || (e.from.IsAlmostEqualTo(compareEdge.to) && e.to.IsAlmostEqualTo(compareEdge.from)));
                    var d = (matchEdge.from - matchEdge.to).Unitized();
                    var dot = bn.Dot(n.Cross(d));
                    classifications.Add((tf, dot < 0.0 ? LocalClassification.Outside : LocalClassification.Inside));
                }
            }

            return classifications;
        }

        public static Solid Difference(Solid a, Solid b)
        {
            throw new NotImplementedException();
        }

        public static Solid Intersection(Solid a, Solid b)
        {
            throw new NotImplementedException();
        }
    }
}