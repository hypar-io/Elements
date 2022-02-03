using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using Newtonsoft.Json;
using Xunit;

namespace Elements.Tests
{
    public class Bbox3Tests : ModelTest
    {
        [Fact]
        public void Bbox3Calculates()
        {
            var polygon = JsonConvert.DeserializeObject<Polygon>("{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-30.52885,\"Y\":-5.09393,\"Z\":0.0},{\"X\":-48.41906,\"Y\":111.94723,\"Z\":0.0},{\"X\":-95.02982,\"Y\":70.39512,\"Z\":0.0},{\"X\":-72.98057,\"Y\":-34.12159,\"Z\":0.0}]}");
            var bbox = new BBox3(polygon);
            var diagonal = bbox.Max.DistanceTo(bbox.Min);
            Assert.Equal(159.676157, diagonal, 4);
        }

        [Fact]
        public void DegenerateTest()
        {
            var bbox = new BBox3(new Vector3(), new Vector3(1, 1, 1));
            Assert.False(bbox.IsDegenerate());

            var degenerateBbox = new BBox3(new Vector3(), new Vector3(1, 1, 0));
            Assert.True(degenerateBbox.IsDegenerate());
        }

        [Fact]
        public void ContainsPointAtBounds()
        {
            var bounds = new BBox3(new Vector3(1, 1, 1), new Vector3(3, 3, 3));
            Assert.True(bounds.Contains(new Vector3(1, 1, 1)));
        }

        [Fact]
        public void ContainsPointInsideBounds()
        {
            var bounds = new BBox3(new Vector3(1, 1, 1), new Vector3(3, 3, 3));
            Assert.True(bounds.Contains(new Vector3(2, 2, 2)));
        }

        [Fact]
        public void DoesNotContainPointOutsideBounds()
        {
            var bounds = new BBox3(new Vector3(1, 1, 1), new Vector3(3, 3, 3));
            Assert.False(bounds.Contains(new Vector3(0, 0, 0)));
        }

        [Fact]
        public void ContainsPointInFlatBounds()
        {
            var bounds = new BBox3(new Vector3(1, 1, 1), new Vector3(3, 3, 1));
            Assert.True(bounds.Contains(new Vector3(2, 2, 1)));
        }

        [Fact]
        public void CorrectlyOrdersMinMax()
        {
            var bounds = new BBox3(new[] { new Vector3(3, 3, 3), new Vector3(1, 1, 1) });
            Assert.Equal(new Vector3(1, 1, 1), bounds.Min);
            Assert.Equal(new Vector3(3, 3, 3), bounds.Max);
        }

        [Fact]
        public void BBoxesForElements()
        {
            Name = nameof(BBoxesForElements);

            var elements = new List<Element>();

            // mass with weird transform
            elements.Add(new Mass(Polygon.Star(5, 3, 5), 1, null, new Transform(new Vector3(4, 3, 2), new Vector3(1, 1, 1))));

            // element instances
            var contentJson = @"
            {
            ""discriminator"": ""Elements.ContentElement"",
            ""gltfLocation"": ""https://hypar-content-catalogs.s3-us-west-2.amazonaws.com/2290ea5e-98aa-429d-8fab-1f260458bf57/Steelcase+-+Convene+-+Conference+Table+Top+-+Boat+Shape+-+Flat+Profile+-+72D+x+216W.glb"",
            ""Bounding Box"": {
                ""discriminator"": ""Elements.Geometry.BBox3"",
                ""Min"": {
                    ""X"": -2.7432066856384281,
                    ""Y"": -0.91380708560943613,
                    ""Z"": 0.68884799709320077
                },
                ""Max"": {
                    ""X"": 2.7432066856384281,
                    ""Y"": 0.91380701293945321,
                    ""Z"": 0.73660002422332771
                }
            },
            ""Gltf Scale to Meters"": 1.0,
            ""SourceDirection"": {
                ""X"": 0.0,
                ""Y"": 1.0,
                ""Z"": 0.0
            },
            ""Transform"": {
                ""Matrix"": {
                    ""Components"": [
                        1.0,
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        1.0,
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        1.0,
                        0.0
                    ]
                }
            },
            ""Material"": {
                ""discriminator"": ""Elements.Material"",
                ""Color"": {
                    ""Red"": 1.0,
                    ""Green"": 1.0,
                    ""Blue"": 1.0,
                    ""Alpha"": 1.0
                },
                ""SpecularFactor"": 0.0,
                ""GlossinessFactor"": 0.0,
                ""Unlit"": false,
                ""DoubleSided"": false,
                ""Id"": ""9babb829-9b96-4e73-97f4-9658d4d6c31c"",
                ""Name"": ""default""
            },
            ""Representation"": null,
            ""IsElementDefinition"": true,
            ""Id"": ""8032b381-13c9-4870-803d-c4127c201b47"",
            ""Name"": ""Steelcase - Convene - Conference Table Top - Boat Shape - Flat Profile - 72D x 216W"",
            ""Elevation from Level"": 6.4392935428259079E-14,
            ""Host"": ""Level : Level 1"",
            ""Offset from Host"": 6.4392935428259079E-14,
            ""Moves With Nearby Elements"": 0,
            ""Opaque"": 1
        }";

            var contentElement = JsonConvert.DeserializeObject<ContentElement>(contentJson);
            elements.Add(contentElement.CreateInstance(new Transform(-6, 0, 0), null));
            elements.Add(contentElement.CreateInstance(new Transform(new Vector3(-8, 0, 0), 45), null));

            // mesh
            var meshElem = ConstructExampleMesh();
            elements.Add(meshElem);

            // model curves
            var polygon = new Circle(4).ToPolygon(10).TransformedPolygon(new Transform(new Vector3(8, 8, 8), new Vector3(1, 0, 2)));
            elements.Add(new ModelCurve(polygon));

            // model points
            var random = new System.Random();
            var points = Enumerable.Range(0, 20).Select((i) => new Vector3(random.Next(15, 20), random.Next(15, 20), random.Next(15, 20))).ToList();
            var modelPoints = new ModelPoints(points, BuiltInMaterials.XAxis);
            elements.Add(modelPoints);

            // profile
            var profile = new Profile(new Circle(7).ToPolygon(10).TransformedPolygon(new Transform(new Vector3(8, 8, 8), new Vector3(1, 0, 2))));
            var xzProfile = new Profile(new Circle(7).ToPolygon(10).TransformedPolygon(new Transform(new Vector3(-8, 8, 8), new Vector3(0, 1, 0))));
            elements.Add(profile);
            elements.Add(xzProfile);
            Model.AddElements(profile.ToModelCurves());
            Model.AddElements(xzProfile.ToModelCurves());

            // non-geometric element
            var invalidBBox = new BBox3(new Material("Red", Colors.Red));
            Assert.False(invalidBBox.IsValid());

            Model.AddElements(elements);
            foreach (var element in elements)
            {
                var bbox = new BBox3(element);
                Model.AddElements(bbox.ToModelCurves());
            }
        }

