using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Elements;
using Elements.Geometry;
using Elements.Spatial.AdaptiveGrid;
using Xunit;
using Elements.Serialization.glTF;
using Vertex = Elements.Spatial.AdaptiveGrid.Vertex;
using static Elements.Spatial.AdaptiveGrid.AdaptiveGraphRouting;

namespace Elements.Tests
{
    public class AdaptiveGraphRoutingTests : ModelTest
    {
        [Fact]
        public void AdaptiveGraphRoutingDijkstraPath()
        {
            Polygon region = new Polygon(new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 10, 0),
                new Vector3(0, 10, 0)
            });

            //Split rectangle into 4 cells.
            var keyPoints = new List<Vector3>()
            {
                new Vector3(5, 4, 0)
            };

            //Remove center vertex leaving only 8 perimeter vertices
            AdaptiveGrid grid = new AdaptiveGrid();
            grid.AddFromPolygon(region, keyPoints);
            var obstacle = Obstacle.FromBBox(new BBox3(
                new Vector3(3, 3, 0), new Vector3(7, 7, 0)));
            grid.SubtractObstacle(obstacle);

            //Each turn cost 1 additional "meter"
            var configuration = new AdaptiveGraphRouting.RoutingConfiguration(turnCost: 1);
            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, configuration);
            grid.TryGetVertexIndex(new Vector3(0, 4, 0), out var inV, grid.Tolerance);

            //Set travel cost for each edge equal to it's length
            var edgeCosts = new Dictionary<ulong, EdgeInfo>();
            foreach (var e in grid.GetEdges())
            {
                edgeCosts[e.Id] = new EdgeInfo(grid, e);
            }

            //Calculate all path from point (0, 4) to all other accessible points.
            var paths = alg.ShortestPathDijkstra(inV, edgeCosts, out var travelCosts);

            //Find most efficient path from (0, 4) to (10, 4)
            grid.TryGetVertexIndex(new Vector3(10, 4, 0), out var outV, grid.Tolerance);
            List<Vector3> expectedPath = new List<Vector3>()
            {
                new Vector3(0, 4, 0),
                new Vector3(0, 0, 0),
                new Vector3(5, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 4, 0)
            };

            //Go from end point (10, 4) backwards 
            var before = outV;
            for (int i = expectedPath.Count; i > 0; i--)
            {
                Assert.Equal(grid.GetVertex(before).Point, expectedPath[i - 1]);
                before = paths[before];
            }
            //After reaching start point before is pointing to 0 (nothing)
            Assert.True(before == 0);
            //Total cost is 4 + 5 + 5 + 4 + 1*2(two turns).
            Assert.Equal(20, travelCosts[outV]);

            //Find most efficient path from (0, 4) to (5, 10)
            Assert.True(grid.TryGetVertexIndex(new Vector3(5, 10, 0), out outV, grid.Tolerance));
            expectedPath = new List<Vector3>()
            {
                new Vector3(0, 4, 0),
                new Vector3(0, 10, 0),
                new Vector3(5, 10, 0)
            };

            before = outV;
            for (int i = expectedPath.Count; i > 0; i--)
            {
                Assert.Equal(grid.GetVertex(before).Point, expectedPath[i - 1]);
                before = paths[before];
            }
            Assert.True(before == 0);
            //Total cost is 6 + 5 + 1(turn).
            Assert.Equal(12, travelCosts[outV]);
        }

        [Fact]
        public void AdaptiveGraphRoutingDijkstraBranches()
        {
            Polygon region = new Polygon(new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 3, 0),
                new Vector3(0, 3, 0)
            });

            var keyPoints = new List<Vector3>()
            {
                new Vector3(2, 1, 0),
                new Vector3(5, 1, 0),
                new Vector3(8, 2, 0)
            };

            AdaptiveGrid grid = new AdaptiveGrid();
            grid.AddFromPolygon(region, keyPoints);

            var configuration = new RoutingConfiguration(turnCost: 1);
            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, configuration);

            Assert.True(grid.TryGetVertexIndex(new Vector3(2, 2, 0), out var inV, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(5, 2, 0), out var ev0, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(8, 2, 0), out var ev1, grid.Tolerance));

            var edgeCosts = new Dictionary<ulong, EdgeInfo>();

            foreach (var e in grid.GetEdges())
            {
                //Set travel cost for each edge equal to it's length.
                //Except (5, 2) -> (8, 2) for which it's 0.9 of length.
                bool startMatch = e.StartId == ev0 || e.StartId == ev1;
                bool endMatch = e.EndId == ev0 || e.EndId == ev1;
                var factor = startMatch && endMatch ? 0.9 : 1;
                edgeCosts[e.Id] = new EdgeInfo(grid, e, factor);
            }

            Assert.True(grid.TryGetVertexIndex(new Vector3(2, 3, 0), out var preV, grid.Tolerance));
            var paths = alg.ShortestBranchesDijkstra(inV, edgeCosts, out var travelCosts, preV);

            //Two paths calculated for (2, 3)->(8, 1) to approach end point from different edges.
            Assert.True(grid.TryGetVertexIndex(new Vector3(8, 1, 0), out var outV, grid.Tolerance));
            List<Vector3> expectedLeft = new List<Vector3>()
            {
                new Vector3(2, 2, 0),
                new Vector3(2, 1, 0),
                new Vector3(5, 1, 0),
                new Vector3(8, 1, 0),
            };

            //Go from end point (10, 4) backwards. At each point there are two items: best routes.
            //They are accompanied with links that says how recursively travel deeper.
            var before = paths[outV];
            var leftBranch = before.Item1;
            for (int i = expectedLeft.Count - 1; i > 0; i--)
            {
                Assert.Equal(grid.GetVertex(leftBranch.Item1).Point, expectedLeft[i - 1]);
                before = paths[leftBranch.Item1];
                leftBranch = leftBranch.Item2 == BranchSide.Left ? before.Item1 : before.Item2;
            }
            Assert.True(leftBranch.Item1 == 0);
            //Total cost is 1 + 3 + 3 + 1(turn).
            Assert.Equal(8, travelCosts[outV].Item1);

            List<Vector3> expectedRight = new List<Vector3>()
            {
                new Vector3(2, 2, 0),
                new Vector3(5, 2, 0),
                new Vector3(8, 2, 0),
                new Vector3(8, 1, 0),
            };

            before = paths[outV];
            var rightBranch = before.Item2;
            for (int i = expectedRight.Count - 1; i > 0; i--)
            {
                Assert.Equal(grid.GetVertex(rightBranch.Item1).Point, expectedRight[i - 1]);
                before = paths[rightBranch.Item1];
                rightBranch = rightBranch.Item2 == BranchSide.Left ? before.Item1 : before.Item2;
            }
            Assert.True(rightBranch.Item1 == 0);
            //Total cost is 3 + 2.7 + 1 + 0.9(aligned turn is also discounted).
            Assert.Equal(8.6, travelCosts[outV].Item2);
        }

        [Fact]
        public void AdaptiveGraphRoutingGeneralTest()
        {
            Polygon mainRegionBoundary = new Polygon(new List<Vector3>()
            {
                new Vector3(0, 0, 1),
                new Vector3(10, 0, 1),
                new Vector3(10, 10, 1),
                new Vector3(0, 10, 1)
            });

            Polygon auxilaryRegionBoundary = new Polygon(new List<Vector3>()
            {
                new Vector3(0, 10, 0),
                new Vector3(10, 10, 0),
                new Vector3(10, 10, 3),
                new Vector3(0, 10, 3)
            });

            var configuration = new RoutingConfiguration(
                turnCost: 1, mainLayer: 2, layerPenalty: 2);

            var inputPoints = new List<Vector3>()
            {
                new Vector3(5, 2, 3),
                new Vector3(5, 5, 3),
                new Vector3(5, 8, 3)
            };

            var tailPoints = new List<Vector3>()
            {
                new Vector3(5, 10, 0),
                new Vector3(5, 10, 2),
                new Vector3(6, 10, 2)
            };

            var keyPoints = new List<Vector3>();
            keyPoints.AddRange(inputPoints);
            keyPoints.AddRange(tailPoints);

            var hintPolyline = new Polyline(new List<Vector3>()
            {
                new Vector3(6, 6),
                new Vector3(3, 6),
                new Vector3(3, 7),
                new Vector3(6, 7),

            });
            keyPoints.AddRange(hintPolyline.Vertices.Select(
                v => new Vector3(v.X, v.Y, configuration.MainLayer)));

            var offsetPolyline = new Polyline(new List<Vector3>()
            {
                new Vector3(6, 2),
                new Vector3(6, 10)
            });
            keyPoints.AddRange(offsetPolyline.Vertices.Select(
                v => new Vector3(v.X, v.Y, configuration.MainLayer)));

            var hints = new List<RoutingHintLine>();
            hints.Add(new RoutingHintLine(hintPolyline, 0.1, 0.2, true));
            hints.Add(new RoutingHintLine(offsetPolyline, 0.5, 0.1, false));

            var box = new BBox3(new Vector3(3, 6, 0), new Vector3(7, 7, 3));
            var obstacle = Obstacle.FromBBox(box);
            keyPoints.AddRange(box.Corners());

            AdaptiveGrid grid = new AdaptiveGrid();
            grid.AddFromExtrude(mainRegionBoundary, Vector3.ZAxis, 1, keyPoints);
            grid.AddFromPolygon(auxilaryRegionBoundary, keyPoints);
            foreach (var input in inputPoints)
            {
                var p = new Vector3(input.X, input.Y, configuration.MainLayer);
                Assert.True(grid.TryGetVertexIndex(p, out ulong down, grid.Tolerance));
                grid.AddVertex(input, new ConnectVertexStrategy(grid.GetVertex(down)));
            }
            grid.SubtractObstacle(obstacle);

            var inputVertices = new List<RoutingVertex>();
            foreach (var input in inputPoints)
            {
                Assert.True(grid.TryGetVertexIndex(input, out ulong id, grid.Tolerance));
                {
                    inputVertices.Add(new RoutingVertex(id, 0.5));
                }
            }
            List<ulong> tailVertices = new List<ulong>();
            foreach (var tail in tailPoints)
            {
                Assert.True(grid.TryGetVertexIndex(tail, out ulong id, grid.Tolerance));
                {
                    tailVertices.Add(id);
                }
            }

            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, configuration);
            var tree = alg.BuildSpanningTree(inputVertices, tailVertices, hints, TreeOrder.ClosestToFurthest);

            List<Vector3> expectedPath = new List<Vector3>()
            {
                new Vector3(5, 8, 3),
                new Vector3(5, 8, 2),
                new Vector3(5, 7, 2),
                new Vector3(6, 7, 2),
                new Vector3(6, 8, 2),
                new Vector3(6, 10, 2),
                new Vector3(5, 10, 2),
                new Vector3(5, 10, 1),
                new Vector3(5, 10, 0)
            };

            CheckTree(grid, inputVertices[2].Id, tree, expectedPath);

            expectedPath = new List<Vector3>()
            {
                new Vector3(5, 2, 3),
                new Vector3(5, 2, 2),
                new Vector3(6, 2, 2),
                new Vector3(6, 5, 2),
                new Vector3(6, 6, 2),
                new Vector3(5, 6, 2),
                new Vector3(3, 6, 2),
                new Vector3(3, 7, 2),
                new Vector3(5, 7, 2),
                new Vector3(6, 7, 2),
                new Vector3(6, 8, 2),
                new Vector3(6, 10, 2),
                new Vector3(5, 10, 2),
                new Vector3(5, 10, 1),
                new Vector3(5, 10, 0),
            };

            CheckTree(grid, inputVertices[0].Id, tree, expectedPath);
        }

        [Fact]
        public void AdaptiveGraphRoutingSimpleTreeExample()
        {
            this.Name = "Adaptive_Graph_Routing_Simple";
            //1. Define boundaries of the grid. 
            Polygon boundary = new Polygon(new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(20, 0, 0),
                new Vector3(20, 25, 0),
                new Vector3(0, 25, 0)
            });

            //2. Define start points.
            var inputPoints = new List<Vector3>()
            {
                new Vector3(2, 5, 0),
                new Vector3(2, 10, 0),
                new Vector3(8, 15, 0),
                new Vector3(12, 5, 0),
                new Vector3(9, 8, 0),
                new Vector3(11, 12, 0),
            };

            //3. Define end points. Last should go first.
            var tailPoints = new List<Vector3>()
            {
                new Vector3(12, 20, 0),
                new Vector3(10, 20, 0)
            };

            //4. All significant points must be added as key points.
            var keyPoints = new List<Vector3>();
            keyPoints.AddRange(inputPoints);
            keyPoints.AddRange(tailPoints);

            //5. Define hint and offset lines.
            var firstOffsetPolyline = new Polyline(new List<Vector3>(){
                new Vector3(5, 2),
                new Vector3(5, 20)
            });
            keyPoints.AddRange(firstOffsetPolyline.Vertices);

            var secondOffsetPolyline = new Polyline(new List<Vector3>()
            {
                new Vector3(15, 2),
                new Vector3(15, 20)
            });
            keyPoints.AddRange(secondOffsetPolyline.Vertices);

            var hintPolyline = new Polyline(new List<Vector3>()
            {
                new Vector3(5, 17),
                new Vector3(15, 17),

            });
            keyPoints.AddRange(hintPolyline.Vertices);

            //6. Define obstacles.
            var box = new BBox3(new Vector3(11, 14, 0), new Vector3(17, 18, 0));
            var obstacle = Obstacle.FromBBox(box);
            keyPoints.AddRange(box.Corners());

            //7. Create grid.
            AdaptiveGrid grid = new AdaptiveGrid();
            grid.AddFromPolygon(boundary, keyPoints);
            grid.SubtractObstacle(obstacle);

            //8. Get indices's for start and end vertices
            var inputVertices = new List<RoutingVertex>();
            foreach (var input in inputPoints)
            {
                Assert.True(grid.TryGetVertexIndex(input, out ulong id, grid.Tolerance));
                {
                    inputVertices.Add(new RoutingVertex(id, 0.5));
                }
            }

            List<ulong> tailVertices = new List<ulong>();
            foreach (var tail in tailPoints)
            {
                Assert.True(grid.TryGetVertexIndex(tail, out ulong id, grid.Tolerance));
                {
                    tailVertices.Add(id);
                }
            }

            //9. Set configurations for hint and offset lines.
            var hint = new RoutingHintLine(hintPolyline, 0.01, 0.1, true);
            var offset1 = new RoutingHintLine(firstOffsetPolyline, 0.9, 0.1, false);
            var offset2 = new RoutingHintLine(secondOffsetPolyline, 0.9, 0.1, false);
            var hints = new List<RoutingHintLine> { hint, offset1, offset2 };

            //10. Run algorithm
            var config = new RoutingConfiguration(turnCost: 1);
            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, config);
            var tree = alg.BuildSpanningTree(inputVertices, tailVertices, hints, TreeOrder.ClosestToFurthest);

            //Throws if no hint lines
            Assert.Throws<ArgumentException>(() =>
                alg.BuildSpanningTree(inputVertices, tailVertices, new List<RoutingHintLine>(), TreeOrder.ClosestToFurthest));

            //Results visualization
            List<Line> lines = new List<Line>();
            foreach (var input in inputVertices)
            {
                var v0 = input.Id;
                var v1 = tree[v0];
                while (v1.HasValue && v1 != 0)
                {
                    lines.Add(new Line(grid.GetVertex(v0).Point, grid.GetVertex(v1.Value).Point));
                    v0 = v1.Value;
                    v1 = tree[v0];
                }
            }
            ModelLines ml = new ModelLines(lines, new Material("", new Color("red")));
            this.Model.AddElements(alg.RenderElements(hints, keyPoints));
            this.Model.AddElement(ml);
        }

        [Fact]
        public void AdaptiveGraphRoutingGroupedTreeExample()
        {
            this.Name = "Adaptive_Graph_Routing_Groups";
            //1. Define boundaries of the grid. 
            Polygon boundary = new Polygon(new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(20, 0, 0),
                new Vector3(20, 25, 0),
                new Vector3(0, 25, 0)
            });

            //2. Define start points.
            var inputPoints = new List<Vector3>()
            {
                new Vector3(2, 5, 0),
                new Vector3(2, 10, 0),
                new Vector3(8, 15, 0),
                new Vector3(12, 5, 0),
                new Vector3(9, 8, 0),
                new Vector3(11, 12, 0),
            };

            //3. Define end points. Last should go first.
            var tailPoints = new List<Vector3>()
            {
                new Vector3(12, 20, 0),
                new Vector3(10, 20, 0)
            };

            //3a. Define local tail points, one per group
            var localTailPoints = new List<Vector3>()
            {
                new Vector3(5, 16, 0),
                new Vector3(15, 13, 0)
            };

            //4. All significant points must be added as key points.
            var keyPoints = new List<Vector3>();
            keyPoints.AddRange(inputPoints);
            keyPoints.AddRange(localTailPoints);
            keyPoints.AddRange(tailPoints);

            //5. Define hint and offset lines.
            var firstOffsetPolyline = new Polyline(new List<Vector3>(){
                new Vector3(5, 2),
                new Vector3(5, 20)
            });
            keyPoints.AddRange(firstOffsetPolyline.Vertices);

            var secondOffsetPolyline = new Polyline(new List<Vector3>()
            {
                new Vector3(15, 2),
                new Vector3(15, 20)
            });
            keyPoints.AddRange(secondOffsetPolyline.Vertices);

            var hintPolyline = new Polyline(new List<Vector3>()
            {
                new Vector3(5, 17),
                new Vector3(15, 17),

            });
            keyPoints.AddRange(hintPolyline.Vertices);

            //6. Define obstacles.
            var box = new BBox3(new Vector3(11, 14, 0), new Vector3(17, 18, 0));
            var obstacle = Obstacle.FromBBox(box);
            keyPoints.AddRange(box.Corners());

            //7. Create grid.
            AdaptiveGrid grid = new AdaptiveGrid();
            grid.AddFromPolygon(boundary, keyPoints);
            grid.SubtractObstacle(obstacle);

            //8. Get indices's for start, end vertices, local tail vertices.
            //Split input vertices into groups.
            var inputVertices = new List<List<RoutingVertex>>();
            inputVertices.Add(new List<RoutingVertex>());
            foreach (var input in inputPoints.Take(3))
            {
                Assert.True(grid.TryGetVertexIndex(input, out ulong id, grid.Tolerance));
                {
                    inputVertices[0].Add(new RoutingVertex(id, 0.5));
                }
            }
            inputVertices.Add(new List<RoutingVertex>());
            foreach (var input in inputPoints.Skip(3))
            {
                Assert.True(grid.TryGetVertexIndex(input, out ulong id, grid.Tolerance));
                {
                    inputVertices[1].Add(new RoutingVertex(id, 0.5));
                }
            }

            List<ulong> localTailVertices = new List<ulong>();
            foreach (var tail in localTailPoints)
            {
                Assert.True(grid.TryGetVertexIndex(tail, out ulong id, grid.Tolerance));
                {
                    localTailVertices.Add(id);
                }
            }

            List<ulong> tailVertices = new List<ulong>();
            foreach (var tail in tailPoints)
            {
                Assert.True(grid.TryGetVertexIndex(tail, out ulong id, grid.Tolerance));
                {
                    tailVertices.Add(id);
                }
            }

            //9. Set configurations for hint and offset lines. Split them into groups.
            var hint = new RoutingHintLine(hintPolyline, 0.01, 0.1, true);
            var offset1 = new RoutingHintLine(firstOffsetPolyline, 0.9, 0.1, false);
            var offset2 = new RoutingHintLine(secondOffsetPolyline, 0.9, 0.1, false);
            var hints = new List<List<RoutingHintLine>>
            {
                new List<RoutingHintLine>{ hint, offset1},
                new List<RoutingHintLine>{ hint, offset2}
            };

            //10. Run algorithm
            var config = new RoutingConfiguration(turnCost: 1);
            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, config);
            var tree = alg.BuildSpanningTree(
                inputVertices, localTailVertices, tailVertices, hints, TreeOrder.ClosestToFurthest);

            //Throws if no hint lines
            Assert.Throws<ArgumentException>(() => alg.BuildSpanningTree(
                    inputVertices, localTailVertices, tailVertices, new List<List<RoutingHintLine>>(), TreeOrder.ClosestToFurthest));

            //Result visualization
            List<Line> lines = new List<Line>();
            foreach (var input in inputVertices.SelectMany(iv => iv))
            {
                var v0 = input.Id;
                var v1 = tree[v0];
                while (v1.HasValue && v1 != 0)
                {
                    lines.Add(new Line(grid.GetVertex(v0).Point, grid.GetVertex(v1.Value).Point));
                    v0 = v1.Value;
                    v1 = tree[v0];
                }
            }
            ModelLines ml = new ModelLines(lines, new Material("", new Color("red")));
            this.Model.AddElements(alg.RenderElements(hints.SelectMany(h => h).ToList(), keyPoints));
            this.Model.AddElement(ml);
        }

        [Fact]
        public void AdaptiveGridRoutingSimpleNetwork()
        {
            this.Name = "Adaptive_Grid_Routing_Simple_Network";

            //1. Define grid skeleton.
            var c1 = new List<Vector3> {
                new Vector3(2, 2, 0),
                new Vector3(8, 2, 0),
                new Vector3(8, 8, 0),
                new Vector3(2, 8, 0),
                new Vector3(2, 2, 0),
            };
            var c2 = new List<Vector3> {
                new Vector3(4, 0, 0),
                new Vector3(4, 2, 0),
            };
            var c3 = new List<Vector3> {
                new Vector3(6, 8, 0),
                new Vector3(6, 10, 0),
            };
            var c4 = new List<Vector3> {
                new Vector3(0, 5, 0),
                new Vector3(2, 5, 0),
            };
            var c5 = new List<Vector3> {
                new Vector3(8, 5, 0),
                new Vector3(10, 5, 0),
            };

            //2. Create grid for vertex strips.
            AdaptiveGrid grid = new AdaptiveGrid();
            var corridors = new List<List<Vector3>> { c1, c2, c3, c4, c5 };
            foreach (var c in corridors)
            {
                grid.AddVertices(c, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);
            }

            //3. Define input vertices.
            var inputPoints = new List<Vector3>()
            {
                new Vector3(4, 0, 0),
                new Vector3(6, 10, 0)
            };

            var inputVertices = new List<RoutingVertex>();
            foreach (var input in inputPoints)
            {
                Assert.True(grid.TryGetVertexIndex(input, out ulong id, grid.Tolerance));
                {
                    inputVertices.Add(new RoutingVertex(id, 0));
                }
            }

            //4. Define exit vertices
            grid.TryGetVertexIndex(new Vector3(0, 5, 0), out var id1);
            grid.TryGetVertexIndex(new Vector3(10, 5, 0), out var id2);
            var exits = new List<ulong> { id1, id2 };

            //5. Run routing without hint lines.
            var config = new RoutingConfiguration(turnCost: 1);
            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, config);
            var tree = alg.BuildSimpleNetwork(inputVertices, exits, new List<RoutingHintLine>());

            List<Vector3> expectedPath = new List<Vector3>()
            {
                new Vector3(4, 0, 0),
                new Vector3(4, 2, 0),
                new Vector3(2, 2, 0),
                new Vector3(2, 5, 0),
                new Vector3(0, 5, 0)
            };

            //Go from input vertex (4, 0) to closest exit vertex (0, 5).
            CheckTree(grid, inputVertices[0].Id, tree, expectedPath);

            //Find most efficient path from (0, 4) 
            expectedPath = new List<Vector3>()
            {
                new Vector3(6, 10, 0),
                new Vector3(6, 8, 0),
                new Vector3(8, 8, 0),
                new Vector3(8, 5, 0),
                new Vector3(10, 5, 0)
            };

            //Go from input vertex (6, 10) to closest exit vertex (10, 5).
            CheckTree(grid, inputVertices[1].Id, tree, expectedPath);

            //6. Run routing with a hint line.
            var hint = new RoutingHintLine(
                new Polyline(new Vector3[] { new Vector3(2, 2, 0), new Vector3(2, 8, 0) }),
                0.1, 0.1, false);
            tree = alg.BuildSimpleNetwork(inputVertices, exits, new List<RoutingHintLine> { hint });

            //Find most efficient path from (0, 4) 
            expectedPath = new List<Vector3>()
            {
                new Vector3(6, 10, 0),
                new Vector3(6, 8, 0),
                new Vector3(2, 8, 0),
                new Vector3(2, 5, 0),
                new Vector3(0, 5, 0)
            };

            //Go from input point (6, 10) to now closest exit vertex (0, 5).
            CheckTree(grid, inputVertices[1].Id, tree, expectedPath);
        }

        [Fact]
        public void AdaptiveGraphRoutingAngleCheck()
        {
            AdaptiveGrid grid = new AdaptiveGrid();
            var vertices = new Vector3[] {
                new Vector3(0, 0),
                new Vector3(5, 0),
                new Vector3(10, 0),
                new Vector3(10, 5),
                new Vector3(10, 10),
            };
            var strip = grid.AddVertices(vertices, AdaptiveGrid.VerticesInsertionMethod.Connect); 

            //Create two shortcuts - small 45 degree and long 30 degree.
            grid.AddEdge(strip[1].Id, strip[3].Id);
            grid.AddEdge(strip[1].Id, strip[4].Id);

            // With only 90 degree allowed - routing uses no shortcut.
            var c = new RoutingConfiguration(supportedAngles: new List<double>() { 90 });
            var routing = new AdaptiveGraphRouting(grid, c);
            var start = new RoutingVertex(strip[0].Id, 0);
            var route = routing.BuildSimpleNetwork(new List<RoutingVertex> { start }, new List<ulong> { strip[4].Id }, new List<RoutingHintLine> { });
            var expectedPath = new List<Vector3>()
            {
                new Vector3(0, 0),
                new Vector3(5, 0),
                new Vector3(10, 0),
                new Vector3(10, 5),
                new Vector3(10, 10),
            };
            CheckTree(grid, start.Id, route, expectedPath);

            // With 45 and 90 degree allowed - routing uses 45 degree shortcut.
            c = new RoutingConfiguration(supportedAngles: new List<double>() { 45, 90 });
            routing = new AdaptiveGraphRouting(grid, c);
            route = routing.BuildSimpleNetwork(new List<RoutingVertex> { start }, new List<ulong> { strip[4].Id }, new List<RoutingHintLine> { });
            expectedPath = new List<Vector3>()
            {
                new Vector3(0, 0),
                new Vector3(5, 0),
                new Vector3(10, 5),
                new Vector3(10, 10),
            };
            CheckTree(grid, start.Id, route, expectedPath);

            // When angles are not specified - any is allowed. In this case routing uses the best 30 degree shortcut.
            c = new RoutingConfiguration();
            routing = new AdaptiveGraphRouting(grid, c);
            route = routing.BuildSimpleNetwork(new List<RoutingVertex> { start }, new List<ulong> { strip[4].Id }, new List<RoutingHintLine> { });
            expectedPath = new List<Vector3>()
            {
                new Vector3(0, 0),
                new Vector3(5, 0),
                new Vector3(10, 10),
            };
            CheckTree(grid, start.Id, route, expectedPath);
        }

        [Fact]
        public void AdaptiveGraphRoutingFilterCheck()
        {
            AdaptiveGrid grid = new AdaptiveGrid();

            // Shorter path need to go down up and down 
            var strip = grid.AddVertices(new Vector3[] {
                new Vector3(0, 0, 5),
                new Vector3(0, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(2, 0, 1),
                new Vector3(4, 0, 1),
                new Vector3(4, 0, 0),
                new Vector3(5, 0, 0)
            }, AdaptiveGrid.VerticesInsertionMethod.Connect);

            // Longer path goes around.
            var sideStrip = grid.AddVertices(new Vector3[]
            {
                new Vector3(2, 0, 0),
                new Vector3(2, 5, 0),
                new Vector3(4, 5, 0),
                new Vector3(4, 0, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.Connect);

            // Without any filters shortest path is taken.
            var c = new RoutingConfiguration();
            var routing = new AdaptiveGraphRouting(grid, c);
            var start = new RoutingVertex(strip[0].Id, 0);
            var route = routing.BuildSimpleNetwork(new List<RoutingVertex> { start }, new List<ulong> { strip.Last().Id }, new List<RoutingHintLine> { });
            var expectedPath = new List<Vector3>()
            {
                new Vector3(0, 0, 5),
                new Vector3(0, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(2, 0, 1),
                new Vector3(4, 0, 1),
                new Vector3(4, 0, 0),
                new Vector3(5, 0, 0)
            };
            CheckTree(grid, start.Id, route, expectedPath);

            // When simple filter is used to prevent routing to go up - it's forced to go around.
            routing.AddRoutingFilter((Vertex start, Vertex end) => start.Point.Z > end.Point.Z - Vector3.EPSILON);
            route = routing.BuildSimpleNetwork(new List<RoutingVertex> { start }, new List<ulong> { strip.Last().Id }, new List<RoutingHintLine> { });
            expectedPath = new List<Vector3>()
            {
                new Vector3(0, 0, 5),
                new Vector3(0, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(2, 5, 0),
                new Vector3(4, 5, 0),
                new Vector3(4, 0, 0),
                new Vector3(5, 0, 0)
            };
            CheckTree(grid, start.Id, route, expectedPath);
        }

        private static void CheckTree(
            AdaptiveGrid grid, ulong startId, IDictionary<ulong, ulong?> tree, List<Vector3> expectedPath)
        {
            ulong? before = startId;
            for (int i = 0; i < expectedPath.Count; i++)
            {
                Assert.Equal(grid.GetVertex(before.Value).Point, expectedPath[i]);
                before = tree[before.Value];
            }
            //After reaching start point before is pointing to 0 (nothing)
            Assert.False(before.HasValue);
        }
    }
}
