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

        private class TestContentElem : ContentElement
        {
            public TestContentElem(string @gltfLocation, BBox3 @bBox, Transform @transform, double scale, Material @material, Representation @representation, bool @isElementDefinition, System.Guid @id, string @name)
                        : base(gltfLocation, bBox, scale, transform, material, representation, isElementDefinition, id, name)
            { }
        }

        [Fact]
        public void InstanceContentElement()
        {
            var model = new Model();
            var boxType = new TestContentElem("../../../models/MergeGlTF/Avocado.glb",
                                      new BBox3(new Vector3(-0.5, -0.5, 0), new Vector3(0.5, 0.5, 3)),
                                      new Transform(new Vector3(), Vector3.YAxis),
                                      20,
                                      BuiltInMaterials.Default,
                                      null,
                                      true,
                                      Guid.NewGuid(),
                                      "BoxyType");
            var boxType2 = new TestContentElem("../../../models/MergeGlTF/Duck.glb",
                                      new BBox3(new Vector3(-1, -1, 0), new Vector3(1, 1, 2)),
                                      new Transform(new Vector3(), Vector3.YAxis),
                                      .01,
                                      BuiltInMaterials.Default,
                                      null,
                                      true,
                                      Guid.NewGuid(),
                                      "BoxyType");
            var newBox = boxType.CreateInstance(new Transform(), "first one");
            var twoBox = boxType2.CreateInstance(new Transform(new Vector3(5, 0, 0)), "then two");
            var threeBox = boxType2.CreateInstance(new Transform(new Vector3(15, 0, 0)), "then two");
            var beam = new Beam(new Line(new Vector3(), new Vector3(0, 5, -0.5)), new Circle(new Vector3(), 0.3).ToPolygon());
            model.AddElement(newBox);
            model.AddElement(twoBox);
            model.AddElement(threeBox);
            model.AddElement(beam);
            model.ToGlTF("../../../GltfInstancing.gltf", false);
            model.ToGlTF("../../../GltfInstancing.glb");
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

            var bufferByteArrays = ours.GetAllBufferByteArrays(testPath);

            GltfMergingUtils.AddAllMeshesFromFromGlb("../../../models/MergeGlTF/Avocado.glb",
                                                     buffers,
                                                     bufferByteArrays,
                                                     buffViews,
                                                     accessors,
                                                     meshes,
                                                     materials,
                                                     textures,
                                                     images,
                                                     samplers
                                                     );

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
            var transform = new Transform(new Vector3(.1, .1, 0), Vector3.XAxis, Vector3.YAxis.Negate()).Scaled(0.01);
            GltfExtensions.CreateNodeForMesh(ours, ours.Meshes.Length - 1, nodeList, transform);
            ours.Nodes = nodeList.ToArray();

            var savepath = "../../../GltfTestResult.gltf";
            ours.SaveBuffersAndAddUris(savepath, bufferByteArrays);
            ours.SaveModel(savepath);

            var mergedBuffer = ours.CombineBufferAndFixRefs(bufferByteArrays.ToArray());
            ours.SaveBinaryModel(mergedBuffer, "../../../GltfTestMerged.glb");
        }
    }
}