        private MeshElement ConstructExampleMesh()
        {
            var mesh = new Mesh();
            var gridSize = 10;
            for (var u = 0; u < gridSize; u += 1)
            {
                for (var v = 0; v < gridSize; v += 1)
                {
                    var sinu = Math.Sin(-Math.PI + 2 * ((double)u / (double)gridSize * Math.PI));
                    var sinv = Math.Sin(-Math.PI + 2 * ((double)v / (double)gridSize * Math.PI));
                    var z = sinu + sinv;
                    var vertex = new Geometry.Vertex(new Vector3(u, v, z), color: Colors.Mint);
                    mesh.AddVertex(vertex);

                    if (u > 0 && v > 0)
                    {
                        var index = u * gridSize + v;
                        var a = mesh.Vertices[index];
                        var b = mesh.Vertices[index - gridSize];
                        var c = mesh.Vertices[index - 1];
                        var d = mesh.Vertices[index - gridSize - 1];
                        var tri1 = new Triangle(a, b, c);
                        var tri2 = new Triangle(c, b, d);

                        mesh.AddTriangle(tri1);
                        mesh.AddTriangle(tri2);
                    }
                }
            }
            mesh.ComputeNormals();
            var meshElement = new MeshElement(mesh, new Material("Lime", Colors.Lime), new Transform(new Vector3(-7, -8, 0), new Vector3(-5, 3, 2)));
            return meshElement;
        }

        [Fact]
        public void PointAtAndUVWCoordinates()
        {
            // point at coordinates
            var box = new BBox3((5, 2, 8), (10, 4, 10));
            Assert.Equal(new Vector3(7.5, 3, 9), box.PointAt(0.5, 0.5, 0.5));

            // point at vector3
            var box2 = new BBox3((0, 0, 0), (100, 1000, 10));
            Assert.Equal(new Vector3(20, 200, 2), box2.PointAt(new Vector3(0.2, 0.2, 0.2)));

            // transformAt
            var box3 = new BBox3((-10, -10, -10), (10, 10, 10));
            Assert.Equal(new Transform(), box3.TransformAt(0.5, 0.5, 0.5));

            // point at and uvw at point should be perfect inverses of each other
            var box4 = new BBox3((12, 6, 3), (45, 8, 22));
            var uvw = new Vector3(0.3, 0.7, 0.2);
            var pointInBox = box4.PointAt(uvw);
            var pointInUVW = box4.UVWAtPoint(pointInBox);
            Assert.Equal(uvw, pointInUVW);

            // point at and uvw at point should both work with coordinates outside the box
            var uvw2 = new Vector3(3, 4.2, -6.2);
            var pointInBox2 = box4.PointAt(uvw2);
            Assert.False(box4.Contains(pointInBox2));
            var pointInUVW2 = box4.UVWAtPoint(pointInBox2);
            Assert.Equal(uvw2, pointInUVW2);
        }

        [Fact]
        public void BoundingBoxIntersections()
        {
            // Contained
            var b1 = new BBox3(Vector3.Origin, new Vector3(5, 5, 5));
            var b2 = new BBox3(new Vector3(1, 1, 1), new Vector3(6, 6, 6));
            Assert.True(b1.Intersects(b2));

            // Coincident at corner
            b2 = new BBox3(new Vector3(-1, -1, -1), Vector3.Origin);
            Assert.True(b1.Intersects(b2));

            // Not contained or touching
            b2 = new BBox3(new Vector3(6, 6, 6), new Vector3(10, 10, 10));
            Assert.False(b1.Intersects(b2));

            // Full overlap
            b2 = b1;
            Assert.True(b1.Intersects(b2));

            // Coincident at face
            b2 = new BBox3(new Vector3(0, -5, 0), new Vector3(5, 0, 5));
            Assert.True(b1.Intersects(b2));
        }
    }
}