using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry.Solids;
using Elements.Search;

namespace Elements.Geometry
{
    /// <summary>
    /// An infinite ray starting at origin and pointing towards direction.
    /// </summary>
    public struct Ray : IEquatable<Ray>
    {
        /// <summary>
        /// The origin of the ray.
        /// </summary>
        public Vector3 Origin { get; set; }

        /// <summary>
        /// The direction of the ray.
        /// </summary>
        public Vector3 Direction { get; set; }

        /// <summary>
        /// Construct a ray.
        /// </summary>
        /// <param name="origin">The origin of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        public Ray(Vector3 origin, Vector3 direction)
        {
            this.Origin = origin;
            this.Direction = direction.Unitized();
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
        /// </summary>
        /// <param name="tri">The triangle to intersect.</param>
        /// <param name="result">The intersection result.</param>
        /// <returns>True if an intersection occurs, otherwise false. If true, check the intersection result for the type and location of intersection.</returns>
        public bool Intersects(Triangle tri, out Vector3 result)
        {
            result = default(Vector3);

            var vertex0 = tri.Vertices[0].Position;
            var vertex1 = tri.Vertices[1].Position;
            var vertex2 = tri.Vertices[2].Position;
            var edge1 = (vertex1 - vertex0);
            var edge2 = (vertex2 - vertex0);
            var h = this.Direction.Cross(edge2);
            var s = this.Origin - vertex0;
            double a, f, u, v;

            a = edge1.Dot(h);
            if (a > -Vector3.EPSILON && a < Vector3.EPSILON)
            {
                return false;    // This ray is parallel to this triangle.
            }
            f = 1.0 / a;
            u = f * (s.Dot(h));
            if (u < 0.0 || u > 1.0)
            {
                return false;
            }
            var q = s.Cross(edge1);
            v = f * this.Direction.Dot(q);
            if (v < 0.0 || u + v > 1.0)
            {
                return false;
            }
            // At this stage we can compute t to find out where the intersection point is on the line.
            double t = f * edge2.Dot(q);
            if (t > Vector3.EPSILON && t < 1 / Vector3.EPSILON) // ray intersection
            {
                result = this.Origin + this.Direction * t;
                return true;
            }
            else // This means that there is a line intersection but not a ray intersection.
            {
                return false;
            }
        }

        /// <summary>
        /// Does this ray intersect with the provided GeometricElement? Only GeometricElements with Solid Representations are currently supported, and voids will be ignored.
        /// </summary>
        /// <param name="element">The element to intersect with.</param>
        /// <param name="result">The list of intersection results.</param>
        /// <returns></returns>
        public bool Intersects(GeometricElement element, out List<Vector3> result)
        {
            if (element.Representation == null || element.Representation.SolidOperations == null || element.Representation.SolidOperations.Count == 0)
            {
                element.UpdateRepresentations();
            }
            List<Vector3> resultsOut = new List<Vector3>();
            var transformFromElement = new Transform(element.Transform);
            transformFromElement.Invert();
            var transformToElement = new Transform(element.Transform);
            var transformedRay = new Ray(transformFromElement.OfPoint(Origin), transformFromElement.OfVector(Direction));
            //TODO: extend to handle voids when void solids in Representations are supported generally
            var intersects = false;
            foreach (var solidOp in element.Representation.SolidOperations.Where(e => !e.IsVoid))
            {
                if (transformedRay.Intersects(solidOp, out List<Vector3> tempResults))
                {
                    intersects = true;
                    resultsOut.AddRange(tempResults.Select(t => transformToElement.OfPoint(t)));
                };
            }
            result = resultsOut;
            return intersects;
        }

        /// <summary>
        /// Does this ray intersect with the provided SolidOperation?
        /// </summary>
        /// <param name="solidOp">The SolidOperation to intersect with.</param>
        /// <param name="result">The list of intersection results, ordered by distance from the ray origin.</param>
        /// <returns>True if an intersection occurs, otherwise false. If true, check the intersection result for the location of the intersection.</returns>
        public bool Intersects(SolidOperation solidOp, out List<Vector3> result)
        {
            var intersects = Intersects(solidOp.Solid, out List<Vector3> tempResult);
            result = tempResult;
            return intersects;
        }

        /// <summary>
        /// Does this ray intersect with the provided Solid?
        /// </summary>
        /// <param name="solid">The Solid to intersect with.</param>
        /// <param name="result">The intersection result.</param>
        /// <returns>True if an intersection occurs, otherwise false. If true, check the intersection result for the location of the intersection.</returns>
        internal bool Intersects(Solid solid, out List<Vector3> result)
        {
            var faces = solid.Faces;
            var intersects = false;
            List<Vector3> results = new List<Vector3>();
            foreach (var face in faces)
            {
                if (Intersects(face.Value, out Vector3 tempResult))
                {
                    intersects = true;
                    results.Add(tempResult);
                }
            }
            var origin = Origin; // lambdas in structs can't refer to their properties
            result = results.OrderBy(r => r.DistanceTo(origin)).ToList();
            return intersects;
        }

        /// <summary>
        /// Does this ray intersect with the provided face?
        /// </summary>
        /// <param name="face">The Face to intersect with.</param>
        /// <param name="result">The intersection result.</param>
        /// <returns>True if an intersection occurs, otherwise false. If true, check the intersection result for the location of the intersection.</returns>
        internal bool Intersects(Face face, out Vector3 result)
        {
            if (Intersects(face.Outer.ToPolygon(), out Vector3 intersection, out _))
            {
                result = intersection;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Does this ray intersect the provided polygon area?
        /// </summary>
        /// <param name="polygon">The Polygon to intersect with.</param>
        /// <param name="result">The intersection result.</param>
        /// <param name="containment">An enumeration detailing the type of intersection if one occurs.</param>
        /// <returns>True if an intersection occurs, otherwise false. If true, check the intersection result for the location of the intersection.</returns>
        public bool Intersects(Polygon polygon, out Vector3 result, out Containment containment)
        {
            var plane = polygon.Plane();
            if (Intersects(plane, out Vector3 test))
            {
                if (polygon.Contains(test, out containment))
                {
                    result = test;
                    return true;
                }
            }
            result = default;
            containment = Containment.Outside;
            return false;
        }

        /// <summary>
        /// Does this ray intersect the provided plane?
        /// </summary>
        /// <param name="plane">The Plane to intersect with.</param>
        /// <param name="result">The intersection result.</param>
        /// <returns>True if an intersection occurs, otherwise false — this can occur if the ray is very close to parallel to the plane.
        /// If true, check the intersection result for the location of the intersection.</returns>
        public bool Intersects(Plane plane, out Vector3 result)
        {
            var doesIntersect = Intersects(plane, out Vector3 resultVector, out _);
            result = resultVector;
            return doesIntersect;
        }

        /// <summary>
        /// Does this ray intersect the provided plane?
        /// </summary>
        /// <param name="plane">The Plane to intersect with.</param>
        /// <param name="result">The intersection result.</param>
        /// <param name="t"></param>
        /// <returns>True if an intersection occurs, otherwise false — this can occur if the ray is very close to parallel to the plane.
        /// If true, check the intersection result for the location of the intersection.</returns>
        public bool Intersects(Plane plane, out Vector3 result, out double t)
        {
            result = default(Vector3);
            t = double.NaN;
            var d = Direction;

            // Test for perpendicular.
            if (plane.Normal.Dot(d) == 0)
            {
                return false;
            }
            t = (plane.Normal.Dot(plane.Origin) - plane.Normal.Dot(Origin)) / plane.Normal.Dot(d);

            // If t < 0, the point of intersection is behind
            // the start of the ray.
            if (t < 0)
            {
                return false;
            }
            result = Origin + d * t;
            return true;
        }

        /// <summary>
        /// Does this ray intersect the provided topography?
        /// </summary>
        /// <param name="topo">The topography.</param>
        /// <param name="result">The location of intersection.</param>
        /// <returns>True if an intersection result occurs.
        /// False if no intersection occurs.</returns>
        public bool Intersects(Topography topo, out Vector3 result)
        {
            var transform = topo.Transform;
            var inverse = transform.Inverted();
            var transformedRay = new Ray(inverse.OfPoint(Origin), Direction);
            var intersects = transformedRay.Intersects(topo.Mesh, out result);
            if (intersects)
            {
                result = transform.OfPoint(result);
            }
            return intersects;
        }

        /// <summary>
        /// Does this ray intersect the provided mesh?
        /// </summary>
        /// <param name="mesh">The Mesh.</param>
        /// <param name="result">The location of intersection.</param>
        /// <returns>True if an intersection result occurs.
        /// False if no intersection occurs.</returns>

        public bool Intersects(Mesh mesh, out Vector3 result)
        {
            return mesh.Intersects(this, out result);
        }

        /// <summary>
        /// Does this ray intersect the provided ray?
        /// </summary>
        /// <param name="ray">The ray to intersect.</param>
        /// <param name="result">The location of intersection.</param>
        /// <param name="ignoreRayDirection">If true, the direction of the rays will be ignored</param>
        /// <returns>True if the rays intersect, otherwise false.</returns>
        public bool Intersects(Ray ray, out Vector3 result, bool ignoreRayDirection = false)
        {
            return Intersects(ray, out result, out _, ignoreRayDirection);
        }

        /// <summary>
        /// Does this ray intersect the provided ray?
        /// </summary>
        /// <param name="ray">The ray to intersect.</param>
        /// <param name="result">The location of intersection.</param>
        /// <param name="intersectionResult">An enumeration of possible ray intersection result types.</param>
        /// <param name="ignoreRayDirection">If true, the direction of the rays will be ignored.</param>
        /// <returns>True if the rays intersect, otherwise false.</returns>
        public bool Intersects(Ray ray, out Vector3 result, out RayIntersectionResult intersectionResult, bool ignoreRayDirection = false)
        {
            var p1 = this.Origin;
            var p2 = ray.Origin;
            var d1 = this.Direction;
            var d2 = ray.Direction;

            var t1 = (p2 - p1).Cross(d2).Dot(d1.Cross(d2)) / Math.Pow(d1.Cross(d2).Length(), 2);
            var t2 = (p2 - p1).Cross(d1).Dot(d1.Cross(d2)) / Math.Pow(d1.Cross(d2).Length(), 2);

            if (double.IsNaN(t1) && double.IsNaN(t2))
            {
                // Rays are coincident or parallel. 

                var tt1 = p1.ProjectedParameterOn(ray);
                var opposite = d1.Dot(d2).ApproximatelyEquals(-1);
                if (tt1 < 0 && opposite)
                {
                    // Rays are disjoint pointing in different directions
                    result = default;
                    intersectionResult = RayIntersectionResult.None;
                    return false;
                }

                // Check for parallel by testing distances to opposite rays.
                // If the distances are equal and non-zero, the rays are parallel.
                var d = p2.DistanceTo(this);
                var dd = p1.DistanceTo(ray);
                if ((Double.IsInfinity(d) && Double.IsInfinity(dd))
                    || d.ApproximatelyEquals(dd)
                    && !d.ApproximatelyEquals(0))
                {
                    // Parallel
                    result = default;
                    intersectionResult = RayIntersectionResult.Parallel;
                    return false;
                }
                else
                {
                    result = Origin;
                    intersectionResult = RayIntersectionResult.Coincident;
                    return true;
                }
            }

            var a = p1 + d1 * t1;
            var b = p2 + d2 * t2;

            result = default;

            if (a.IsAlmostEqualTo(b))
            {
                // The rays intersect
                var valid = ignoreRayDirection || t1 >= 0 && t2 >= 0;
                intersectionResult = valid ? RayIntersectionResult.Intersect : RayIntersectionResult.None;
                result = valid ? a : default;
                return valid;
            }

            intersectionResult = RayIntersectionResult.None;
            return false;
        }

        /// <summary>
        /// Does this ray intersect the provided line?
        /// </summary>
        /// <param name="line">The line to intersect.</param>
        /// <param name="result">The location of intersection.</param>
        /// <returns>True if the rays intersect, otherwise false.</returns>
        public bool Intersects(Line line, out Vector3 result)
        {
            return Intersects(line.Start, line.End, out result);
        }

        /// <summary>
        /// Does this ray intersect a line segment defined by start and end?
        /// </summary>
        /// <param name="start">The start of the line segment.</param>
        /// <param name="end">The end of the line segment.</param>
        /// <param name="result">The location of the intersection.</param>
        /// <param name="intersectionResult">The nature of the ray intersection.</param>
        /// <returns>True if the ray intersects, otherwise false.</returns>
        public bool Intersects(Vector3 start, Vector3 end, out Vector3 result, out RayIntersectionResult intersectionResult)
        {
            var d = (end - start).Unitized();
            var l = start.DistanceTo(end);
            var otherRay = new Ray(start, d);
            if (Intersects(otherRay, out Vector3 rayResult, out intersectionResult))
            {
                // Quick out if the result is exactly at the 
                // start or the end of the line.
                if (rayResult.IsAlmostEqualTo(start) || rayResult.IsAlmostEqualTo(end))
                {
                    result = rayResult;
                    return true;
                }
                else if ((rayResult - start).Length() > l)
                {
                    result = default;
                    return false;
                }
                else
                {
                    result = rayResult;
                    return true;
                }
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Does this ray intersect a line segment defined by start and end?
        /// </summary>
        /// <param name="start">The start of the line segment.</param>
        /// <param name="end">The end of the line segment.</param>
        /// <param name="result">The location of the intersection.</param>
        /// <returns>True if the ray intersects, otherwise false.</returns>
        public bool Intersects(Vector3 start, Vector3 end, out Vector3 result)
        {
            return Intersects(start, end, out result, out _);
        }

        /// <summary>
        /// Find points in the collection that are within the provided distance of this ray.
        /// </summary>
        /// <param name="points">The collection of points to search</param>
        /// <param name="distance">The maximum distance from the ray.</param>
        /// <returns>Points that are within the given distance of the ray.</returns>
        public Vector3[] NearbyPoints(IEnumerable<Vector3> points, double distance)
        {
            // TODO: calibrate these values
            var octree = new PointOctree<Vector3>(10000, (0, 0, 0), (float)Vector3.EPSILON * 100);
            foreach (var point in points)
            {
                octree.Add(point, point);
            }
            var nearbyPoints = octree.GetNearby(this, (float)distance);
            return nearbyPoints;
        }

        /// <summary>
        /// Is this ray equal to the provided ray?
        /// </summary>
        /// <param name="other">The ray to test.</param>
        /// <returns>Returns true if the two rays are equal, otherwise false.</returns>
        public bool Equals(Ray other)
        {
            return this.Origin.Equals(other.Origin) && this.Direction.Equals(other.Direction);
        }

        internal static Ray GetTestRayInPlane(Vector3 origin, Vector3 normal)
        {
            var v1 = normal.IsAlmostEqualTo(Vector3.XAxis) ? Vector3.YAxis : Vector3.XAxis;
            var d = v1.Cross(normal);
            return new Ray(origin, d);
        }
    }
}