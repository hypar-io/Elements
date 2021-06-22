using Xunit;
using Elements.Geometry;
using Elements.Serialization.glTF;
using glTFLoader;
using System.Linq;
using System.Collections.Generic;
using glTFLoader.Schema;
using System;
using System.IO;
using Newtonsoft.Json;

namespace Elements.Tests
{
    public class GltfTests
    {
        [Fact]
        public void EmptyModelSerializesWithoutThrowingException()
        {
            var model = new Model();
            model.ToGlTF("./empty_model.glb", true);
        }

        [Fact]
        public void ModelSerializesToBase64String()
        {
            var model = new Model();
            var beam = new Beam(new Line(Vector3.Origin, new Vector3(5, 5, 5)), Polygon.Rectangle(0.1, 0.2));
            model.AddElement(beam);
            var str = model.ToBase64String();
        }

        [Fact]
        public void InstanceContentElements()
        {
            var singleElement = new ContentElement("https://hypar-content-catalogs.s3-us-west-2.amazonaws.com/a1cf1df6-0762-45e7-942b-7ba17d813ff4/HermanMiller_Collection_Eames_MoldedPlywood_DiningChair_MtlBase+-+Upholstered.glb",
                                                   new BBox3(new Vector3(), new Vector3(1, 1, 1)),
                                                   1,
                                                   Vector3.XAxis,
                                                   new Transform(),
                                                   null,
                                                   null,
                                                   true,
                                                   Guid.NewGuid(),
                                                   "",
                                                   "");
            var baseModel = new Model();
            for (int i = 0; i < 10; i++)
            {
                var transform = new Transform();
                transform.Rotate(i * 10);
                transform.Concatenate(new Transform(i * 2, 0, 0));
                baseModel.AddElement(singleElement.CreateInstance(transform, "Individual Element"));
            }
            var glbWithInstancesPath = Path.Combine("models", "multiple-instances.glb");
            baseModel.ToGlTF(glbWithInstancesPath);
            var cElement = new ContentElement(glbWithInstancesPath, new BBox3(new Vector3(), new Vector3(1, 1, 1)), 1, Vector3.XAxis, new Transform(), null, null, true, Guid.NewGuid(), "", "");

            var model = new Model();
            foreach (var i in Enumerable.Range(1, 5))
            {
                var inst = cElement.CreateInstance(new Transform(new Vector3(0, i * 3, 0)), "");
                model.AddElement(inst);
            }
            model.ToGlTF("models/GltfInstancedContent.glb");
        }

        private class NoMaterial : GeometricElement
        {
            public NoMaterial() : base(new Transform(), null, null, false, Guid.NewGuid(), "NoMaterialElement") { }
        }

        [Fact]
        public void GeometricElementWithoutMaterialUsesDefaultMaterial()
        {
            var testElement = new NoMaterial();
            var model = new Model();
            model.AddElement(testElement);
            Assert.True(testElement.Material == BuiltInMaterials.Default);
        }

        [Fact]
        public void ThinObjectsGenerateCorrectly()
        {
            var json = File.ReadAllText("../../../models/Geometry/Single-Panel.json");
            var panel = JsonConvert.DeserializeObject<Panel>(json);
            var model = new Model();
            model.AddElement(panel);
            var modelsDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "models");
            var gltfPath = Path.Combine(modelsDir, "Single-Panel.gltf");
            model.ToGlTF(gltfPath, false);
            var gltfJson = File.ReadAllText(gltfPath);
            using (var glbStream = GltfExtensions.GetGlbStreamFromPath(gltfPath))
            {
                var loadingStream = new MemoryStream();
                glbStream.Position = 0;
                glbStream.CopyTo(loadingStream);
                loadingStream.Position = 0;
                var loaded = Interface.LoadModel(loadingStream);
                var vertexCount = loaded.Accessors.First(a => a.Type == Accessor.TypeEnum.VEC3).Count;
                Assert.True(vertexCount == 12, $"The stored mesh should contain 12 vertices, instead it contains {vertexCount}");
            }
        }
    }
}