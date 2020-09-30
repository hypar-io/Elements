using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using glTFLoader;
using glTFLoader.Schema;

[assembly: InternalsVisibleTo("Elements.Tests")]
namespace Elements.Serialization.glTF
{

    internal static class GlftMergingUtils
    {
        public static void AddAllMeshesFromFromGlb(string glbPath,
                                        // Dictionary<string, int> materials,
                                        // List<byte> buffer,
                                        List<Buffer> buffers,
                                        List<byte[]> bufferByteArrays,
                                        List<BufferView> bufferViews,
                                        List<Accessor> accessors,
                                        List<glTFLoader.Schema.Mesh> meshes
                                        )
        {
            var newMaterials = new Dictionary<int, int>();
            var loaded = Interface.LoadModel(glbPath);
            var newByteArrays = loaded.GetAllBufferByteArrays(glbPath);
            bufferByteArrays.AddRange(newByteArrays);

            var bufferIncrement = buffers.Count;
            foreach (var originBuffer in loaded.Buffers)
            {
                buffers.Add(originBuffer);
            }

            var buffViewIncrement = bufferViews.Count;
            foreach (var originBuffView in loaded.BufferViews)
            {
                originBuffView.Buffer = originBuffView.Buffer + bufferIncrement;
                bufferViews.Add(originBuffView);
            }

            var accessorIncrement = accessors.Count;
            foreach (var originAccessor in loaded.Accessors)
            {
                originAccessor.BufferView = originAccessor.BufferView + buffViewIncrement;
                accessors.Add(originAccessor);
            }

            foreach (var originMesh in loaded.Meshes)
            {
                foreach (var prim in originMesh.Primitives)
                {
                    var attributes = new Dictionary<string, int>();
                    foreach (var kvp in prim.Attributes)
                    {
                        attributes[kvp.Key] = kvp.Value + accessorIncrement;
                    }
                    prim.Attributes = attributes;
                    prim.Indices = prim.Indices + accessorIncrement;
                }
                meshes.Add(originMesh);
            }
        }
    }
}