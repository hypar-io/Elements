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
        private List<Vector3> _points;
        private double _offset;

        private readonly List<Plane> _horizontalPlanes = new List<Plane>();
        private readonly List<Plane> _verticalPlanes = new List<Plane>();

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
            List<Vector3> points = new List<Vector3>();
            points.AddRange(p.Vertices);
            points.AddRange(p.Vertices.Select(
                v => new Vector3(v.X, v.Y, v.Z + column.Height)));
            return new Obstacle(points, offset, perimeter, allowOutsideBoundary, null);
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
            List<Vector3> points = new List<Vector3>();
            points.Add(wall.CenterLine.Start + ortho * wall.Thickness / 2);
            points.Add(wall.CenterLine.End + ortho * wall.Thickness / 2);
            points.Add(wall.CenterLine.Start - ortho * wall.Thickness / 2);
            points.Add(wall.CenterLine.End - ortho * wall.Thickness / 2);
            points.AddRange(points.Select(v => new Vector3(v.X, v.Y, v.Z + wall.Height)).ToArray());
            var transfrom = new Transform(Vector3.Origin,
                wall.CenterLine.Direction(), ortho, Vector3.ZAxis);
            return new Obstacle(points, offset, perimeter, allowOutsideBoundary, transfrom);
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
            return new Obstacle(box.Corners(), offset, perimeter, allowOutsideBoundary, null);
        }

        /// <summary>
        /// Create an obstacle from a 2d polygon and height.
        /// </summary>
        /// <param name="polyon">2d polygon to avoid.</param>
        /// <param name="height">Height of the obstacle.</param>
        /// <param name="offset">Extra space around obstacle bounding box.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <param name="allowOutsideBoundary">Should edges be created when obstacle is outside of <see cref="AdaptiveGrid.Boundaries"/></param>
        /// <returns>New obstacle object.</returns>
        public static Obstacle From2DPolygon(Polygon polyon, double height, double offset = 0, bool perimeter = false, bool allowOutsideBoundary = false)
        {
            List<Vector3> points = new List<Vector3>();
            points.AddRange(polyon.Vertices.Select(p => new Vector3(p.X, p.Y)));
            points.AddRange(polyon.Vertices.Select(p => new Vector3(p.X, p.Y, height)));
            return new Obstacle(points, offset, perimeter, allowOutsideBoundary, null);
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

            List<Vector3> points = new List<Vector3>();
            points.Add(line.Start);
            points.Add(line.End);

            Transform frame = null;
            var forward = line.Direction();
            if (!forward.IsParallelTo(Vector3.ZAxis))
            {
                var rigth = forward.Cross(Vector3.ZAxis);
                var up = forward.Cross(rigth);
                frame = new Transform(Vector3.Origin, forward, rigth, up);
            }

            return new Obstacle(points, offset, perimeter,allowOutsideBoundary, frame);
        }

        /// <summary>
        /// Create an obstacle from a list of points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="offset">Extra space around obstacle bounding box.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <param name="allowOutsideBoundary">Should edges be created when obstacle is outside of <see cref="AdaptiveGrid.Boundaries"/></param>
        /// <param name="transformation">Transformation of the obstacle.</param>
        public Obstacle(List<Vector3> points, double offset, bool perimeter, bool allowOutsideBoundary, Transform transformation)
        {
            Points = points;
            Offset = offset;
            Perimeter = perimeter;
            AllowOutsideBoudary = allowOutsideBoundary;
            Transform = transformation;

            UpdatePlanes();
        }

        /// <summary>
        /// List of points defining obstacle.
        /// </summary>
        public List<Vector3> Points 
        {
            get => _points;
            set
            {
                _points = value;
                UpdatePlanes();
            }
        }

        /// <summary>
        /// Offset of bounding box created from the list of points.
        /// </summary>
        public double Offset 
        {
            get => _offset;
            set
            {
                _offset = value;
                UpdatePlanes();
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
                UpdatePlanes();
            }      
        }

        /// <summary>
        /// Check if any segment of polyline intersects with obstacle or is inside of obstacle
        /// </summary>
        /// <param name="polyline">Polyline to check</param>
        /// <param name="tolerance">Tolerance for checks</param>
        /// <returns>Result of check</returns>
        public bool Intersects(Polyline polyline, double tolerance = 1e-05)
        {
            var minX = Points.Min(x => x.X);
            var maxX = Points.Max(x => x.X);
            var minY = Points.Min(x => x.Y);
            var maxY = Points.Max(x => x.Y);
            var minZ = Points.Min(x => x.Z);
            var maxZ = Points.Max(x => x.Z);

            var domain = (new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));

            return polyline
                .Segments()
                .Any(x => AdaptiveGrid.IsLineInDomain((x.Start, x.End), domain, tolerance, tolerance, out var _, out var _));
        }
        private void UpdatePlanes()
        {
            _horizontalPlanes.Clear();
            _verticalPlanes.Clear();

            var transformedPoints = Transform != null
                ? Points.Select(x => Transform.OfPoint(x)).ToList()
                : Points;

            var minZ = transformedPoints.Min(x => x.Z);
            var maxZ = transformedPoints.Max(x => x.Z);

            _horizontalPlanes.Add(new Plane(new Vector3(0, 0, minZ - Offset), Vector3.ZAxis.Negate()));

            var heightDifference = maxZ - minZ + Offset * 2;
            if (!heightDifference.ApproximatelyEquals(0))
            {
                _horizontalPlanes.Add(new Plane(new Vector3(0, 0, maxZ + Offset), Vector3.ZAxis));
            }

            var minZPolygon = CreatePolygonFromPoints(transformedPoints.Where(x => x.Z.ApproximatelyEquals(minZ)).ToList(), Vector3.ZAxis.Negate());
            foreach (var segment in minZPolygon.Segments())
            {
                var polygon = new Polygon(segment.Start, segment.End, segment.End + Vector3.ZAxis, segment.Start + Vector3.ZAxis);   
                _verticalPlanes.Add(polygon.Plane());
            }            
        }

        private Polygon CreatePolygonFromPoints(List<Vector3> vertices, Vector3 offsetDirection)
        {
            var filteredVerticies = new List<Vector3>();
            foreach (var vertex in vertices)
            {
                if (filteredVerticies.Any(x => x.IsAlmostEqualTo(vertex)))
                {
                    continue;
                }
                filteredVerticies.Add(vertex);
            }

            var orderedVertices = filteredVerticies.OrderBy(x => x.DistanceTo(filteredVerticies.First())).ToList();
            var polygon =  new Polygon(orderedVertices[0], orderedVertices[1], orderedVertices[3], orderedVertices[2]);

            var offsetVector = offsetDirection * Offset;
            var transform = new Transform(offsetVector);
            var offsetPolygon = polygon.Offset(Offset).First();
            return offsetPolygon.TransformedPolygon(transform);
        }

        private static bool IsBetweenPlanesOrOnAnyOfThem(List<Plane> planes, Vector3 point, double tolerance = 1e-05) => planes.
            Select(x => x.SignedDistanceTo(point)).All(x => x.ApproximatelyEquals(0, tolerance) || x < 0);

        private static bool IsBetweenPlanes(List<Plane> planes, Vector3 point, double tolerance = 1e-05) => planes
            .Select(x => x.SignedDistanceTo(point)).All(x => !x.ApproximatelyEquals(0, tolerance) && x < 0);

        private static List<Vector3> GetIntersections(List<Plane> planes, Line line) => planes
            .Where(x => line.Intersects(x, out var _))
            .Select(x =>
            {
                line.Intersects(x, out var result);
                return result;
            }).ToList();
    }
}
