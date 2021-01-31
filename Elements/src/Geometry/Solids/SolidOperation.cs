using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;
using Newtonsoft.Json;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// The base class for all operations which create solids.
    /// </summary>
    public abstract partial class SolidOperation
    {
        internal Solid _solid;

        internal Csg.Solid _csg;

        /// <summary>
        /// The local transform of the operation.
        /// </summary>
        public Transform LocalTransform { get; set; }

        /// <summary>
        /// The solid operation's solid.
        /// </summary>
        [JsonIgnore]
        public Solid Solid
        {
            get { return _solid; }
        }
    }

    /// <summary>
    /// Solid operation extensions.
    /// </summary>
    public static class SolidOperationExtensions
    {
        /// <summary>
        /// Intersect a collection of solid operations with the provided plane.
        /// </summary>
        /// <param name="solidOperations">A collection of solid operations.</param>
        /// <param name="p">A plane.</param>
        /// <param name="result">A collection of profiles representing discrete results of the intersection operation.</param>
        /// <returns>True if at least one intersection is found, otherwise false.</returns>
        public static bool TryIntersect(this IList<SolidOperation> solidOperations, Plane p, out List<Profile> result)
        {
            if (!p.Normal.IsParallelTo(Vector3.ZAxis))
            {
                throw new Exception("Only horizontal planes are currently supported for intersection with solid operations.");
            }

            var add = new List<Polygon>();
            var remove = new List<Polygon>();

            result = new List<Profile>();

            foreach (var op in solidOperations)
            {
                if (op.Solid.TryIntersect(p, out var pgons))
                {
                    if (op.IsVoid)
                    {
                        remove.AddRange(pgons);
                    }
                    else
                    {
                        add.AddRange(pgons);
                    }
                }
            }

            Clipper clipper = new Clipper();

            var addPaths = add.Select(s => s.ToClipperPath(Vector3.EPSILON)).ToList();
            var removePaths = remove.Select(s => s.Reversed()).Select(s => s.ToClipperPath(Vector3.EPSILON)).ToList();
            clipper.AddPaths(addPaths, PolyType.ptSubject, true);
            clipper.AddPaths(removePaths, PolyType.ptClip, true);

            PolyTree solution = new PolyTree();
            clipper.Execute(ClipType.ctDifference, solution, PolyFillType.pftNonZero);
            if (solution.ChildCount == 0)
            {
                return false;
            }

            // Clipper will flatten all polygons to the ground plane.
            // Create a transform to move these back to the correct elevation.
            var level = new Transform(0, 0, p.Origin.Z);
            foreach (var child in solution.Childs)
            {
                var perimeter = PolygonExtensions.ToPolygon(child.Contour, Vector3.EPSILON);
                List<Polygon> voidCrvs = new List<Polygon>();
                if (child.ChildCount > 0)
                {
                    foreach (var innerChild in child.Childs)
                    {
                        var voidCrv = PolygonExtensions.ToPolygon(innerChild.Contour, Vector3.EPSILON);
                        voidCrvs.Add(voidCrv);
                    }

                }

                result.Add(new Profile(perimeter, voidCrvs, Guid.NewGuid(), string.Empty).Transformed(level));
            }

            return true;
        }
    }
}