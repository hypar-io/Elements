using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;
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

        [Fact]
        public void Intersect()
        {
            var geometricElement = new GeometricElement(new Transform(),
                                             BuiltInMaterials.Default,
                                             null,
                                             false,
                                             System.Guid.NewGuid(),
                                             "");
            var width = 10;
            var height = 10;
            var hhickness = 0.2;
            var frameOuterPolygon = new Polygon(new Vector3(-width / 2.0, 0, -height / 2.0), new Vector3(-width / 2.0, 0, height / 2.0),
                new Vector3(width / 2.0, 0, height / 2.0), new Vector3(width / 2.0, 0, -height / 2.0));
            var frameRepresentation = new SolidRepresentation();
            var profile = new Profile(frameOuterPolygon);
            frameRepresentation.SolidOperations.Add(new Extrude(profile, hhickness, Vector3.YAxis));
            geometricElement.RepresentationInstances.Add(new RepresentationInstance(frameRepresentation, BuiltInMaterials.XAxis));
            var plane = new Plane(Vector3.Origin, Vector3.ZAxis);
            var intersection = geometricElement.Intersects(plane, out var intersectionPolygons, out var beyondPolygons, out var lines);
            Assert.True(intersection);
            Assert.Single(intersectionPolygons[geometricElement.Id]);
            Assert.True(intersectionPolygons[geometricElement.Id][0].Area().ApproximatelyEquals(2));
            Assert.Empty(beyondPolygons[geometricElement.Id]);
            Assert.True(!lines.Any() || !lines[geometricElement.Id].Any());
        }


        [Fact]
        public void DoesNotIntersect()
        {
            var geometricElement = new GeometricElement(new Transform(),
                                             BuiltInMaterials.Default,
                                             null,
                                             false,
                                             System.Guid.NewGuid(),
                                             "");
            var width = 10;
            var height = 10;
            var thickness = 0.2;
            var frameOuterPolygon = new Polygon(new Vector3(-width / 2.0, 0, -height / 2.0), new Vector3(-width / 2.0, 0, height / 2.0),
                new Vector3(width / 2.0, 0, height / 2.0), new Vector3(width / 2.0, 0, -height / 2.0));
            var frameRepresentation = new SolidRepresentation();
            var profile = new Profile(frameOuterPolygon);
            frameRepresentation.SolidOperations.Add(new Extrude(profile, thickness, Vector3.YAxis));
            geometricElement.RepresentationInstances.Add(new RepresentationInstance(frameRepresentation, BuiltInMaterials.XAxis));
            geometricElement.Transform = new Transform(new Vector3(width / 2.0, 0, height / 2.0));
            var plane = new Plane(Vector3.Origin, Vector3.ZAxis);
            var intersection = geometricElement.Intersects(plane, out var intersectionPolygons, out var beyondPolygons, out var lines);
            Assert.False(intersection);
            Assert.True(!intersectionPolygons.Any() || !intersectionPolygons[geometricElement.Id].Any());
            Assert.True(!beyondPolygons.Any() || !beyondPolygons[geometricElement.Id].Any());
            Assert.True(!lines.Any() || !lines[geometricElement.Id].Any());
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