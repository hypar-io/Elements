using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry.Interfaces;

namespace Elements.Geometry
{
    /// <summary>
    /// Methods for creating extrusions.
    /// </summary>
    public static class Extrusions
    {
        /// <summary>
        /// Extrude a Profile.
        /// </summary>
        /// <param name="profile">The Profile to extrude.</param>
        /// <param name="height">The height of the extrusion.</param>
        /// <param name="offset">The offset from 0.0 at which to begin the extrusion.</param>
        /// <param name="capped">Does the extrusion have faces at each end?</param>
        /// <returns></returns>
        public static IFace[] Extrude(IProfile profile, double height, double offset = 0, bool capped = true)
        {
            var faces = new List<IFace>();
            var clipped = Clip(profile.Perimeter, profile.Voids);

            // start cap
            if(capped)
            {
                var tStart = new Transform(new Vector3(0, 0, offset));
                var start = tStart.OfPolygons(clipped.Reversed());
                faces.Add(new PlanarFace(start));
            }

            foreach(var p in clipped)
            {
                foreach(var s in p.Segments())
                {
                    faces.Add(Extrude(s, height, offset));
                }
            }

            // end cap
            if(capped)
            {
                var tEnd = new Transform(new Vector3(0, 0, offset + height));
                var end = tEnd.OfPolygons(clipped);
                faces.Add(new PlanarFace(end));
            }
            
            return faces.ToArray();
        }

        /// <summary>
        /// Extrude a Profile along a curve.
        /// </summary>
        /// <param name="profile">The Profile to extrude.</param>
        /// <param name="curve">The curve along which to extrude.</param>
        /// <param name="capped">Should the extrusion have faces at each end?</param>
        /// <param name="startSetback">The distance from the start of the curve that the extrusion should start.</param>
        /// <param name="endSetback">The distance from the end of the curve that the extrusion should start.</param>
        public static IFace[] ExtrudeAlongCurve(IProfile profile, ICurve curve, bool capped = true, double startSetback = 0, double endSetback = 0)
        {
            var clipped = Clip(profile.Perimeter, profile.Voids);

            var faces = new List<IFace>();

            var l = curve.Length();
            var ssb = startSetback / l;
            var esb = endSetback / l;

            var transforms = curve.Frames(ssb, esb);

            if (curve is Polygon)
            {
                for (var i = 0; i < transforms.Length; i++)
                {
                    var next = i == transforms.Length - 1 ? transforms[0] : transforms[i + 1];

                    foreach(var p in clipped)
                    {
                        faces.AddRange(ExtrudePolygonBetweenPlanes(p, transforms[i].XY, next.XY));
                    }
                }
            }
            else
            {
                for (var i = 0; i < transforms.Length - 1; i++)
                {
                    foreach(var p in clipped)
                    {
                        faces.AddRange(ExtrudePolygonBetweenPlanes(p, transforms[i].XY, transforms[i + 1].XY));
                    }
                }

                if (capped)
                {
                    faces.Add(new PlanarFace(transforms[0].OfPolygons(clipped)));
                    faces.Add(new PlanarFace(transforms[transforms.Length - 1].OfPolygons(clipped.Reversed())));
                }
            }

            return faces.ToArray();
        }

        /// <summary>
        /// Extrude a profile along direction by distance.
        /// </summary>
        /// <param name="profile">The Profile to extrude.</param>
        /// <param name="start">The Transform which defines the plane into which the start profile will be transformed.</param>
        /// <param name="direction">The direction along which to extrude.</param>
        /// <param name="distance">The distance to extrude.</param>
        /// <param name="capped">Should the extrusion have faces at each end?</param>
        /// <returns></returns>
        public static IFace[] ExtrudeInDirection(IProfile profile, Transform start, Vector3 direction, double distance, bool capped = true)
        {
            var clipped = Clip(profile.Perimeter, profile.Voids);
            var end = new Transform(start);
            var trans = direction.Normalized() * distance;
            end.Move(trans);

            var faces = new List<IFace>();
            var startPolys = start.OfPolygons(clipped.Reversed());
            
            foreach(var p in startPolys)
            {
                for(var i=0; i<p.Vertices.Length; i++)
                {
                    var a = p.Vertices[i];
                    var b = i == p.Vertices.Length - 1 ? p.Vertices[0] : p.Vertices[i+1];
                    var c = b + trans;
                    var d = a + trans;
                    faces.Add(new QuadFace(new[]{d,c,b,a}));
                }
            }

            if (capped)
            {
                faces.Add(new PlanarFace(startPolys));
                faces.Add(new PlanarFace(end.OfPolygons(clipped)));
            }
            
            return faces.ToArray();
        }

        private static Polygon[] Clip(Polygon perimeter, IList<Polygon> voids)
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
            if (solution.Count > 0)
            {
                var polys = solution.Select(s => s.ToPolygon()).ToArray();
                return polys;
            }
            return null;
        }

        private static IFace Extrude(Arc a, double height, double offset = 0)
        {
            var endCenter = a.Start + new Vector3(0, 0, height + offset);
            var endArc = new Arc(endCenter, a.Radius, a.StartAngle, a.EndAngle);
            return new ConicFace(a, endArc);
        }

        private static IFace Extrude(Vector3 v1, Vector3 v2, double height, double offset = 0)
        {
            var v1n = new Vector3(v1.X, v1.Y, offset);
            var v2n = new Vector3(v2.X, v2.Y, offset);
            var v3n = new Vector3(v2.X, v2.Y, offset + height);
            var v4n = new Vector3(v1.X, v1.Y, offset + height);
            return new PlanarFace(new Polygon(new[] { v1n, v2n, v3n, v4n }));
        }

        private static IFace Extrude(Line l, double height, double offset = 0)
        {
            var v1 = l.Start;
            var v2 = l.End;
            return Extrude(v1, v2, height, offset);
        }

        private static IFace[] ExtrudePolygonBetweenPlanes(Polygon p, Plane start, Plane end)
        {
            var faces = new List<IFace>();

            // Transform the polygon to the mid plane between two transforms.
            var mid = new Line(start.Origin, end.Origin).TransformAt(0.5).OfPolygon(p);
            var v = (end.Origin - start.Origin).Normalized();
            var startP = mid.ProjectAlong(v, start);
            var endP = mid.ProjectAlong(v, end);

            for (var i = 0; i < startP.Vertices.Length; i++)
            {
                Vector3 a, b, c, d;

                if (i == startP.Vertices.Length - 1)
                {
                    a = startP.Vertices[i];
                    b = startP.Vertices[0];
                    c = endP.Vertices[0];
                    d = endP.Vertices[i];
                }
                else
                {
                    a = startP.Vertices[i];
                    b = startP.Vertices[i + 1];
                    c = endP.Vertices[i + 1];
                    d = endP.Vertices[i];
                }

                faces.Add(new QuadFace(new[] { a, d, c, b }));
            }

            return faces.ToArray();
        }
    }
}