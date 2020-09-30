using Xunit;
using Elements.Geometry;
using Elements.Serialization.glTF;
using glTFLoader;
using System.Linq;
using System.Collections.Generic;
using glTFLoader.Schema;

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
            // var materials = ours1.Materials;
            var accessors = ours.Accessors.ToList();
            var meshes = ours.Meshes.ToList();

            var bufferByteArrays = ours.GetAllBufferByteArrays(testPath);

            GlftMergingUtils.AddAllMeshesFromFromGlb("../../../models/MergeGlTF/Duck.glb", buffers, bufferByteArrays, buffViews, accessors, meshes);

            ours.Buffers = buffers.ToArray();
            ours.BufferViews = buffViews.ToArray();
            ours.Accessors = accessors.ToArray();
            ours.Meshes = meshes.ToArray();

            var savepath = "../../../GltfTestResult.gltf";
            ours.SaveBuffersAndUris(savepath, bufferByteArrays);

            var nodeList = ours.Nodes.ToList();
            var duckTransform = new Transform(new Vector3(), Vector3.XAxis, Vector3.YAxis.Negate());
            duckTransform.Scale(1.0 / 100);
            GltfExtensions.CreateNodeForMesh(ours, ours.Meshes.Length - 1, nodeList, duckTransform);
            ours.Nodes = nodeList.ToArray();
            ours.SaveModel(savepath);

        }
    }
}