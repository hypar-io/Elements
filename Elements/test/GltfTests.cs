using Xunit;
using Elements.Geometry;
using Elements.Serialization.glTF;
using glTFLoader;
using System.Linq;
using System.Collections.Generic;
using glTFLoader.Schema;
using System;

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

                var savepath = "../../../GltfTestResult.gltf";
                ours.SaveBuffersAndAddUris(savepath, bufferByteArrays);
                ours.SaveModel(savepath);

                var mergedBuffer = ours.CombineBufferAndFixRefs(bufferByteArrays.ToArray());
                ours.SaveBinaryModel(mergedBuffer, "../../../GltfTestMerged.glb");
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
    }
}