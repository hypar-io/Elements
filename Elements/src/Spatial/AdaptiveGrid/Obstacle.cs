using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// AdaptiveGrid obstacle represented by a set of points with extra parameters.
    /// Points are used to created bounding box that is aligned with transformation parameter
    /// with extra offset. Since offset is applied on the box, distance on corners is even larger.
    /// Can be constructed from different objects.
    /// </summary>
    public class Obstacle 
    {
        private Transform _transform;
        private double _offset;

        private readonly List<Polygon> _primaryPolygons = new List<Polygon>();
        private readonly List<Polygon> _secondaryPolygons = new List<Polygon>();

        /// <summary>
        /// Create an obstacle from a column.
        /// </summary>
        /// <param name="column">Column to avoid.</param>
        /// <param name="offset">Extra space around obstacle bounding box.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <param name="allowOutsideBoundary">Should edges be created when obstacle is outside of <see cref="AdaptiveGrid.Boundaries"/></param>
        /// <returns>New obstacle object.</returns>
        public static Obstacle FromColumn(Column column, double offset = 0, bool perimeter = false, bool allowOutsideBoundary = false)
        {
            var p = column.Profile.Perimeter.TransformedPolygon(
                new Transform(column.Location));
            return new Obstacle(p, column.Height, offset, perimeter, allowOutsideBoundary, null);
        }

        /// <summary>
        /// Create an obstacle from a wall.
        /// </summary>
        /// <param name="wall">Wall to avoid.</param>
        /// <param name="offset">Extra space around obstacle bounding box.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <param name="allowOutsideBoundary">Should edges be created when obstacle is outside of <see cref="AdaptiveGrid.Boundaries"/></param>
        /// <returns>New obstacle object.</returns>
        public static Obstacle FromWall(StandardWall wall, double offset = 0, bool perimeter = false, bool allowOutsideBoundary = false)
        {
            var ortho = wall.CenterLine.Direction().Cross(Vector3.ZAxis);
            var polygon = new Polygon
            (
                wall.CenterLine.Start + ortho * wall.Thickness / 2,
                wall.CenterLine.End + ortho * wall.Thickness / 2,
                wall.CenterLine.Start - ortho * wall.Thickness / 2,
                wall.CenterLine.End - ortho * wall.Thickness / 2
            );
            var transfrom = new Transform(Vector3.Origin,
                wall.CenterLine.Direction(), ortho, Vector3.ZAxis);

            return new Obstacle(polygon, wall.Height, offset, perimeter, allowOutsideBoundary, transfrom);
        }

        /// <summary>
        /// Create an obstacle from a bounding box.
        /// </summary>
        /// <param name="box">Bounding box to avoid.</param>
        /// <param name="offset">Extra space around obstacle bounding box.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <param name="allowOutsideBoundary">Should edges be created when obstacle is outside of <see cref="AdaptiveGrid.Boundaries"/></param>
        /// <returns>New obstacle object.</returns>
        public static Obstacle FromBBox(BBox3 box, double offset = 0, bool perimeter = false, bool allowOutsideBoundary = false)
        {
            var minZ = box.Corners().Min(x => x.Z);
            var maxZ = box.Corners().Max(x => x.Z);

            var minZVertices = box.Corners().Where(x => x.Z.ApproximatelyEquals(minZ)).ToList();

            var filteredVerices = new List<Vector3>();

            foreach(var vertex in minZVertices)
            {
                if(filteredVerices.Any(x => x.IsAlmostEqualTo(vertex)))
                {
                    continue;
                }

                filteredVerices.Add(vertex);
            }

            var orderedVerices = filteredVerices.OrderBy(x => x.DistanceTo(filteredVerices.First())).ToList();
            var polygon = new Polygon(orderedVerices[0], orderedVerices[1], orderedVerices[3], orderedVerices[2]);

            var height = maxZ - minZ;

            return new Obstacle(polygon, height, offset, perimeter, allowOutsideBoundary, null);
        }

        /// <summary>
        /// Create an obstacle from a line.
        /// </summary>
        /// <param name="line">Line to avoid.</param>
        /// <param name="offset">Extra space around obstacle bounding box. Should be larger than 0.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <param name="allowOutsideBoundary">Should edges be created when obstacle is outside of <see cref="AdaptiveGrid.Boundaries"/></param>
        /// <returns>New obstacle object.</returns>
        public static Obstacle FromLine(Line line, double offset = 0.1, bool perimeter = false, bool allowOutsideBoundary = false)
        {
            if (offset < Vector3.EPSILON)
            {
                throw new ArgumentException("Offset should be larger then zero.");
            }

            var frame = line.TransformAt(0);

            var polygon = new Polygon
            (
                line.Start + offset * (frame.XAxis + frame.YAxis),
                line.Start - offset * (frame.XAxis + frame.YAxis),
                line.End - offset * (frame.XAxis + frame.YAxis),
                line.End + offset * (frame.XAxis + frame.YAxis)
            );

            var height = offset * 2;

            return new Obstacle(polygon, height, offset, perimeter, allowOutsideBoundary, frame);
        }

        /// <summary>
        /// Create an obstacle from a list of points.
        /// </summary>
        /// <param name="boudary">Boundary of an obstacle</param>
        /// <param name="offset">Extra space around obstacle bounding box.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <param name="allowOutsideBoundary">Should edges be created when obstacle is outside of <see cref="AdaptiveGrid.Boundaries"/></param>
        /// <param name="transformation">Transformation of the obstacle.</param>
        public Obstacle(Polygon boudary, double height, double offset, bool perimeter, bool allowOutsideBoundary, Transform transformation)
        {
            Boundary = boudary;
            Height = height;
            Offset = offset;
            Perimeter = perimeter;
            AllowOutsideBoudary = allowOutsideBoundary;
            Transform = transformation;

            UpdatePolygons();
        }

        /// <summary>
        /// List of points defining obstacle.
        /// </summary>
        public List<Vector3> Points => Boundary
            .Vertices
            .SelectMany(x => new List<Vector3> { x, x + new Vector3(0, 0, Height) })
            .ToList();
        
        public Polygon Boundary { get; }

        public double Height { get; }

        /// <summary>
        /// Offset of bounding box created from the list of points.
        /// </summary>
        public double Offset 
        {
            get => _offset;
            set
            {
                _offset = value;
                UpdatePolygons();
            }
        }

        /// <summary>
        /// Should edges be created around obstacle.
        /// If false - any intersected edges are just discarded.
        /// If true - intersected edges are cut to obstacle and perimeter edges are inserted.
        /// </summary>
        public bool Perimeter { get; set; }

        /// <summary>
        /// Should edges be created when obstacle is outside <see cref="AdaptiveGrid.Boundaries"/>, it will work only when <see cref="Perimeter"/> property is true />
        /// </summary>
        public bool AllowOutsideBoudary { get; set; }

        /// <summary>
        /// Transformation of bounding box created from the list of points.
        /// </summary>
        public Transform Transform 
        {
            get => _transform;
            set
            {
                _transform = value;
                UpdatePolygons();
            }      
        }

        /// <summary>
        /// Check if any segment of polyline intersects with obstacle or is inside of obstacle
        /// </summary>
        /// <param name="polyline">Polyline to check</param>
        /// <param name="tolerance">Tolerance of checks</param>
        /// <returns>Result of check</returns>
        public bool Intersects(Polyline polyline, double tolerance = 1e-05)
        {
            return polyline.Segments().Any(s => Intersects(s, tolerance));
        }

        /// <summary>
        /// Check if line intersects with obstacle
        /// </summary>
        /// <param name="line">Line to check</param>
        /// <param name="tolerance">Tolerance of checks</param>
        /// <returns>Result of check</returns>
        public bool Intersects(Line line, double tolerance = 1e-05)
        {
            if (_secondaryPolygons.Any(x => IntersectsWithHorizontalPolygon(x, line, tolerance)))
            {
                return true;
            }

            var hints = _primaryPolygons.Where(x => DoesPolygonIntersectsWithLine(x, line, tolerance)).ToList();
            return hints.Any() && hints.Count % 2 == 0;
        }

        private bool DoesPolygonIntersectsWithLine(Polygon polygon, Line line, double tolerance = 1e-05)
        {
            var plane = polygon.Plane();
            if(!line.Intersects(plane, out var intersection, true))
            {
                return false;
            }

            if(!polygon.Contains3D(intersection))
            {
                return false;
            }

            if(line.PointOnLine(intersection))
            {
                return true;
            }

            var distance = plane.SignedDistanceTo(intersection);

            return plane.Normal.IsParallelTo(line.Direction()) && (distance.ApproximatelyEquals(0, tolerance) || distance < 0);
            
        }

        private bool IntersectsWithHorizontalPolygon(Polygon polygon, Line line, double tolerance = 1e-05)
        {
            if(polygon.Contains3D(line.Start) || polygon.Contains3D(line.End))
            {
                return true;
            }

            if(!polygon.Intersects(line, out var intersections, false, includeEnds: true))
            {
                return false;
            }

            return intersections.Any() && intersections.Count % 2 == 0;
        }

        private void UpdatePolygons()
        {
            if(Boundary == null)
            {
                return;
            }

            _primaryPolygons.Clear();
            _secondaryPolygons.Clear();

            var boundary = Boundary.Offset(Offset).First();
            if(Transform != null)
            {
                boundary = boundary.TransformedPolygon(Transform);
            }

            if (!boundary.IsClockWise())
            {
                boundary = boundary.Reversed();
            }

            var offsetVector = boundary.Normal() * Offset;
            boundary = boundary.TransformedPolygon(new Transform(offsetVector));
            _secondaryPolygons.Add(boundary);

            var topVector = boundary.Normal().Negate() * (Height + 2 * Offset);

            if(topVector.IsAlmostEqualTo(Vector3.Origin))
            {
                return;
            }

            var topBoundary = boundary.TransformedPolygon(new Transform(topVector)).Reversed();
            _secondaryPolygons.Add(topBoundary);

            _primaryPolygons.AddRange(_secondaryPolygons);
            foreach (var segment in boundary.Segments())
            {
                var polygon = new Polygon(segment.Start, segment.End, segment.End + topVector, segment.Start + topVector).Reversed();
                _primaryPolygons.Add(polygon);
            }            
        }
    }
}
