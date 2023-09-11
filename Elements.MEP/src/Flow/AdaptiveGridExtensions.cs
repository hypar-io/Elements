using Elements.Geometry;
using Elements.Spatial.AdaptiveGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Flow
{
    public static class AdaptiveGridExtensions
    {
        /// <summary>
        /// Make sure that grid doesn't go through other tree buts still have a possible way to go.
        /// This function is designed for grids that are mostly created on the same plane.
        /// If grid intersects with any tree, than main plain is copied into another plane, connecting two together.
        /// Intersected tree are subtracted from the grid, perimeter is created around.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="trees">Trees to subtract.</param>
        /// <param name="layer">Main layer of the grid.</param>
        /// <param name="connectionsOffset">Obstacle distance around trees.</param>
        /// <param name="verticalOffset">Vertical drop for auxiliary grid layer.</param>
        public static void SubtractTrees(
            this AdaptiveGrid grid, IEnumerable<Tree> trees,
            double layer, double connectionsOffset, double verticalOffset)
        {
            bool needDuplicate = false;
            var newEdges = new List<Edge>();

            // Store all edges positions before any of them are removed.
            var plane = new Plane(new Vector3(0, 0, layer), Vector3.ZAxis);
            var mainLayerSnapshot = grid.SnapshotEdgesOnPlane(plane);

            foreach (var tree in trees)
            {
                // Check if any connections of other tree intersecting any of edges in the grid.
                needDuplicate |= grid.SubtractTree(tree, connectionsOffset, newEdges);
            }

            if (needDuplicate)
            {
                mainLayerSnapshot.AddRange(grid.SnapshotEdgesOnPlane(plane, newEdges));
                // Create another graph plane that will combine all old edges from main layer
                // plus new edges created on the main layer by other tree subtraction.
                // This create "tunnels" around other tree.
                var transform = new Transform(0, 0, verticalOffset);
                grid.InsertSnapshot(mainLayerSnapshot, transform);
                foreach (var tree in trees)
                {
                    // Vertical connections need to be excluded once again after duplication is done.
                    // This is because vertical connection may also cut though new layer as well.
                    needDuplicate |= grid.SubtractTree(tree, connectionsOffset, newEdges, true);
                }
            }
        }

        /// <summary>
        /// Create an empty space in the grid distance around the tree, creating perimeter around.
        /// Connections of a tree as used as obstacle but there is one combined perimeter.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="tree">Tree to subtract.</param>
        /// <param name="offset">Obstacle distance around the tree.</param>
        /// <param name="newEdges">New perimeter edges added to the grid.</param>
        /// <param name="verticalOnly">Use only connections with vertical difference as obstacle.</param>
        /// <returns>True if tree intersected with any of grid edges.</returns>
        private static bool SubtractTree(
            this AdaptiveGrid grid, Tree tree, double offset, List<Edge> newEdges, bool verticalOnly = false)
        {
            bool anyIntersections = false;
            var lastEdgeIndex = grid.GetEdges().Max(e => e.Id);

            foreach (var connection in tree.Connections)
            {
                if (verticalOnly && Math.Abs(connection.Direction().Dot(Vector3.ZAxis)) < 1e-3)
                {
                    continue;
                }

                var path = connection.Path();
                var obstacle = Obstacle.FromLine(path, offset, addPerimeterEdges: true);

                if (grid.SubtractObstacle(obstacle))
                {
                    anyIntersections = true;
                }
            }

            // Complex obstacle, like other tree, should have complex boundaries.
            // As we don't have this not, we check if any connections are intersecting new edges.
            // Found intersections are excess perimeter edges of that need to be removed.
            if (anyIntersections)
            {
                // New edges are defined as whose who have id large than maximum before the operation.
                foreach (var e in grid.GetEdges().Where(e => e.Id > lastEdgeIndex).ToList())
                {
                    bool tooClose = false;
                    var line = grid.GetLine(e);
                    foreach (var c in tree.Connections)
                    {
                        var path = c.Path();
                        if (line.Intersects(path, out _) ||
                            path.Start.DistanceTo(line) < offset - grid.Tolerance ||
                            path.End.DistanceTo(line) < offset - grid.Tolerance ||
                            line.Start.DistanceTo(path) < offset - grid.Tolerance ||
                            line.End.DistanceTo(path) < offset - grid.Tolerance)
                        {
                            tooClose = true;
                            grid.RemoveEdge(e);
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        newEdges.Add(e);
                    }
                }
            }

            return anyIntersections;
        }
    }
}
