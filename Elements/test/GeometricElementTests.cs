using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class GeometricElementTests : ModelTest
    {
        [Fact]
        public void CheckMeshVertexAndTriangleCount()
        {
            var profile = Polygon.Rectangle(1.0, 1.0);
            var mass = new Mass(profile, 5.0, BuiltInMaterials.Mass, new Transform());
            var mesh = mass.ToMesh();

            Assert.Equal(24, mesh.Vertices.Count);
            Assert.Equal(12, mesh.Triangles.Count);
        }

        [Fact]
        public void NoRepresentationTest()
        {
            var empty = new GeometricElement(new Transform(),
                                             BuiltInMaterials.Default,
                                             null,
                                             false,
                                             System.Guid.NewGuid(),
                                             "");
            Assert.Throws<ArgumentNullException>(() => empty.ToMesh());
        }

        [Fact]
        public void CheckLaminaMeshAndTransforming()
        {
            var center = new Vector3(2, 2, 2);
            var profile = Polygon.Rectangle(1.0, 1.0);
            var panel = new Panel(profile);
            panel.Transform.Move(center);

            var mesh = panel.ToMesh();
            var meshTransformed = panel.ToMesh(true);
            var centroid = meshTransformed.Triangles.Select(t => t.ToPolygon().Centroid())
                                                    .ToList()
                                                    .Average(); ;
            Assert.Equal(center, centroid);

            Assert.Equal(panel.Area() * 2, mesh.Triangles.Sum(t => t.Area()), 5);
            Assert.Equal(panel.Area() * 2, meshTransformed.Triangles.Sum(t => t.Area()), 5);
        }
    }
}