using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry.Interfaces;

namespace Elements.Geometry
{
    internal static class Extrusions
    {
        internal static IFace[] Extrude(IProfile profile, double height, double offset=0)
        {
            var faces = new List<IFace>();
            var clipped = Clip(profile.Perimeter, profile.Voids);

            // start cap
            var tStart = new Transform(new Vector3(0,0,offset));
            var start = tStart.OfProfile(clipped.Reversed());
            faces.Add(new PlanarFace(start));

            // outer loop
            foreach(var s in clipped.Perimeter.Segments())
            {
                faces.Add(Extrude(s, height, offset));
            }

            // inner loops
            if(clipped.Voids != null)
            {
                foreach(var v in profile.Voids)
                {
                    foreach(var s in v.Segments())
                    {
                        faces.Add(Extrude(s, height, offset));
                    }
                }
            }

            // end cap
            var tEnd = new Transform(new Vector3(0,0,offset + height));
            var end = tEnd.OfProfile(clipped);
            faces.Add(new PlanarFace(end));
            return faces.ToArray();
        }

        private static Profile Clip(Polygon perimeter, IList<Polygon> voids)
        {
            var clipper = new ClipperLib.Clipper();
            clipper.AddPath(perimeter.ToClipperPath(), ClipperLib.PolyType.ptSubject, true);
            if (voids != null)
            {
                clipper.AddPaths(voids.Select(p => p.ToClipperPath()).ToList(), ClipperLib.PolyType.ptClip, true);
            }
            var solution = new List<List<ClipperLib.IntPoint>>();
            var result = clipper.Execute(ClipperLib.ClipType.ctDifference, solution, ClipperLib.PolyFillType.pftEvenOdd);
            
            // Completely disjoint polygons like a circular pipe
            // profile will result in an empty solution.
            if(solution.Count > 0)
            {
                var polys = solution.Select(s => s.ToPolygon()).ToList();
                if (polys.Count > 1)
                {
                    return new Profile(polys.First(), polys.Skip(1).ToList());
                }
                else
                {
                    return new Profile(polys.First());
                }
            }
            return null;
        }

        internal static IFace Extrude(Arc a, double height, double offset = 0)
        {
            var endCenter = a.Start + new Vector3(0,0,height+offset);
            var endArc = new Arc(endCenter, a.Radius, a.StartAngle, a.EndAngle);
            return new ConicFace(a, endArc);
        }

        internal static IFace Extrude(Vector3 v1, Vector3 v2, double height, double offset = 0)
        {
            var v1n = new Vector3(v1.X, v1.Y, offset);
            var v2n = new Vector3(v2.X, v2.Y, offset);
            var v3n = new Vector3(v2.X, v2.Y, offset + height);
            var v4n = new Vector3(v1.X, v1.Y, offset + height);
            return new PlanarFace(new Profile(new Polygon(new []{v1n, v2n, v3n, v4n })));
        }

        internal static IFace Extrude(Line l, double height, double offset = 0)
        {
            var v1 = l.Start;
            var v2 = l.End;
            return Extrude(v1, v2, height, offset);
        }
        
        internal static IFace[] ExtrudeAlongCurve(IProfile profile, ICurve curve, bool capped = true, double startSetback = 0, double endSetback = 0)
        {
            var clipped = Clip(profile.Perimeter, profile.Voids);

            var faces = new List<IFace>();

            var l = curve.Length();
            var ssb = startSetback/l;
            var esb = endSetback/l;

            var transforms = new List<Transform>();
            
            transforms.AddRange(curve.Frames(ssb, esb));

            if(curve is Polygon)
            {
                for(var i = 0; i < transforms.Count; i++)
                {
                    var next = i == transforms.Count - 1 ? transforms[0] : transforms[i+1];
                    faces.AddRange(ExtrudePolygonBetweenPlanes(clipped.Perimeter, transforms[i], next));

                    if(clipped.Voids != null)
                    {
                        foreach(var p in clipped.Voids)
                        {
                            faces.AddRange(ExtrudePolygonBetweenPlanes(p, transforms[i], next));
                        }
                    }
                }
            }
            else
            {
                for(var i = 0; i < transforms.Count - 1; i++)
                {
                    faces.AddRange(ExtrudePolygonBetweenPlanes(clipped.Perimeter, transforms[i], transforms[i+1]));

                    if(clipped.Voids != null)
                    {
                        foreach(var p in clipped.Voids)
                        {
                            faces.AddRange(ExtrudePolygonBetweenPlanes(p, transforms[i], transforms[i+1]));
                        }
                    }
                }

                if(capped)
                {
                    faces.Add(new PlanarFace(transforms[0].OfProfile(clipped)));
                    faces.Add(new PlanarFace(transforms[transforms.Count-1].OfProfile(clipped.Reversed())));
                }
            }

            return faces.ToArray();
        }

        private static IFace[] ExtrudePolygonBetweenPlanes(Polygon p, Transform tStart, Transform tEnd, bool reverse = false)
        {
            var faces = new List<IFace>();

            // Transform the polygon to the mid plane between two transforms.
            var mid = new Line(tStart.Origin, tEnd.Origin).TransformAt(0.5).OfPolygon(p); 
            var v = (tEnd.Origin - tStart.Origin).Normalized();
            var start = mid.ProjectAlong(v, tStart.XY);
            var end = mid.ProjectAlong(v, tEnd.XY);

            for(var i=0; i<start.Vertices.Count; i++)
            {
                Vector3 a, b, c, d;

                if(i == start.Vertices.Count-1)
                {
                    a = start.Vertices[i];
                    b = start.Vertices[0];
                    c = end.Vertices[0];
                    d = end.Vertices[i];
                }
                else
                {
                    a = start.Vertices[i];
                    b = start.Vertices[i+1];
                    c = end.Vertices[i+1];
                    d = end.Vertices[i];
                }

                // if(reverse)
                // {
                    faces.Add(new QuadFace(new []{a,d,c,b}));
                // }
                // else
                // {
                //     faces.Add(new QuadFace(new []{a,b,c,d}));
                // }
            }

            return faces.ToArray();
        }
    }
}