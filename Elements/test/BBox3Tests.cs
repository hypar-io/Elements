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
            var polygon = Polygon.Circle(4, 10).TransformedPolygon(new Transform(new Vector3(8, 8, 8), new Vector3(1, 0, 2)));
            elements.Add(new ModelCurve(polygon));

            // model points
            var random = new System.Random();
            var points = Enumerable.Range(0, 20).Select((i) => new Vector3(random.Next(15, 20), random.Next(15, 20), random.Next(15, 20))).ToList();
            var modelPoints = new ModelPoints(points, BuiltInMaterials.XAxis);
            elements.Add(modelPoints);

            // profile
            var profile = new Profile(Polygon.Circle(7, 10).TransformedPolygon(new Transform(new Vector3(8, 8, 8), new Vector3(1, 0, 2))));
            elements.Add(profile);
            Model.AddElements(profile.ToModelCurves());

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
    }
}