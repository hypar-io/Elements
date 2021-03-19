using System;
using Xunit;
using Elements.Components;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Tests;
using System.Linq;

namespace Elements.Components.Tests
{
    public class ComponentTests : ComponentTest
    {
        private static List<Vector3> TestReferencePointsA = new List<Vector3> {
              new Vector3(0,0,0),
              new Vector3(20,0,0),
              new Vector3(20,20,0),
              new Vector3(0, 20,0),
              new Vector3(10,10, 0)
            };
        private static List<List<Vector3>> TestTargetBoundariesA = new List<List<Vector3>> {
            // Same as reference
            new List<Vector3> {
              new Vector3(0,0,0),
              new Vector3(20,0,0),
              new Vector3(20,20,0),
              new Vector3(0, 20,0),
              new Vector3(10,10, 0)
            },
            // Scaled up
             new List<Vector3> {
              new Vector3(0,0,0),
              new Vector3(30,0,0),
              new Vector3(30,30,0),
              new Vector3(0, 30,0),
              new Vector3(15,15, 0)
            },
            // Smaller
            new List<Vector3> {
              new Vector3(0,0,0),
              new Vector3(15,0,0),
              new Vector3(15,15,0),
              new Vector3(0, 15,0),
              new Vector3(7.5,7.5, 0)
            },
            // Even Smaller
            new List<Vector3> {
              new Vector3(0,0,0),
              new Vector3(10,0,0),
              new Vector3(10,10,0),
              new Vector3(0,10,0),
              new Vector3(5,5,0)
            },
            // Rotated
            new List<Vector3> {
              new Vector3(0,0,0),
              new Vector3(20,0,0),
              new Vector3(20,20,0),
              new Vector3(0, 20,0),
              new Vector3(10,10, 0)
            }.Select(v => new Transform(Vector3.Origin, 45).OfPoint(v)).ToList(),
            // Distorted
            new List<Vector3> {
              new Vector3(0,0,0),
              new Vector3(20,2,0),
              new Vector3(22,19,0),
              new Vector3(0, 20,0),
              new Vector3(10,9, 0)
            }
        };

        [Fact]
        public void InstantiatePositionPlacementRule()
        {
            Name = nameof(InstantiatePositionPlacementRule);

            // establish reference points for rule definition.
            var referencePoints = TestReferencePointsA;

            // create objects, positioned within the reference boundary

            // red cube in the upper-right corner
            var red_cube = new Mass(
                            new Profile(Polygon.Rectangle(1, 1)),
                            7,
                            new Material("Red", new Color(1, 0, 0, 1)),
                            new Transform(19, 19, 0)
                            );

            // blue cube in the lower-right corner
            var blue_cube = new Mass(
                new Profile(Polygon.Rectangle(1, 1)),
                3,
                new Material("Blue", new Color(0, 0, 1, 1)),
                new Transform(18, 2, 0)
                );

            // create a set of position placement rules, using the closest point from each object to the reference points
            // to determine each object's anchor.
            var rules = PositionPlacementRule.FromClosestPoints(new[] { red_cube, blue_cube }, referencePoints);

            // create a polyline rule, to visualize the boundary: 
            var boundaryRule = PolylinePlacementRule.FromClosestPoints(Polygon.Rectangle(new Vector3(0, 0, 0), new Vector3(20, 20)), referencePoints, "Boundary");
            rules.Add(boundaryRule);

            // define component
            var definition = new ComponentDefinition(rules, referencePoints.ToList());

            ArrayResults(definition, TestTargetBoundariesA);
        }

        [Fact]
        public void InstantiateArrayPlacementRule()
        {
            Name = nameof(InstantiateArrayPlacementRule);
            var red_cube = new Mass(
                          new Profile(Polygon.Rectangle(1, 1)),
                          1,
                          new Material("Red", new Color(1, 0, 0, 1)),
                          new Transform()
                          );
            var green_cube = new Mass(
                          new Profile(Polygon.Rectangle(1, 1)),
                          7,
                          new Material("Green", new Color(0, 1, 0, 1)),
                          new Transform()
                          );
            var blue_cube = new Mass(
                          new Profile(Polygon.Rectangle(1, 1)),
                          7,
                          new Material("Blue", new Color(0, 0, 1, 1)),
                          new Transform()
                          );
            var refPath1 = new Polyline(new[] { new Vector3(1, 1), new Vector3(1, 19) });
            var refPath2 = new Polyline(new[] { new Vector3(2, 18), new Vector3(18, 2) });
            var refPath3 = new Polyline(new[] { new Vector3(19, 1), new Vector3(19, 19) });
            var rule1 = ArrayPlacementRule.FromClosestPoints(red_cube, refPath1, new SpacingConfiguration(SpacingMode.ByCount, 5), TestReferencePointsA, "By Count Array");
            var rule2 = ArrayPlacementRule.FromClosestPoints(green_cube, refPath2, new SpacingConfiguration(SpacingMode.ByApproximateLength, 4), TestReferencePointsA, "By Approx Length Array");
            var rule3 = ArrayPlacementRule.FromClosestPoints(blue_cube, refPath3, new SpacingConfiguration(SpacingMode.ByLength, 2), TestReferencePointsA, "By Length Array");
            // create a polyline rule, to visualize the boundary: 
            var boundaryRule = PolylinePlacementRule.FromClosestPoints(Polygon.Rectangle(new Vector3(0, 0, 0), new Vector3(20, 20)), TestReferencePointsA, "Boundary");

            var definition = new ComponentDefinition(new IComponentPlacementRule[] { rule1, rule2, rule3, boundaryRule }, TestReferencePointsA);

            ArrayResults(definition, TestTargetBoundariesA);
        }

        [Fact]
        public void InstantiateSizeBasedPlacementRule()
        {
            Name = nameof(InstantiateSizeBasedPlacementRule);
            var sizes = new[] { 18, 15, 13, 10, 4 };
            var elementConfigs = new List<(GeometricElement, Polygon)>();
            var innerBoundary = Polygon.Rectangle(new Vector3(1, 1), new Vector3(19, 19));
            foreach (var size in sizes)
            {
                var color = new Color(size / 18.0, 0, 1.0, 1.0);
                var mat = new Material(size.ToString(), color);
                var clearance = Polygon.Rectangle(new Vector3(0, 0), new Vector3(size, size));
                var mass = new Mass(new Profile(clearance, clearance.Offset(-1), Guid.NewGuid(), null), 1, mat);
                elementConfigs.Add((mass, clearance));
            }
            var rule = SizeBasedPlacementRule.FromClosestPoints(elementConfigs, innerBoundary, TestReferencePointsA, "Size rule");
            // create a polyline rule, to visualize the boundary: 
            var boundaryRule1 = PolylinePlacementRule.FromClosestPoints(Polygon.Rectangle(new Vector3(0, 0, 0), new Vector3(20, 20)), TestReferencePointsA, "Boundary");
            
            var boundaryRule2 = PolylinePlacementRule.FromClosestPoints(innerBoundary, TestReferencePointsA, "Boundary 2");

            var definition = new ComponentDefinition(new IComponentPlacementRule[] { rule, boundaryRule1, boundaryRule2 }, TestReferencePointsA);

            ArrayResults(definition, TestTargetBoundariesA);
        }
    }
}
