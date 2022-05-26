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
        public void GeneratingMeshFromMassHasCorrectVerticesAndTriangles()
        {
            var profile = Polygon.Rectangle(1.0, 1.0);
            var mass = new Mass(profile, 5.0, BuiltInMaterials.Mass, new Transform());
            var mesh = mass.ToMesh();

            Assert.Equal(24, mesh.Vertices.Count);
            Assert.Equal(12, mesh.Triangles.Count);
        }

        [Fact]
        public void ThrowsExceptionWhenCreatingMeshFromElementWithNoRepresentation()
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
        public void GeneratedMeshHasSameAreaAsElementGeometry()
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

        [Fact]
        public void Colorize()
        {
            this.Name = nameof(Colorize);
            var height = 5.0;
            var mass = new Mass(Polygon.L(2, 2, 1), height, BuiltInMaterials.Default);
            mass.ModifyVertexAttributes = (v) =>
            {
                return (v.position, v.normal, v.uv, new Color(v.position.Z / height, v.position.Z / height, 1, 1));
            };
            this.Model.AddElement(mass);
        }

        [Fact]
        public void CustomGraphicsBuffers()
        {
            this.Name = nameof(CustomGraphicsBuffers);
            var geo = new CustomGBClass
            {
                Material = BuiltInMaterials.Steel
            };
            Model.AddElement(geo);
        }

    }
    class CustomGBClass : GeometricElement
    {
        public override bool TryToGraphicsBuffers(out List<GraphicsBuffers> graphicsBuffers, out string id, out glTFLoader.Schema.MeshPrimitive.ModeEnum? mode)
        {
            id = $"{this.Id}_customthing";
            mode = glTFLoader.Schema.MeshPrimitive.ModeEnum.TRIANGLE_FAN;
            var vertices = new List<Vector3>() {
                    (0,0,0),
                    (1,0,0),
                    (1.5,0.5,0),
                    (1,1,0),
                    (0.5, 1.5,0),
                    (0,1,0),
            };
            graphicsBuffers = new List<GraphicsBuffers>() { vertices.ToGraphicsBuffers() };
            return true;
        }
    }
}