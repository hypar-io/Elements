using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using glTFLoader;
using glTFLoader.Schema;

[assembly: InternalsVisibleTo("Elements.Tests")]
namespace Elements.Serialization.glTF
{

    internal class ProtoNode
    {
        public int? Mesh;
        public float[] Matrix;
        public List<ProtoNode> Children = new List<ProtoNode>();
    }
    internal static class GltfMergingUtils
    {
        public static List<int> AddAllMeshesFromFromGlb(Stream glbStream,
                                        List<Buffer> buffers,
                                        List<byte[]> bufferByteArrays,
                                        List<BufferView> bufferViews,
                                        List<Accessor> accessors,
                                        List<glTFLoader.Schema.Mesh> meshes,
                                        List<glTFLoader.Schema.Material> materials,
                                        List<Texture> textures,
                                        List<Image> images,
                                        List<Sampler> samplers,
                                        HashSet<string> extensions,
                                        bool shouldAddMaterials,
                                        System.Guid contentElementId,
                                        out ProtoNode parentNode
                                        )
        {
            var loadingStream = new MemoryStream();
            glbStream.Position = 0;
            glbStream.CopyTo(loadingStream);
            loadingStream.Position = 0;
            var loaded = Interface.LoadModel(loadingStream);
            var newByteArrays = loaded.GetAllBufferByteArrays(glbStream);
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

            foreach (var extension in loaded.ExtensionsUsed)
            {
                extensions.Add(extension);
            }

            var imageIncrement = images.Count;
            if (loaded.Images != null)
            {
                foreach (var originImage in loaded.Images)
                {
                    if (originImage.BufferView.HasValue)
                    {
                        originImage.BufferView = originImage.BufferView + buffViewIncrement;
                    }
                    images.Add(originImage);
                }
            }

            var samplerIncrement = samplers.Count;
            if (loaded.Samplers != null)
            {
                foreach (var originSampler in loaded.Samplers)
                {
                    samplers.Add(originSampler);
                }
            }

            var textureIncrement = textures.Count;
            if (loaded.Textures != null)
            {
                foreach (var originTexture in loaded.Textures)
                {
                    originTexture.Source = originTexture.Source + imageIncrement;
                    if (originTexture.Sampler.HasValue)
                    {
                        originTexture.Sampler = originTexture.Sampler + samplerIncrement;

                    }
                    textures.Add(originTexture);
                }
            }


            var materialIncrement = materials.Count;

            if (shouldAddMaterials)
            {
                AddMaterials(materials, loaded, textureIncrement);
            }

            var meshIndices = new List<int>();
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
                    if (shouldAddMaterials)
                    {
                        prim.Material = prim.Material + materialIncrement;
                    }
                    else
                    {
                        prim.Material = 0;  // This assumes that the default material is at index 0
                    }
                }
                originMesh.Name = $"{contentElementId}_mesh";
                meshes.Add(originMesh);
                meshIndices.Add(meshes.Count - 1);
            }

            var topNode = loaded.Nodes[loaded.Scenes[0].Nodes[0]];
            parentNode = RecursivelyModifyMeshIndices(topNode, meshIndices, loaded.Nodes);
            parentNode.Matrix = topNode.Matrix;

            return meshIndices;
        }

        /// <summary>
        /// We construct a new, recursively-structured 'ProtoNode' from the flat gltf node, and mutate its mesh indices to
        /// point to the correct mesh in the merged gltf.
        /// </summary>
        private static ProtoNode RecursivelyModifyMeshIndices(glTFLoader.Schema.Node node, List<int> meshIndices, glTFLoader.Schema.Node[] loadedNodes)
        {
            var protoNode = new ProtoNode();
            protoNode.Matrix = node.Matrix;
            if (node.Mesh != null)
            {
                protoNode.Mesh = meshIndices[node.Mesh.Value];
            }
            if (node.Children != null && node.Children.Count() > 0)
            {
                foreach (var child in node.Children)
                {
                    protoNode.Children.Add(RecursivelyModifyMeshIndices(loadedNodes[child], meshIndices, loadedNodes));
                }
            }
            return protoNode;
        }

        private static void AddMaterials(List<glTFLoader.Schema.Material> materials, Gltf loaded, int textureIncrement)
        {
            foreach (var originMaterial in loaded.Materials)
            {
                if (originMaterial.EmissiveTexture != null)
                {
                    originMaterial.EmissiveTexture.Index = originMaterial.EmissiveTexture.Index + textureIncrement;
                }
                if (originMaterial.NormalTexture != null)
                {
                    originMaterial.NormalTexture.Index = originMaterial.NormalTexture.Index + textureIncrement;
                }
                if (originMaterial.OcclusionTexture != null)
                {
                    originMaterial.OcclusionTexture.Index = originMaterial.OcclusionTexture.Index + textureIncrement;
                }
                if (originMaterial.PbrMetallicRoughness != null)
                {
                    if (originMaterial.PbrMetallicRoughness.MetallicRoughnessTexture != null)
                    {
                        originMaterial.PbrMetallicRoughness.MetallicRoughnessTexture.Index = originMaterial.PbrMetallicRoughness.MetallicRoughnessTexture.Index + textureIncrement;
                    }
                    if (originMaterial.PbrMetallicRoughness.BaseColorTexture != null)
                    {
                        originMaterial.PbrMetallicRoughness.BaseColorTexture.Index = originMaterial.PbrMetallicRoughness.BaseColorTexture.Index + textureIncrement;
                    }
                }

                materials.Add(originMaterial);
            }
        }
    }
}