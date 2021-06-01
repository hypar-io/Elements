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
        public void MergeGlbFiles()
        {
            var testPath = "../../../models/MergeGlTF/Ours.glb";
            var ours = Interface.LoadModel(testPath);
            var buffers = ours.Buffers.ToList();
            var buffViews = ours.BufferViews.ToList();
            var accessors = ours.Accessors.ToList();
            var meshes = ours.Meshes.ToList();
            var materials = ours.Materials.ToList();
            var images = ours.Images != null ? ours.Images.ToList() : new List<Image>();
            var textures = ours.Textures != null ? ours.Textures.ToList() : new List<Texture>();
            var samplers = ours.Samplers != null ? ours.Samplers.ToList() : new List<Sampler>();

            using (var testStream = GltfExtensions.GetGlbStreamFromPath(testPath))
            {
                var bufferByteArrays = ours.GetAllBufferByteArrays(testStream);

                using (var avocadoStream = GltfExtensions.GetGlbStreamFromPath("../../../models/MergeGlTF/Avocado.glb"))
                {
                    GltfMergingUtils.AddAllMeshesFromFromGlb(avocadoStream,
                                                             buffers,
                                                             bufferByteArrays,
                                                             buffViews,
                                                             accessors,
                                                             meshes,
                                                             materials,
                                                             textures,
                                                             images,
                                                             samplers,
                                                             true
                                                             );
                }

                ours.Buffers = buffers.ToArray();
                ours.BufferViews = buffViews.ToArray();
                ours.Accessors = accessors.ToArray();
                ours.Meshes = meshes.ToArray();
                ours.Materials = materials.ToArray();
                ours.Images = images.ToArray();
                ours.Textures = textures.ToArray();
                if (samplers.Count > 0)
                {
                    ours.Samplers = samplers.ToArray();
                }

                var nodeList = ours.Nodes.ToList();
                var transform = new Transform(new Vector3(1, 1, 0), Vector3.XAxis, Vector3.YAxis.Negate()).Scaled(20);
                NodeUtilities.CreateNodeForMesh(ours.Meshes.Length - 1, nodeList, transform);
                ours.Nodes = nodeList.ToArray();

                var savepath = "models/GltfTestResult.gltf";
                ours.SaveBuffersAndAddUris(savepath, bufferByteArrays);
                ours.SaveModel(savepath);

                var mergedBuffer = ours.CombineBufferAndFixRefs(bufferByteArrays.ToArray());
                ours.SaveBinaryModel(mergedBuffer, "models/GltfTestMerged.glb");
            }
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
                Assert.True(vertexCount == 6, $"The stored mesh should contain 6 vertices, instead it contains {vertexCount}");
            }
        }
    }
}