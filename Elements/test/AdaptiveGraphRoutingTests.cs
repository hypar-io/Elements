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
            var configuration = new RoutingConfiguration(turnCost: 1);
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

            var tailPoint = new Vector3(5, 10, 0);

            var keyPoints = new List<Vector3>();
            keyPoints.AddRange(inputPoints);
            keyPoints.Add(tailPoint);

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
                new Vector3(6, 10),
                new Vector3(5, 10)
            });
            keyPoints.AddRange(offsetPolyline.Vertices.Select(
                v => new Vector3(v.X, v.Y, configuration.MainLayer)));

            var hints = new List<RoutingHintLine>();
            hints.Add(new RoutingHintLine(hintPolyline, 
                factor: 0.01, influence: 0.2, userDefined: true, is2D: true));
            hints.Add(new RoutingHintLine(offsetPolyline,
                factor: 0.1, influence: 0.1, userDefined: false, is2D: true));

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

            Assert.True(grid.TryGetVertexIndex(tailPoint, out ulong tailVertex, grid.Tolerance));

            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, configuration);
            var tree = alg.BuildSpanningTree(inputVertices, tailVertex, hints, TreeOrder.ClosestToFurthest);

            List<Vector3> expectedPath = new List<Vector3>()
            {
                new Vector3(5, 8, 3),
                new Vector3(5, 8, 2),
                new Vector3(6, 8, 2),
                new Vector3(6, 10, 2),
                new Vector3(5, 10, 2),
                new Vector3(5, 10, 1),
                new Vector3(5, 10, 0)
            };

            CheckTree(grid, inputVertices[2].Id, tree, expectedPath);

            expectedPath = new List<Vector3>()
            {
                new Vector3(5, 5, 3),
                new Vector3(5, 5, 2),
                new Vector3(5, 6, 2),
                new Vector3(3, 6, 2),
                new Vector3(3, 7, 2),
                new Vector3(5, 7, 2),
                new Vector3(6, 7, 2),
                new Vector3(6, 8, 2),
                new Vector3(6, 10, 2),
                new Vector3(5, 10, 2),
                new Vector3(5, 10, 1),
                new Vector3(5, 10, 0)
            };

            CheckTree(grid, inputVertices[1].Id, tree, expectedPath);

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

            //3. Define end point.
            var tailPoint = new Vector3(12, 20, 0);

            //4. All significant points must be added as key points.
            var keyPoints = new List<Vector3>();
            keyPoints.AddRange(inputPoints);
            keyPoints.Add(tailPoint);

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

            Assert.True(grid.TryGetVertexIndex(tailPoint, out ulong tailVertex, grid.Tolerance));

            //9. Set configurations for hint and offset lines.
            var hint = new RoutingHintLine(hintPolyline, 
                factor: 0.01, influence: 0.1, userDefined: true, is2D: true);
            var offset1 = new RoutingHintLine(firstOffsetPolyline,
                factor: 0.9, influence: 0.1, userDefined: false, is2D: true);
            var offset2 = new RoutingHintLine(secondOffsetPolyline,
                factor: 0.9, influence: 0.1, userDefined: false, is2D: true);
            var hints = new List<RoutingHintLine> { hint, offset1, offset2 };

            //10. Run algorithm
            var config = new RoutingConfiguration(turnCost: 1);
            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, config);
            var tree = alg.BuildSpanningTree(inputVertices, tailVertex, hints, TreeOrder.ClosestToFurthest);

            //Not throws if no hint lines
            alg.BuildSpanningTree(inputVertices, tailVertex, new List<RoutingHintLine>(), TreeOrder.ClosestToFurthest);

            //Results visualization
            VisualizeRoutingTree(grid, inputVertices, tree);
            VisualizeGrid(alg, hints, keyPoints);
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

            //3. Define end point.
            var tailPoint = new Vector3(12, 20, 0);

            //4. All significant points must be added as key points.
            var keyPoints = new List<Vector3>();
            keyPoints.AddRange(inputPoints);
            keyPoints.Add(tailPoint);

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

            Assert.True(grid.TryGetVertexIndex(tailPoint, out ulong tailVertex, grid.Tolerance));

            //9. Set configurations for hint and offset lines. Split them into groups.
            var hint = new RoutingHintLine(hintPolyline,
                factor: 0.01, influence: 0.1, userDefined: true, is2D: true);
            var offset1 = new RoutingHintLine(firstOffsetPolyline, 
                factor: 0.9, influence: 0.1, userDefined: false, is2D: true);
            var offset2 = new RoutingHintLine(secondOffsetPolyline, 
                factor: 0.9, influence: 0.1, userDefined: false, is2D: true);
            var hints = new List<List<RoutingHintLine>>
            {
                new List<RoutingHintLine>{ hint, offset1},
                new List<RoutingHintLine>{ hint, offset2}
            };

            //10. Run algorithm
            var config = new RoutingConfiguration(turnCost: 1);
            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, config);
            var tree = alg.BuildSpanningTree(
                inputVertices, tailVertex, hints, TreeOrder.ClosestToFurthest);

            //No throws if no hint lines
            alg.BuildSpanningTree(
                inputVertices, tailVertex, new List<List<RoutingHintLine>>(), TreeOrder.ClosestToFurthest);

            //Result visualization
            VisualizeRoutingTree(grid, inputVertices.SelectMany(iv => iv), tree);
            VisualizeGrid(alg, hints.SelectMany(h => h).ToList(), keyPoints);
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
                factor: 0.1, influence: 0.1, userDefined: false, is2D: true);
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

        [Fact]
        public void AdaptiveGraphRoutingIsolationRadiusCheck()
        {
            AdaptiveGrid grid = new AdaptiveGrid();

            // Straight path
            var strip = grid.AddVertices(new Vector3[] {
                new Vector3(0, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(4, 0, 0)
            }, AdaptiveGrid.VerticesInsertionMethod.Connect);

            // Longer path goes around.
            grid.AddVertices(new Vector3[]
            {
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(3, 1, 0),
                new Vector3(3, 0, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);

            // Without isolation distance shortest path is taken.
            var c = new RoutingConfiguration();
            var routing = new AdaptiveGraphRouting(grid, c);
            var first = new RoutingVertex(strip[0].Id, 0);
            var second = new RoutingVertex(strip[1].Id, 0);
            var route = routing.BuildSimpleNetwork(
                new List<RoutingVertex> { first, second },
                new List<ulong> { strip.Last().Id });
            var expectedPath = new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(3, 0, 0),
                new Vector3(4, 0, 0)
            };
            CheckTree(grid, first.Id, route, expectedPath);

            // When isolation radius is used inlets can't go one near another.
            first = new RoutingVertex(strip[0].Id, 0.1);
            second = new RoutingVertex(strip[1].Id, 0.1);
            route = routing.BuildSimpleNetwork(
                new List<RoutingVertex> { first, second },
                new List<ulong> { strip.Last().Id });
            expectedPath = new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(3, 1, 0),
                new Vector3(3, 0, 0),
                new Vector3(4, 0, 0)
            };
            CheckTree(grid, first.Id, route, expectedPath);
        }

        [Fact]
        public void AdaptiveGraphRouting3DHintLineCheck()
        {
            var grid = new AdaptiveGrid();
            grid.AddFromPolygon(Polygon.Rectangle(new Vector3(0, 0), new Vector3(10, 10)),
                                new List<Vector3> { new Vector3(2, 2), new Vector3(5, 5), new Vector3(8, 8) });

            var hintPath = new Vector3[] {
                new Vector3(2, 5, 0),
                new Vector3(3, 5, 0),
                new Vector3(4, 5, 1),
                new Vector3(5, 5, 1),
                new Vector3(6, 5, 0),
                new Vector3(7, 5, 0)
            };

            grid.AddVerticesWithCustomExtension(hintPath, 2);
            var hint = new RoutingHintLine(new Polyline(hintPath), 
                factor: 0.1, influence: 0, userDefined: true, is2D: false);

            var c = new RoutingConfiguration();
            var routing = new AdaptiveGraphRouting(grid, c);
            Assert.True(grid.TryGetVertexIndex(new Vector3(0, 5), out var inputId, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(10, 5), out var outputId, grid.Tolerance));
            var input = new RoutingVertex(inputId, 0);
            var route = routing.BuildSpanningTree(
                new List<RoutingVertex> { input }, outputId, new List<RoutingHintLine> { hint }, TreeOrder.ClosestToFurthest);

            var expectedPath = new List<Vector3>()
            {
                new Vector3(0, 5, 0),
                new Vector3(2, 5, 0),
                new Vector3(3, 5, 0),
                new Vector3(4, 5, 1),
                new Vector3(5, 5, 1),
                new Vector3(6, 5, 0),
                new Vector3(8, 5, 0),
                new Vector3(10, 5, 0),
            };
            CheckTree(grid, inputId, route, expectedPath);
        }

        [Fact]
        public void AdaptiveGraphRoutingEqualLeafCheck()
        {
            var grid = new AdaptiveGrid();
            grid.AddVertices(new Vector3[] {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.Connect);

            grid.AddVertices(new Vector3[] {
                new Vector3(2, 2, 0),
                new Vector3(2, 0, 0)
            }, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);

            grid.AddVertices(new Vector3[] {
                new Vector3(5, 10, 0),
                new Vector3(4, 10, 0),
                new Vector3(4, 0, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);

            grid.AddVertices(new Vector3[] {
                new Vector3(5, 10, 0),
                new Vector3(6, 10, 0),
                new Vector3(6, 0, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);

            grid.AddVertices(new Vector3[] {
                new Vector3(8, 2, 0),
                new Vector3(8, 0, 0)
            }, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);


            var c = new RoutingConfiguration();
            var routing = new AdaptiveGraphRouting(grid, c);
            Assert.True(grid.TryGetVertexIndex(new Vector3(2, 2), out var inputId0, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(5, 10), out var inputId1, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(8, 2), out var inputId2, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(0, 0), out var outputId, grid.Tolerance));
            var inputs = new List<RoutingVertex> {
                new RoutingVertex(inputId0, 0),
                new RoutingVertex(inputId1, 0),
                new RoutingVertex(inputId2, 0)
            };
            var tree = routing.BuildSpanningTree(inputs, outputId, new List<RoutingHintLine>(), TreeOrder.ClosestToFurthest);

            var expectedPath = new List<Vector3>()
            {
                new Vector3(5, 10, 0),
                new Vector3(4, 10, 0),
                new Vector3(4, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(0, 0, 0)
            };
            CheckTree(grid, inputId1, tree, expectedPath);

            Assert.True(grid.TryGetVertexIndex(new Vector3(10, 0), out outputId, grid.Tolerance));
            tree = routing.BuildSpanningTree(inputs, outputId, new List<RoutingHintLine>(), TreeOrder.ClosestToFurthest);

            expectedPath = new List<Vector3>()
            {
                new Vector3(5, 10, 0),
                new Vector3(6, 10, 0),
                new Vector3(6, 0, 0),
                new Vector3(8, 0, 0),
                new Vector3(10, 0, 0)
            };
            CheckTree(grid, inputId1, tree, expectedPath);
        }

        [Fact]
        public void AdaptiveGraphRoutingClosestLeafCheck()
        {
            var grid = new AdaptiveGrid();
            grid.AddVertices(new Vector3[] {
                new Vector3(0, 20, 0),
                new Vector3(0, 10, 0),
                new Vector3(10, 10, 0),
                new Vector3(10, 0, 0),
                new Vector3(20, 0, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.Connect);

            grid.AddVertices(new Vector3[] {
                new Vector3(10, 20, 0),
                new Vector3(8, 20, 0),
                new Vector3(8, 10, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);

            grid.AddVertices(new Vector3[] {
                new Vector3(10, 20, 0),
                new Vector3(12, 20, 0),
                new Vector3(12, 0, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);

            var c = new RoutingConfiguration();
            var routing = new AdaptiveGraphRouting(grid, c);
            Assert.True(grid.TryGetVertexIndex(new Vector3(0, 20), out var inputId0, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(10, 20), out var inputId1, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(20, 0), out var outputId, grid.Tolerance));
            var inputs = new List<RoutingVertex> {
                new RoutingVertex(inputId0, 1),
                new RoutingVertex(inputId1, 1),
            };
            var tree = routing.BuildSpanningTree(inputs, outputId, new List<RoutingHintLine>(), TreeOrder.ClosestToFurthest);

            var expectedPath = new List<Vector3>()
            {
                new Vector3(10, 20, 0),
                new Vector3(8, 20, 0),
                new Vector3(8, 10, 0),
                new Vector3(10, 10, 0),
                new Vector3(10, 0, 0),
                new Vector3(12, 0, 0),
                new Vector3(20, 0, 0),
            };
            CheckTree(grid, inputId1, tree, expectedPath);
        }

        [Fact]
        public void AdaptiveGraphRoutingCorrectTurn()
        {
            var grid = new AdaptiveGrid();
            grid.AddVertices(new Vector3[] {
                new Vector3(5, 15, 0),
                new Vector3(0, 15, 0),
                new Vector3(0, 0, 0)
            }, AdaptiveGrid.VerticesInsertionMethod.Connect);

            grid.AddVertices(new Vector3[] {
                new Vector3(5, 15, 0),
                new Vector3(5, 10, 0),
                new Vector3(-5, 10, 0),
                new Vector3(-5, 5, 0),
                new Vector3(5, 5, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);

            var c = new RoutingConfiguration();
            var routing = new AdaptiveGraphRouting(grid, c);
            Assert.True(grid.TryGetVertexIndex(new Vector3(5, 5), out var inputId0, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(-5, 10), out var inputId1, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(5, 15), out var inputId2, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(0, 0), out var outputId, grid.Tolerance));
            var inputs = new List<RoutingVertex> {
                new RoutingVertex(inputId0, 1),
                new RoutingVertex(inputId1, 1),
                new RoutingVertex(inputId2, 1),
            };
            var tree = routing.BuildSpanningTree(inputs, outputId, new List<RoutingHintLine>(), TreeOrder.ClosestToFurthest);

            var expectedPath = new List<Vector3>()
            {
                new Vector3(5, 5, 0),
                new Vector3(0, 5, 0),
                new Vector3(0, 0, 0)
            };
            CheckTree(grid, inputId0, tree, expectedPath);

            //Second and third vertex can go to the side and then down or down and then to the side.
            //It should choose the direction of next path segment.
            expectedPath = new List<Vector3>()
            {
                new Vector3(-5, 10, 0),
                new Vector3(0, 10, 0),
                new Vector3(0, 5, 0),
                new Vector3(0, 0, 0)
            };
            CheckTree(grid, inputId1, tree, expectedPath);

            expectedPath = new List<Vector3>()
            {
                new Vector3(5, 15, 0),
                new Vector3(0, 15, 0),
                new Vector3(0, 10, 0),
                new Vector3(0, 5, 0),
                new Vector3(0, 0, 0)
            };
            CheckTree(grid, inputId2, tree, expectedPath);
        }

        [Fact]
        public void AdaptiveGraphRoutingGoesDown()
        {
            var grid = new AdaptiveGrid();
            grid.AddVertices(new Vector3[] {
                new Vector3(0, 0, 5),
                new Vector3(0, 0, 0),
                new Vector3(8, 0, 0),
                new Vector3(9, 0, 0),
                new Vector3(20, 0, 0),
                new Vector3(20, 0, -5)
            }, AdaptiveGrid.VerticesInsertionMethod.Connect);

            grid.AddVertices(new Vector3[] {
                new Vector3(5, 0, 5),
                new Vector3(5, 0, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);

            grid.AddVertices(new Vector3[] {
                new Vector3(10, 0, 5),
                new Vector3(10, 0, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);

            grid.AddVertices(new Vector3[] {
                new Vector3(15, 0, 5),
                new Vector3(15, 0, 0),
            }, AdaptiveGrid.VerticesInsertionMethod.ConnectAndCut);

            var snapshot = grid.SnapshotEdgesOnPlane(Plane.XY);
            var obstacle = Obstacle.FromLine(
                new Line(new Vector3(8.5, 0, 1), new Vector3(8.5, 0, -1)), offset: 0.5);
            Assert.True(grid.SubtractObstacle(obstacle));
            grid.InsertSnapshot(snapshot, new Transform(0, 0, -1), true);


            var c = new RoutingConfiguration(turnCost: 1, layerPenalty: 2);
            var routing = new AdaptiveGraphRouting(grid, c);
            routing.AddRoutingFilter((Vertex start, Vertex end) => start.Point.Z > end.Point.Z - Vector3.EPSILON);
            Assert.True(grid.TryGetVertexIndex(new Vector3(0, 0, 5), out var inputId0, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(5, 0, 5), out var inputId1, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(10, 0, 5), out var inputId2, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(15, 0, 5), out var inputId3, grid.Tolerance));
            Assert.True(grid.TryGetVertexIndex(new Vector3(20, 0, -5), out var outputId, grid.Tolerance));
            var inputs = new List<RoutingVertex> {
                new RoutingVertex(inputId0, 0),
                new RoutingVertex(inputId1, 0),
                new RoutingVertex(inputId2, 0),
                new RoutingVertex(inputId3, 0),
            };
            var tree = routing.BuildSpanningTree(inputs, outputId, new List<RoutingHintLine>(), TreeOrder.ClosestToFurthest);

            var expectedPath = new List<Vector3>()
            {
                new Vector3(15, 0, 5),
                new Vector3(15, 0, 0),
                new Vector3(15, 0, -1),
                new Vector3(20,  0, -1),
                new Vector3(20,  0, -5),
            };
            CheckTree(grid, inputId3, tree, expectedPath);

            expectedPath = new List<Vector3>()
            {
                new Vector3(10, 0, 5),
                new Vector3(10, 0, 0),
                new Vector3(10, 0, -1),
                new Vector3(15,  0, -1),
                new Vector3(20,  0, -1),
                new Vector3(20,  0, -5),
            };
            CheckTree(grid, inputId2, tree, expectedPath);

            expectedPath = new List<Vector3>()
            {
                new Vector3(5, 0, 5),
                new Vector3(5, 0, 0),
                new Vector3(8, 0, 0),
                new Vector3(8, 0, -1),
                new Vector3(9, 0, -1),
                new Vector3(10, 0, -1),
                new Vector3(15,  0, -1),
                new Vector3(20,  0, -1),
                new Vector3(20,  0, -5),
            };
            CheckTree(grid, inputId1, tree, expectedPath);

            expectedPath = new List<Vector3>()
            {
                new Vector3(0, 0, 5),
                new Vector3(0, 0, 0),
                new Vector3(5, 0, 0),
                new Vector3(8, 0, 0),
                new Vector3(8, 0, -1),
                new Vector3(9, 0, -1),
                new Vector3(10, 0, -1),
                new Vector3(15,  0, -1),
                new Vector3(20,  0, -1),
                new Vector3(20,  0, -5),
            };
            CheckTree(grid, inputId0, tree, expectedPath);
        }
        private void VisualizeRoutingTree(
            AdaptiveGrid grid,
            IEnumerable<RoutingVertex> routingVertices,
            IDictionary<ulong, TreeNode> tree)
        {
            List<Line> lines = new List<Line>();
            foreach (var input in routingVertices)
            {
                var node = tree[input.Id];
                while (node.Trunk != null)
                {
                    lines.Add(new Line(grid.GetVertex(node.Id).Point,
                                       grid.GetVertex(node.Trunk.Id).Point));
                    node = node.Trunk;
                }
            }
            ModelLines ml = new ModelLines(lines, new Material("", new Color("red")));
            this.Model.AddElement(ml);
        }

        private void VisualizeGrid(
            AdaptiveGraphRouting alg,
            IList<RoutingHintLine> hints, 
            IList<Vector3> keyPoints)
        {
            this.Model.AddElements(alg.RenderElements(hints, keyPoints));
        }

        private static void CheckTree(
            AdaptiveGrid grid, ulong startId, IDictionary<ulong, TreeNode> tree, List<Vector3> expectedPath)
        {
            TreeNode node = tree[startId];
            for (int i = 0; i < expectedPath.Count; i++)
            {
                Assert.Equal(expectedPath[i], grid.GetVertex(node.Id).Point);
                node = node.Trunk;
            }
            //After reaching start point before is pointing to 0 (nothing)
            Assert.Null(node);
        }
    }
}
