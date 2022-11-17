using Elements.Geometry;
using Elements.Geometry.Tessellation;
using System;
using System.Collections.Generic;
// TODO: Get rid of System.Linq
using System.Linq;
using glTFLoader;
using glTFLoader.Schema;
using System.IO;
using System.Runtime.CompilerServices;
using Elements.Geometry.Solids;
using Elements.Geometry.Interfaces;
using SixLabors.ImageSharp.Processing;
using Elements.Collections.Generics;
using System.Net;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp;
using Image = glTFLoader.Schema.Image;
using Vector3 = Elements.Geometry.Vector3;
using Quaternion = Elements.Geometry.Quaternion;
using System.Reflection;

[assembly: InternalsVisibleTo("Hypar.Elements.Tests")]
[assembly: InternalsVisibleTo("Elements.Benchmarks")]

namespace Elements.Serialization.glTF
{
    /// <summary>
    /// Extensions for glTF serialization.
    /// </summary>
    public static class GltfExtensions
    {
        private static int _currentId = -1;

        private static int GetNextId()
        {
            _currentId++;
            return _currentId;
        }

        /// <summary>
        /// In normal function use, this should be set to null.
        /// If not null and set to a valid directory path, gltfs loaded for 
        /// content elements will be cached to this directory, and can be
        /// explicitly loaded by calling LoadGltfCacheFromDisk(). This is used
        /// by `hypar run` and test capabilities to speed up repeated runs. 
        /// </summary>
        public static string GltfCachePath
        {
            get => gltfCachePath;
            set
            {
                if (Directory.Exists(value))
                {
                    gltfCachePath = value;
                }
                else
                {
                    throw new ArgumentException("GltfCachePath must be a valid directory path.");
                }
            }
        }
        private static string gltfCachePath = null;
        private const string GLTF_CACHE_FOLDER_NAME = "elementsGltfCache";

        private const string emptyGltf = @"{
    ""asset"": {""version"": ""2.0""},
    ""nodes"": [{""name"": ""empty""}],
    ""scenes"": [{""nodes"": [0]}],
    ""scene"": 0
}";

        /// <summary>
        /// Serialize the model to a gltf file on disk.
        /// If there is no geometry, an empty GLTF will still be produced.
        /// </summary>
        /// <param name="model">The model to serialize.</param>
        /// <param name="path">The output path.</param>
        /// <param name="errors">A collection of serialization errors</param>
        /// <param name="useBinarySerialization">Should binary serialization be used?</param>
        /// <param name="drawEdges">Should the solid edges be written to the gltf?</param>
        public static void ToGlTF(this Model model, string path, out List<BaseError> errors, bool useBinarySerialization = true, bool drawEdges = false)
        {
            errors = new List<BaseError>();
            if (model.Elements.Count > 0)
            {
                if (useBinarySerialization)
                {
                    if (SaveGlb(model, path, out errors, drawEdges))
                    {
                        return;
                    }
                    // Else fall through to produce an empty GLTF.
                }
                else
                {
                    if (SaveGltf(model, path, out errors, drawEdges))
                    {
                        return;
                    }
                    // Else fall through to produce an empty GLTF.
                }
            }

            // There are no elements that produced geometry. Write an empty GLTF.
            File.WriteAllText(path, emptyGltf);
        }

        /// <summary>
        /// Serialize the model to a gltf file on disk.
        /// If there is no geometry, an empty GLTF will still be produced.
        /// </summary>
        /// <param name="model">The model to serialize.</param>
        /// <param name="path">The output path.</param>
        /// <param name="useBinarySerialization">Should binary serialization be used?</param>
        /// <param name="drawEdges">Should the solid edges be written to the gltf?</param>
        public static void ToGlTF(this Model model, string path, bool useBinarySerialization = true, bool drawEdges = false)
        {
            ToGlTF(model, path, out _, useBinarySerialization, drawEdges);
        }

        /// <summary>
        /// Serialize the model to a byte array.
        /// </summary>
        /// <param name="model">The model to serialize.</param>
        /// <param name="drawEdges">Should edges of the model be drawn?</param>
        /// <param name="mergeVertices">Should vertices be merged in the resulting output?</param>
        /// <returns>A byte array representing the model.</returns>
        public static byte[] ToGlTF(this Model model, bool drawEdges = false, bool mergeVertices = false)
        {
            var gltf = InitializeGlTF(model, out var buffers, out _, drawEdges, mergeVertices);
            if (gltf == null)
            {
                return null;
            }
            var mergedBuffer = gltf.CombineBufferAndFixRefs(buffers);

            byte[] bytes;
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                gltf.SaveBinaryModel(mergedBuffer, ms);
                // Avoid a copy of this array by using GetBuffer() instead
                // of ToArray()
                bytes = ms.GetBuffer();
            }

            return bytes;
        }

        /// <summary>
        /// Serialize the model to a base64 encoded string.
        /// </summary>
        /// <returns>A Base64 string representing the model.</returns>
        public static string ToBase64String(this Model model, bool drawEdges = false, bool mergeVertices = false)
        {
            var gltf = InitializeGlTF(model, out var buffers, out _, drawEdges, mergeVertices);
            if (gltf == null)
            {
                return "";
            }
            var mergedBuffer = gltf.CombineBufferAndFixRefs(buffers);
            string b64;
            using (var ms = new MemoryStream())
            {
                gltf.SaveBinaryModel(mergedBuffer, ms);
                b64 = Convert.ToBase64String(ms.GetBuffer());
            }

            return b64;
        }

        internal static Dictionary<string, int> AddMaterials(this Gltf gltf,
                                                             IList<Material> materials,
                                                             List<byte> buffer,
                                                             List<BufferView> bufferViews)
        {
            var materialDict = new Dictionary<string, int>();
            var newMaterials = new List<glTFLoader.Schema.Material>();

            var textureDict = new Dictionary<string, int>(); // the name of the texture image, the id of the texture
            var textures = new List<Texture>();

            var images = new List<Image>();
            var samplers = new List<Sampler>();

            var matId = 0;
            var texId = 0;
            var imageId = 0;
            var samplerId = 0;

            foreach (var material in materials)
            {
                if (materialDict.ContainsKey(material.Id.ToString()))
                {
                    continue;
                }

                var gltfMaterial = new glTFLoader.Schema.Material();
                newMaterials.Add(gltfMaterial);

                gltfMaterial.PbrMetallicRoughness = new MaterialPbrMetallicRoughness
                {
                    BaseColorFactor = material.Color.ToArray(true),
                    MetallicFactor = 1.0f
                };
                gltfMaterial.DoubleSided = material.DoubleSided;

                gltfMaterial.Name = material.Id.ToString();

                if (material.Unlit)
                {
                    gltfMaterial.Extensions = new Dictionary<string, object>{
                        {"KHR_materials_unlit", new Dictionary<string, object>{}},
                    };
                }
                else
                {
                    // We convert to a linear color space
                    gltfMaterial.Extensions = new Dictionary<string, object>{
                        {"KHR_materials_pbrSpecularGlossiness", new Dictionary<string,object>{
                            {"diffuseFactor", new[]{
                                Geometry.Color.SRGBToLinear(material.Color.Red),
                                Geometry.Color.SRGBToLinear(material.Color.Green),
                                Geometry.Color.SRGBToLinear(material.Color.Blue),
                                material.Color.Alpha}},
                            {"specularFactor", new[]{material.SpecularFactor, material.SpecularFactor, material.SpecularFactor}},
                            {"glossinessFactor", material.GlossinessFactor}
                        }}
                    };
                }

                if (material.EdgeDisplaySettings != null)
                {
                    AddExtension(gltf, gltfMaterial, "HYPAR_materials_edge_settings", new Dictionary<string, object>{
                        {"lineWidth", material.EdgeDisplaySettings.LineWidth},
                        {"widthMode", (int)material.EdgeDisplaySettings.WidthMode},
                        {"dashMode", (int)material.EdgeDisplaySettings.DashMode},
                        {"dashSize", material.EdgeDisplaySettings.DashSize}
                    });
                }

                if (material.DrawInFront)
                {
                    AddExtension(gltf, gltfMaterial, "HYPAR_draw_in_front", new Dictionary<string, object>{
                        {"drawInFront", true},
                    });
                }

                var textureHasTransparency = false;

                if (material.Texture != null && File.Exists(material.Texture))
                {
                    // Add the texture
                    var textureInfo = new TextureInfo();
                    gltfMaterial.PbrMetallicRoughness.BaseColorTexture = textureInfo;
                    textureInfo.Index = texId;
                    textureInfo.TexCoord = 0;
                    if (!material.Unlit)
                    {
                        ((Dictionary<string, object>)gltfMaterial.Extensions["KHR_materials_pbrSpecularGlossiness"])["diffuseTexture"] = textureInfo;
                    }

                    if (textureDict.ContainsKey(material.Texture))
                    {
                        textureInfo.Index = textureDict[material.Texture];
                    }
                    else
                    {
                        var texture = new Texture();
                        textures.Add(texture);
                        var image = CreateImage(material.Texture, bufferViews, buffer, out textureHasTransparency);
                        texture.Source = imageId;
                        images.Add(image);

                        var sampler = CreateSampler(material.RepeatTexture);
                        if (!material.InterpolateTexture)
                        {
                            sampler.MagFilter = Sampler.MagFilterEnum.NEAREST;
                        }
                        texture.Sampler = samplerId;
                        samplers.Add(sampler);

                        textureDict.Add(material.Texture, texId);

                        texId++;
                        imageId++;
                        samplerId++;
                    }
                }

                if (material.NormalTexture != null && File.Exists(material.NormalTexture))
                {
                    var textureInfo = new MaterialNormalTextureInfo();
                    gltfMaterial.NormalTexture = textureInfo;
                    textureInfo.Index = texId;
                    textureInfo.Scale = 1.0f;
                    // Use the same texture coordinate as the
                    // base texture.
                    textureInfo.TexCoord = 0;

                    if (textureDict.ContainsKey(material.NormalTexture))
                    {
                        textureInfo.Index = textureDict[material.NormalTexture];
                    }
                    else
                    {
                        var texture = new Texture();
                        textures.Add(texture);
                        var image = CreateImage(material.NormalTexture, bufferViews, buffer, out _);
                        texture.Source = imageId;
                        images.Add(image);
                        textureDict.Add(material.NormalTexture, texId);

                        var sampler = CreateSampler(material.RepeatTexture);
                        if (!material.InterpolateTexture)
                        {
                            sampler.MagFilter = Sampler.MagFilterEnum.NEAREST;
                        }
                        texture.Sampler = samplerId;
                        samplers.Add(sampler);

                        texId++;
                        imageId++;
                        samplerId++;
                    }
                }

                if (material.EmissiveTexture != null && File.Exists(material.EmissiveTexture))
                {
                    var textureInfo = new TextureInfo();
                    gltfMaterial.EmissiveTexture = textureInfo;
                    textureInfo.Index = texId;
                    textureInfo.TexCoord = 0;

                    if (textureDict.ContainsKey(material.EmissiveTexture))
                    {
                        textureInfo.Index = textureDict[material.EmissiveTexture];
                    }
                    else
                    {
                        var texture = new Texture();
                        textures.Add(texture);
                        var image = CreateImage(material.EmissiveTexture, bufferViews, buffer, out _);
                        texture.Source = imageId;
                        images.Add(image);

                        var sampler = CreateSampler(material.RepeatTexture);
                        texture.Sampler = samplerId;
                        samplers.Add(sampler);

                        textureDict.Add(material.EmissiveTexture, texId);

                        texId++;
                        imageId++;
                        samplerId++;
                    }

                    gltfMaterial.EmissiveFactor = new float[] { (float)material.EmissiveFactor, (float)material.EmissiveFactor, (float)material.EmissiveFactor };
                }

                if (material.Color.Alpha < 1.0 || textureHasTransparency)
                {
                    gltfMaterial.AlphaMode = glTFLoader.Schema.Material.AlphaModeEnum.BLEND;
                }
                else
                {
                    gltfMaterial.AlphaMode = glTFLoader.Schema.Material.AlphaModeEnum.OPAQUE;
                }

                materialDict.Add(gltfMaterial.Name, matId);
                matId++;
            }

            if (materials.Count > 0)
            {
                gltf.Materials = newMaterials.ToArray(newMaterials.Count);
            }
            if (textures.Count > 0)
            {
                gltf.Textures = textures.ToArray(textures.Count);
            }
            if (images.Count > 0)
            {
                gltf.Images = images.ToArray(images.Count);
            }
            if (samplers.Count > 0)
            {
                gltf.Samplers = samplers.ToArray(samplers.Count);
            }

            return materialDict;
        }

        private static void AddExtension(Gltf gltf, glTFLoader.Schema.Material gltfMaterial, string extensionName, Dictionary<string, object> extensionAttributes)
        {
            if (gltfMaterial.Extensions == null)
            {
                gltfMaterial.Extensions = new Dictionary<string, object>();
            }
            if (!gltf.ExtensionsUsed.Contains(extensionName))
            {
                gltf.ExtensionsUsed = new List<string>(gltf.ExtensionsUsed) { extensionName }.ToArray();
            }
            gltfMaterial.Extensions.Add(extensionName, extensionAttributes);
        }

        private static Image CreateImage(string path, List<BufferView> bufferViews, List<byte> buffer, out bool textureHasTransparency)
        {
            var image = new Image();

            using (var ms = new MemoryStream())
            {
                // Flip the texture image vertically
                // to align with OpenGL convention.
                // 0,1  1,1
                // 0,0  1,0
                using (var texImage = SixLabors.ImageSharp.Image.Load(path))
                {
                    PngMetadata meta = texImage.Metadata.GetPngMetadata();
                    textureHasTransparency = meta.ColorType == PngColorType.RgbWithAlpha || meta.ColorType == PngColorType.GrayscaleWithAlpha;
                    texImage.Mutate(x => x.Flip(FlipMode.Vertical));
                    texImage.Save(ms, new PngEncoder());
                }
                var imageData = ms.ToArray();
                image.BufferView = AddBufferView(bufferViews, 0, buffer.Count, imageData.Length, null, null);
                buffer.AddRange(imageData);
            }

            while (buffer.Count % 4 != 0)
            {
                buffer.Add(0);
            }

            image.MimeType = Image.MimeTypeEnum.image_png;
            return image;
        }

        private static Sampler CreateSampler(bool repeatTexture)
        {
            var sampler = new Sampler
            {
                MagFilter = Sampler.MagFilterEnum.LINEAR,
                MinFilter = Sampler.MinFilterEnum.LINEAR,
                WrapS = repeatTexture ? Sampler.WrapSEnum.REPEAT : Sampler.WrapSEnum.CLAMP_TO_EDGE,
                WrapT = repeatTexture ? Sampler.WrapTEnum.REPEAT : Sampler.WrapTEnum.CLAMP_TO_EDGE
            };
            return sampler;
        }

        internal static void AddLights(this Gltf gltf, List<Light> lights, List<Node> nodes)
        {
            gltf.Extensions = new Dictionary<string, object>();
            var lightCount = 0;
            var lightsArr = new List<object>();
            foreach (var light in lights)
            {
                // Create the top level collection of lights.
                var gltfLight = new Dictionary<string, object>(){
                    {"color", new[]{light.Color.Red, light.Color.Green, light.Color.Blue}},
                    {"type", Enum.GetName(typeof(LightType), light.LightType).ToLower()},
                    {"intensity", light.Intensity},
                    {"name", light.Name ?? string.Empty}
                };
                if (light.LightType == LightType.Spot)
                {
                    gltfLight["spot"] = new Dictionary<string, double>(){
                        {"innerConeAngle", ((SpotLight)light).InnerConeAngle},
                        {"outerConeAngle", ((SpotLight)light).OuterConeAngle}
                    };
                }
                lightsArr.Add(gltfLight);

                // Create the light nodes
                var lightNode = new Node
                {
                    Extensions = new Dictionary<string, object>(){
                    {"KHR_lights_punctual", new Dictionary<string,object>(){
                        {"light", lightCount}
                    }}
                }
                };

                var ml = light.Transform.Matrix;
                lightNode.Matrix = new float[]{
                (float)ml.m11, (float)ml.m12, (float)ml.m13, 0f,
                (float)ml.m21, (float)ml.m22, (float)ml.m23, 0f,
                (float)ml.m31, (float)ml.m32, (float)ml.m33, 0f,
                (float)ml.tx, (float)ml.ty, (float)ml.tz, 1f};

                NodeUtilities.AddNode(nodes, lightNode, 0);
                lightCount++;
            }

            if (lightsArr.Count > 0)
            {
                gltf.Extensions.Add("KHR_lights_punctual", new Dictionary<string, object>{
                    {"lights", lightsArr}
                });
            }
        }

        private static int AddAccessor(List<Accessor> accessors,
                                       int bufferView,
                                       int byteOffset,
                                       Accessor.ComponentTypeEnum componentType,
                                       int count,
                                       float[] min,
                                       float[] max,
                                       Accessor.TypeEnum accessorType)
        {
            var a = new Accessor
            {
                BufferView = bufferView,
                ByteOffset = byteOffset,
                ComponentType = componentType,
                Min = min,
                Max = max,
                Type = accessorType,
                Count = count
            };

            accessors.Add(a);

            return accessors.Count - 1;
        }

        private static int AddBufferView(List<BufferView> bufferViews, int buffer, int byteOffset, int byteLength, BufferView.TargetEnum? target, int? byteStride)
        {
            var b = new BufferView
            {
                Buffer = buffer,
                ByteLength = byteLength,
                ByteOffset = byteOffset,
                Target = target,
                ByteStride = byteStride
            };

            bufferViews.Add(b);

            return bufferViews.Count - 1;
        }

        internal static int AddTriangleMesh(string name,
                                            List<byte> buffer,
                                            List<BufferView> bufferViews,
                                            List<Accessor> accessors,
                                            int materialId,
                                            GraphicsBuffers gBuffers,
                                            List<glTFLoader.Schema.Mesh> meshes)
        {
            var m = new glTFLoader.Schema.Mesh
            {
                Name = name
            };

            var vBuff = AddBufferView(bufferViews, 0, buffer.Count, gBuffers.Vertices.Count, null, null);
            buffer.AddRange(gBuffers.Vertices);

            var nBuff = AddBufferView(bufferViews, 0, buffer.Count, gBuffers.Normals.Count, null, null);
            buffer.AddRange(gBuffers.Normals);

            var iBuff = AddBufferView(bufferViews, 0, buffer.Count, gBuffers.Indices.Count, null, null);
            buffer.AddRange(gBuffers.Indices);

            while (buffer.Count % 4 != 0)
            {
                buffer.Add(0);
            }

            var vAccess = AddAccessor(accessors,
                                      vBuff,
                                      0,
                                      Accessor.ComponentTypeEnum.FLOAT,
                                      gBuffers.Vertices.Count / sizeof(float) / 3,
                                      new[] { (float)gBuffers.VMin[0], (float)gBuffers.VMin[1], (float)gBuffers.VMin[2] },
                                      new[] { (float)gBuffers.VMax[0], (float)gBuffers.VMax[1], (float)gBuffers.VMax[2] },
                                      Accessor.TypeEnum.VEC3);
            var nAccess = AddAccessor(accessors,
                                      nBuff,
                                      0,
                                      Accessor.ComponentTypeEnum.FLOAT,
                                      gBuffers.Normals.Count / sizeof(float) / 3,
                                      new[] { (float)gBuffers.NMin[0], (float)gBuffers.NMin[1], (float)gBuffers.NMin[2] },
                                      new[] { (float)gBuffers.NMax[0], (float)gBuffers.NMax[1], (float)gBuffers.NMax[2] },
                                      Accessor.TypeEnum.VEC3);
            var iAccess = AddAccessor(accessors,
                                      iBuff,
                                      0,
                                      Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                                      gBuffers.Indices.Count / sizeof(ushort),
                                      new[] { (float)gBuffers.IMin },
                                      new[] { (float)gBuffers.IMax },
                                      Accessor.TypeEnum.SCALAR);

            var prim = new MeshPrimitive
            {
                Indices = iAccess,
                Material = materialId,
                Mode = MeshPrimitive.ModeEnum.TRIANGLES,
                Attributes = new Dictionary<string, int>{
                {"NORMAL",nAccess},
                {"POSITION",vAccess}
            }
            };

            if (gBuffers.UVs.Count > 0)
            {
                var uvBuff = AddBufferView(bufferViews, 0, buffer.Count, gBuffers.UVs.Count, null, null);
                buffer.AddRange(gBuffers.UVs);
                var uvAccess = AddAccessor(accessors,
                                           uvBuff,
                                           0,
                                           Accessor.ComponentTypeEnum.FLOAT,
                                           gBuffers.UVs.Count / sizeof(float) / 2,
                                           new[] { (float)gBuffers.UVMin[0], (float)gBuffers.UVMin[1] },
                                           new[] { (float)gBuffers.UVMax[0], (float)gBuffers.UVMax[1] },
                                           Accessor.TypeEnum.VEC2);
                prim.Attributes.Add("TEXCOORD_0", uvAccess);
            }

            // TODO: Add to the buffer above instead of inside this block.
            // There's a chance the padding operation will put padding before
            // the color information.
            if (gBuffers.Colors.Count > 0)
            {
                var cBuff = AddBufferView(bufferViews, 0, buffer.Count, gBuffers.Colors.Count, null, null);
                buffer.AddRange(gBuffers.Colors);
                var cAccess = AddAccessor(accessors,
                                          cBuff,
                                          0,
                                          Accessor.ComponentTypeEnum.FLOAT,
                                          gBuffers.Colors.Count / sizeof(float) / 3,
                                          new[] { (float)gBuffers.CMin[0], (float)gBuffers.CMin[1], (float)gBuffers.CMin[2] },
                                          new[] { (float)gBuffers.CMax[0], (float)gBuffers.CMax[1], (float)gBuffers.CMax[2] },
                                          Accessor.TypeEnum.VEC3);
                prim.Attributes.Add("COLOR_0", cAccess);
            }

            m.Primitives = new[] { prim };

            // Add mesh to gltf
            meshes.Add(m);

            return meshes.Count - 1;
        }

        internal static int AddPointsOrLines(string name,
                                        List<byte> buffer,
                                        List<BufferView> bufferViews,
                                        List<Accessor> accessors,
                                        int materialId,
                                        List<GraphicsBuffers> gBuffersList,
                                        MeshPrimitive.ModeEnum mode,
                                        List<glTFLoader.Schema.Mesh> meshes,
                                        List<Node> nodes,
                                        Transform transform = null)
        {
            var m = new glTFLoader.Schema.Mesh
            {
                Name = name,
                Primitives = new MeshPrimitive[gBuffersList.Count()]
            };
            for (var idx = 0; idx < gBuffersList.Count(); idx++)
            {
                var gBuffers = gBuffersList[idx];
                var vBuff = AddBufferView(bufferViews, 0, buffer.Count, gBuffers.Vertices.Count, null, null);
                var iBuff = AddBufferView(bufferViews, 0, buffer.Count + gBuffers.Vertices.Count, gBuffers.Indices.Count, null, null);

                buffer.AddRange(gBuffers.Vertices);
                buffer.AddRange(gBuffers.Indices);

                while (buffer.Count % 4 != 0)
                {
                    buffer.Add(0);
                }

                var vAccess = AddAccessor(accessors,
                                          vBuff,
                                          0,
                                          Accessor.ComponentTypeEnum.FLOAT,
                                          gBuffers.Vertices.Count / sizeof(float) / 3,
                                          new[] { (float)gBuffers.VMin[0], (float)gBuffers.VMin[1], (float)gBuffers.VMin[2] },
                                          new[] { (float)gBuffers.VMax[0], (float)gBuffers.VMax[1], (float)gBuffers.VMax[2] },
                                          Accessor.TypeEnum.VEC3);
                var iAccess = AddAccessor(accessors,
                                          iBuff,
                                          0,
                                          Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                                          gBuffers.Indices.Count / sizeof(ushort),
                                          new[] { (float)gBuffers.IMin },
                                          new[] { (float)gBuffers.IMax },
                                          Accessor.TypeEnum.SCALAR);

                var prim = new MeshPrimitive
                {
                    Indices = iAccess,
                    Material = materialId,
                    Mode = mode,
                    Attributes = new Dictionary<string, int>{
                        {"POSITION",vAccess}
                    }
                };

                if (gBuffers.Colors.Count > 0)
                {
                    var cBuff = AddBufferView(bufferViews, 0, buffer.Count, gBuffers.Colors.Count, null, null);
                    buffer.AddRange(gBuffers.Colors);
                    var cAccess = AddAccessor(accessors,
                                              cBuff,
                                              0,
                                              Accessor.ComponentTypeEnum.FLOAT,
                                              gBuffers.Colors.Count / sizeof(float) / 3,
                                              new[] { (float)gBuffers.CMin[0], (float)gBuffers.CMin[1], (float)gBuffers.CMin[2] },
                                              new[] { (float)gBuffers.CMax[0], (float)gBuffers.CMax[1], (float)gBuffers.CMax[2] },
                                              Accessor.TypeEnum.VEC3);
                    prim.Attributes.Add("COLOR_0", cAccess);
                }

                m.Primitives[idx] = prim;
            }

            // Add mesh to gltf
            meshes.Add(m);

            var parentId = 0;

            if (transform != null)
            {
                parentId = NodeUtilities.CreateAndAddTransformNode(nodes, transform, parentId);
            }

            // Add mesh node to gltf
            var node = new Node
            {
                Mesh = meshes.Count - 1
            };
            NodeUtilities.AddNode(nodes, node, parentId);

            return meshes.Count - 1;
        }

        internal static void ToGlb(this Solid solid, string path)
        {
            var gltf = new Gltf();
            var asset = new Asset
            {
                Version = "2.0",
                Generator = "hypar-gltf"
            };

            gltf.Asset = asset;

            var root = new Node
            {
                Translation = new[] { 0.0f, 0.0f, 0.0f },
                Scale = new[] { 1.0f, 1.0f, 1.0f }
            };

            // Set Z up by rotating -90d around the X Axis
            var q = new Quaternion(new Vector3(1, 0, 0), -Math.PI / 2);
            root.Rotation = new[]{
                (float)q.X, (float)q.Y, (float)q.Z, (float)q.W
            };

            var meshes = new List<glTFLoader.Schema.Mesh>();
            var nodes = new List<Node> { root };

            gltf.Scene = 0;
            var scene = new Scene
            {
                Nodes = new[] { 0 }
            };
            gltf.Scenes = new[] { scene };

            gltf.ExtensionsUsed = new[] { "KHR_materials_pbrSpecularGlossiness", "KHR_materials_unlit" };

            var buffer = new List<byte>();
            var bufferViews = new List<BufferView>();

            var materials = gltf.AddMaterials(new[] { BuiltInMaterials.Default, BuiltInMaterials.Edges, BuiltInMaterials.EdgesHighlighted },
                                              buffer,
                                              bufferViews);

            var mesh = new Geometry.Mesh();
            solid.Tessellate(ref mesh);
            mesh.ComputeNormals();

            var gbuffers = mesh.GetBuffers();

            var accessors = new List<Accessor>();

            var meshId = AddTriangleMesh("mesh",
                                         buffer,
                                         bufferViews,
                                         accessors,
                                         materials[BuiltInMaterials.Default.Id.ToString()],
                                         gbuffers,
                                         meshes);

            NodeUtilities.CreateNodeForMesh(meshId, nodes, null);

            var edgeCount = 0;
            var vertices = new List<Vector3>();
            var verticesHighlighted = new List<Vector3>();
            foreach (var e in solid.Edges.Values)
            {
                if (e.Left.Loop == null || e.Right.Loop == null)
                {
                    verticesHighlighted.AddRange(new[] { e.Left.Vertex.Point, e.Right.Vertex.Point });
                }
                else
                {
                    vertices.AddRange(new[] { e.Left.Vertex.Point, e.Right.Vertex.Point });
                }

                edgeCount++;
            }

            if (vertices.Count > 0)
            {
                // Draw standard edges
                var id = $"{100000}_curve";
                var gb = vertices.ToGraphicsBuffers();
                AddPointsOrLines(id,
                                 buffer,
                                 bufferViews,
                                 accessors,
                                 materials[BuiltInMaterials.Edges.Id.ToString()],
                                 new List<GraphicsBuffers>() { gb },
                                 MeshPrimitive.ModeEnum.LINES,
                                 meshes,
                                 nodes,
                                 null);
            }

            if (verticesHighlighted.Count > 0)
            {
                // Draw highlighted edges
                var id = $"{100001}_curve";
                var gb = verticesHighlighted.ToGraphicsBuffers();
                AddPointsOrLines(id,
                                 buffer,
                                 bufferViews,
                                 accessors,
                                 materials[BuiltInMaterials.EdgesHighlighted.Id.ToString()],
                                 new List<GraphicsBuffers>() { gb },
                                 MeshPrimitive.ModeEnum.LINES,
                                 meshes,
                                 nodes,
                                 null);
            }

            var buff = new glTFLoader.Schema.Buffer
            {
                ByteLength = buffer.Count
            };
            gltf.Buffers = new[] { buff };

            gltf.BufferViews = bufferViews.ToArray(bufferViews.Count);
            gltf.Accessors = accessors.ToArray(accessors.Count);
            gltf.Nodes = nodes.ToArray(nodes.Count);
            if (meshes.Count > 0)
            {
                gltf.Meshes = meshes.ToArray(meshes.Count);
            }

            gltf.SaveBinaryModel(buffer.ToArray(buffer.Count), path);
        }

        /// <returns>Whether a Glb was successfully saved. False indicates that there was no geometry to save.</returns>
        private static bool SaveGlb(Model model, string path, out List<BaseError> errors, bool drawEdges = false, bool mergeVertices = false)
        {
            var gltf = InitializeGlTF(model, out var buffers, out errors, drawEdges, mergeVertices);
            if (gltf == null)
            {
                return false;
            }

            //TODO handle initializing multiple gltf buffers at once.
            var mergedBuffer = gltf.CombineBufferAndFixRefs(buffers);

            gltf.SaveBinaryModel(mergedBuffer, path);
            return true;
        }

        /// <returns>Whether a Glb was successfully saved. False indicates that there was no geometry to save.</returns>
        private static bool SaveGltf(Model model, string path, out List<BaseError> errors, bool drawEdges = false, bool mergeVertices = false)
        {
            var gltf = InitializeGlTF(model, out List<byte[]> buffers, out errors, drawEdges, mergeVertices);
            if (gltf == null)
            {
                return false;
            }

            // Buffers must be saved first, URIs may be set or modified inside this method.
            gltf.SaveBuffersAndAddUris(path, buffers);
            gltf.SaveModel(path);
            return true;
        }

        internal static Gltf InitializeGlTF(Model model,
                                            out List<byte[]> allBuffers,
                                            out List<BaseError> errors,
                                            bool drawEdges = false,
                                            bool mergeVertices = false)
        {
            errors = new List<BaseError>();
            var schemaBuffer = new glTFLoader.Schema.Buffer();
            var schemaBuffers = new List<glTFLoader.Schema.Buffer> { schemaBuffer };

            // Attempt to pre-allocate these lists. This won't be perfect.
            // Before processing the geometry of an element we can't know
            // how much to allocate. In the worst case, this will reduce 
            // the list resizing.
            // TODO: In future work where we update element geometry during 
            // UpdateRepresentations, we will know at this moment how big the
            // geometry for an element is, and we should tighten this up.
            var elementCount = model.AllElementsAssignableFromType<GeometricElement>().Count();
            var buffer = new List<byte>(elementCount * GraphicsBuffers.PreallocationSize());
            var contentElementCount = model.AllElementsAssignableFromType<ContentElement>().Count();
            allBuffers = new List<byte[]>(contentElementCount * GraphicsBuffers.PreallocationSize()) { Array.Empty<byte>() };

            var gltf = new Gltf();
            var asset = new Asset
            {
                Version = "2.0",
                Generator = "hypar-gltf"
            };

            gltf.Asset = asset;

            var root = new Node();

            var rootTransform = new Transform(model.Transform);

            // Rotate the transform for +Z up.
            rootTransform.Rotate(new Vector3(1, 0, 0), -90.0);
            var m = rootTransform.Matrix;
            root.Matrix = new float[]{
                (float) m.m11, (float) m.m12, (float) m.m13, 0f,
                (float) m.m21, (float) m.m22, (float) m.m23, 0f,
                (float) m.m31, (float) m.m32, (float) m.m33, 0f,
                (float) m.tx, (float) m.ty, (float) m.tz, 1f};

            var nodes = new List<Node> { root };
            var meshes = new List<glTFLoader.Schema.Mesh>();

            gltf.Scene = 0;
            var scene = new Scene
            {
                Nodes = new[] { 0 }
            };
            gltf.Scenes = new[] { scene };

            var lights = model.AllElementsOfType<Light>().ToList();
            gltf.ExtensionsUsed = lights.Any() ? new[] {
                "KHR_materials_pbrSpecularGlossiness",
                "KHR_materials_unlit",
                "KHR_lights_punctual"
            } : new[] {
                "KHR_materials_pbrSpecularGlossiness",
                "KHR_materials_unlit"};

            var bufferViews = new List<BufferView>();

            var materialsToAdd = model.AllElementsOfType<Material>().ToList();
            if (drawEdges)
            {
                materialsToAdd.Add(BuiltInMaterials.Edges);
            }

            // Gltf can't handle a materials array with zero materials.
            // So we add the default material if this happens.
            if (materialsToAdd.Count == 0)
            {
                materialsToAdd.Add(BuiltInMaterials.Default);
            }

            var materialIndexMap = gltf.AddMaterials(materialsToAdd, buffer, bufferViews);

            var elements = model.Elements.Where(e =>
            {
                return e.Value is GeometricElement || e.Value is ElementInstance;
            }).Select(e => e.Value).ToList();


            if (lights.Any())
            {
                gltf.AddLights(lights, nodes);
            }

            // Lines are stored in a list of lists
            // according to the max available index size.
            var lines = new List<List<Vector3>>();
            var currLines = new List<Vector3>();
            lines.Add(currLines);

            var accessors = new List<Accessor>();
            var textures = new List<Texture>();
            var images = new List<Image>();
            var samplers = new List<Sampler>();
            var animations = new List<glTFLoader.Schema.Animation>();
            var materials = gltf.Materials != null ? gltf.Materials.ToList() : new List<glTFLoader.Schema.Material>();

            var meshElementMap = new Dictionary<Guid, List<int>>();
            var nodeElementMap = new Dictionary<Guid, ProtoNode>();
            var meshTransformMap = new Dictionary<Guid, Transform>();
            foreach (var e in elements)
            {
                // Check if we'll overrun the index size
                // for the current line array. If so,
                // create a new line array.
                if (currLines.Count * 2 > ushort.MaxValue)
                {
                    currLines = new List<Vector3>();
                    lines.Add(currLines);
                }

                try
                {
                    GetRenderDataForElement(e,
                                            gltf,
                                            materialIndexMap,
                                            buffer,
                                            allBuffers,
                                            schemaBuffers,
                                            bufferViews,
                                            accessors,
                                            materials,
                                            textures,
                                            images,
                                            samplers,
                                            meshes,
                                            nodes,
                                            meshElementMap,
                                            nodeElementMap,
                                            meshTransformMap,
                                            currLines,
                                            drawEdges,
                                            animations,
                                            mergeVertices);
                }
                catch (Exception ex)
                {
                    errors.Add(new ElementError(e.Id, ex));
                }
            }
            if (allBuffers.Sum(b => b.Count()) + buffer.Count == 0 && lights.Count == 0)
            {
                return null;
            }

            if (drawEdges && lines.Count() > 0)
            {
                foreach (var lineSet in lines)
                {
                    if (lineSet.Count == 0)
                    {
                        continue;
                    }
                    var id = $"{GetNextId()}_edge";
                    var gb = lineSet.ToGraphicsBuffers();
                    AddPointsOrLines(id,
                                     buffer,
                                     bufferViews,
                                     accessors,
                                     materialIndexMap[BuiltInMaterials.Edges.Id.ToString()],
                                     new List<GraphicsBuffers>() { gb },
                                     MeshPrimitive.ModeEnum.LINES,
                                     meshes,
                                     nodes,
                                     null);
                }
            }

            if (buffer.Count > 0)
            {
                schemaBuffers[0].ByteLength = buffer.Count;
            }
            gltf.Buffers = schemaBuffers.ToArray(schemaBuffers.Count);
            if (bufferViews.Count > 0)
            {
                gltf.BufferViews = bufferViews.ToArray(bufferViews.Count);
            }
            if (accessors.Count > 0)
            {
                gltf.Accessors = accessors.ToArray(accessors.Count);
            }

            gltf.Materials = materials.ToArray(materials.Count);
            if (textures.Count > 0)
            {
                gltf.Textures = textures.ToArray(textures.Count);
            }
            if (images.Count > 0)
            {
                gltf.Images = images.ToArray(images.Count);
            }
            if (samplers.Count > 0)
            {
                gltf.Samplers = samplers.ToArray(samplers.Count);
            }
            if (animations.Count > 0)
            {
                gltf.Animations = animations.ToArray(animations.Count);
            }
            gltf.Nodes = nodes.ToArray(nodes.Count);
            if (meshes.Count > 0)
            {
                gltf.Meshes = meshes.ToArray(meshes.Count);
            }

            // This is a hack! For web assembly, the ToArray() call creates
            // a copy of all items in the list, and is extremely slow. We
            // get around this by accessing the underlying list directly.
            // https://stackoverflow.com/a/4973060
            // allBuffers[0] = buffer.ToArray(buffer.Count);
            buffer.TrimExcess();
            allBuffers[0] = (byte[])typeof(List<byte>)
                   .GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)
                   .GetValue(buffer);

            return gltf;
        }

        private static void GetRenderDataForElement(Element e,
                                                    Gltf gltf,
                                                    Dictionary<string, int> materialIndexMap,
                                                    List<byte> buffer,
                                                    List<byte[]> allBuffers,
                                                    List<glTFLoader.Schema.Buffer> schemaBuffers,
                                                    List<BufferView> bufferViews,
                                                    List<Accessor> accessors,
                                                    List<glTFLoader.Schema.Material> materials,
                                                    List<Texture> textures,
                                                    List<Image> images,
                                                    List<Sampler> samplers,
                                                    List<glTFLoader.Schema.Mesh> meshes,
                                                    List<Node> nodes,
                                                    Dictionary<Guid, List<int>> meshElementMap,
                                                    Dictionary<Guid, ProtoNode> nodeElementMap,
                                                    Dictionary<Guid, Transform> meshTransformMap,
                                                    List<Vector3> lines,
                                                    bool drawEdges,
                                                    List<glTFLoader.Schema.Animation> animations,
                                                    bool mergeVertices = false)
        {
            var materialId = BuiltInMaterials.Default.Id.ToString();
            int meshId = -1;
            int nodeId = -1;

            if (e is GeometricElement element)
            {
                if (typeof(ContentElement).IsAssignableFrom(e.GetType()))
                {
                    var content = e as ContentElement;
                    Stream glbStream = GetGlbStreamFromPath(content.GltfLocation);
                    if (glbStream != Stream.Null)
                    {
                        GltfMergingUtils.AddAllMeshesFromFromGlb(glbStream,
                                                                 schemaBuffers,
                                                                 allBuffers,
                                                                 bufferViews,
                                                                 accessors,
                                                                 meshes,
                                                                 materials,
                                                                 textures,
                                                                 images,
                                                                 samplers,
                                                                 true,
                                                                 e.Id,
                                                                 out var parentNode);


                        if (!nodeElementMap.ContainsKey(e.Id) && parentNode != null)
                        {
                            nodeElementMap.Add(e.Id, parentNode);
                        }
                        if (!content.IsElementDefinition)
                        {
                            // This element is not used for instancing.
                            // apply scale transform here to bring the content glb into meters
                            var transform = content.Transform.Scaled(content.GltfScaleToMeters);
                            NodeUtilities.CreateNodeForMesh(meshId, nodes, transform);
                        }
                        else
                        {
                            // This element will be used for instancing.  Save the transform of the
                            // content element base that will be needed when instances are placed.
                            // The scaled transform is only necessary because we are using the glb.
                            if (!meshTransformMap.ContainsKey(e.Id))
                            {
                                meshTransformMap[e.Id] = content.Transform.Scaled(content.GltfScaleToMeters);
                            }
                        }
                    }
                    else
                    {
                        meshId = ProcessGeometricRepresentation(e,
                                                                ref materialIndexMap,
                                                                ref buffer,
                                                                bufferViews,
                                                                accessors,
                                                                meshes,
                                                                nodes,
                                                                materialId,
                                                                ref meshId,
                                                                content,
                                                                out nodeId,
                                                                mergeVertices);
                        if (!meshElementMap.ContainsKey(e.Id))
                        {
                            meshElementMap.Add(e.Id, new List<int> { meshId });
                        }
                    }
                }
                else
                {
                    var geometricElement = element;
                    materialId = geometricElement.Material.Id.ToString();

                    meshId = ProcessGeometricRepresentation(e,
                                                            ref materialIndexMap,
                                                            ref buffer,
                                                            bufferViews,
                                                            accessors,
                                                            meshes,
                                                            nodes,
                                                            materialId,
                                                            ref meshId,
                                                            geometricElement,
                                                            out nodeId,
                                                            mergeVertices);
                    if (meshId > -1 && !meshElementMap.ContainsKey(e.Id))
                    {
                        meshElementMap.Add(e.Id, new List<int> { meshId });
                    }
                }
            }

            if (e is ElementInstance i)
            {
                var transform = new Transform();
                if (i.BaseDefinition is ContentElement)
                {
                    // if we have a stored node for this object, we use that when adding it to the gltf.
                    if (nodeElementMap.TryGetValue(i.BaseDefinition.Id, out _))
                    {
                        transform.Concatenate(i.Transform);
                        NodeUtilities.AddInstanceAsCopyOfNode(nodes, nodeElementMap[i.BaseDefinition.Id], transform, i.Id);
                    }
                    else
                    {
                        // If there is a transform stored for the content base definition we
                        // should apply it when creating instances.
                        // TODO check if this meshTransformMap ever does anything.
                        if (meshTransformMap.TryGetValue(i.BaseDefinition.Id, out var baseTransform))
                        {
                            transform.Concatenate(baseTransform);
                        }
                        transform.Concatenate(i.Transform);
                        NodeUtilities.AddInstanceNode(nodes, meshElementMap[i.BaseDefinition.Id], transform);
                    }
                }
                else
                {
                    transform.Concatenate(i.Transform);
                    // Lookup the corresponding mesh in the map.
                    NodeUtilities.AddInstanceNode(nodes, meshElementMap[i.BaseDefinition.Id], transform);
                }

                if (drawEdges)
                {
                    // Get the edges for the solid
                    var geom = i.BaseDefinition;
                    if (geom.Representation != null)
                    {
                        foreach (var solidOp in geom.Representation.SolidOperations)
                        {
                            if (solidOp.Solid != null)
                            {
                                foreach (var edge in solidOp.Solid.Edges.Values)
                                {
                                    lines.AddRange(new[] { i.Transform.OfPoint(edge.Left.Vertex.Point), i.Transform.OfPoint(edge.Right.Vertex.Point) });
                                }
                            }
                        }
                    }
                }
            }

            if (e is GeometricElement ge && !ge.IsElementDefinition)
            {
                if (ge.TryToGraphicsBuffers(out List<GraphicsBuffers> gb, out string id, out MeshPrimitive.ModeEnum? mode))
                {
                    AddPointsOrLines(id,
                                     buffer,
                                     bufferViews,
                                     accessors,
                                     materialIndexMap[ge.Material.Id.ToString()],
                                     gb,
                                     (MeshPrimitive.ModeEnum)mode,
                                     meshes,
                                     nodes,
                                     ge.Transform);
                }

                if (ge.Animation != null && nodeId != -1)
                {
                    // https://github.com/KhronosGroup/glTF-Tutorials/blob/master/gltfTutorial/gltfTutorial_007_Animations.md

                    var channels = new List<AnimationChannel>();
                    var animationSamplers = new List<AnimationSampler>();

                    if (ge.Animation.HasAnimatedScale())
                    {
                        var scaleTimeBufferView = AddBufferView(bufferViews, 0, buffer.Count, ge.Animation.ScaleTimes.Length, null, null);
                        buffer.AddRange(ge.Animation.ScaleTimes);
                        var scaleTimeAccessor = AddAccessor(accessors,
                                                            scaleTimeBufferView,
                                                            0,
                                                            Accessor.ComponentTypeEnum.FLOAT,
                                                            ge.Animation.ScaleTimes.Length / sizeof(float),
                                                            new float[] { ge.Animation.ScaleTimeMin },
                                                            new float[] { ge.Animation.ScaleTimeMax },
                                                            Accessor.TypeEnum.SCALAR);

                        var scaleBufferView = AddBufferView(bufferViews, 0, buffer.Count, ge.Animation.Scales.Length, null, null);
                        buffer.AddRange(ge.Animation.Scales);
                        var scaleAccessor = AddAccessor(accessors,
                                                            scaleBufferView,
                                                            0,
                                                            Accessor.ComponentTypeEnum.FLOAT,
                                                            ge.Animation.Scales.Length / sizeof(float) / 3,
                                                            ge.Animation.ScaleMin,
                                                            ge.Animation.ScaleMax,
                                                            Accessor.TypeEnum.VEC3);

                        var scaleSampler = new AnimationSampler
                        {
                            Input = scaleTimeAccessor,  // time
                            Output = scaleAccessor,    // scale
                            Interpolation = AnimationSampler.InterpolationEnum.LINEAR
                        };

                        var scaleTarget = new AnimationChannelTarget
                        {
                            Node = nodeId,
                            Path = AnimationChannelTarget.PathEnum.scale
                        };

                        var scaleChannel = new AnimationChannel()
                        {
                            Target = scaleTarget,
                            Sampler = animationSamplers.Count
                        };

                        animationSamplers.Add(scaleSampler);
                        channels.Add(scaleChannel);
                    }
                    if (ge.Animation.HasAnimatedTranslation())
                    {
                        var translationTimeBufferView = AddBufferView(bufferViews, 0, buffer.Count, ge.Animation.TranslationTimes.Length, null, null);
                        buffer.AddRange(ge.Animation.TranslationTimes);
                        var translationTimeAccessor = AddAccessor(accessors,
                                                            translationTimeBufferView,
                                                            0,
                                                            Accessor.ComponentTypeEnum.FLOAT,
                                                            ge.Animation.TranslationTimes.Length / sizeof(float),
                                                            new float[] { ge.Animation.TranslationTimeMin },
                                                            new float[] { ge.Animation.TranslationTimeMax },
                                                            Accessor.TypeEnum.SCALAR);

                        var translationBufferView = AddBufferView(bufferViews, 0, buffer.Count, ge.Animation.Translations.Length, null, null);
                        buffer.AddRange(ge.Animation.Translations);
                        var translationAccessor = AddAccessor(accessors,
                                                            translationBufferView,
                                                            0,
                                                            Accessor.ComponentTypeEnum.FLOAT,
                                                            ge.Animation.Translations.Length / sizeof(float) / 3,
                                                            ge.Animation.TranslationMin,
                                                            ge.Animation.TranslationMax,
                                                            Accessor.TypeEnum.VEC3);

                        var translationSampler = new AnimationSampler
                        {
                            Input = translationTimeAccessor,  // time
                            Output = translationAccessor,    // scale
                            Interpolation = AnimationSampler.InterpolationEnum.LINEAR
                        };

                        var translationTarget = new AnimationChannelTarget
                        {
                            Node = nodeId,
                            Path = AnimationChannelTarget.PathEnum.translation
                        };

                        var translationChannel = new AnimationChannel()
                        {
                            Target = translationTarget,
                            Sampler = animationSamplers.Count
                        };

                        animationSamplers.Add(translationSampler);
                        channels.Add(translationChannel);
                    }
                    if (ge.Animation.HasAnimatedRotation())
                    {
                        var rotationTimeBufferView = AddBufferView(bufferViews, 0, buffer.Count, ge.Animation.RotationTimes.Length, null, null);
                        buffer.AddRange(ge.Animation.RotationTimes);
                        var rotationTimeAccessor = AddAccessor(accessors,
                                                            rotationTimeBufferView,
                                                            0,
                                                            Accessor.ComponentTypeEnum.FLOAT,
                                                            ge.Animation.RotationTimes.Length / sizeof(float),
                                                            new float[] { ge.Animation.RotationTimeMin },
                                                            new float[] { ge.Animation.RotationTimeMax },
                                                            Accessor.TypeEnum.SCALAR);

                        var rotationBufferView = AddBufferView(bufferViews, 0, buffer.Count, ge.Animation.Rotations.Length, null, null);
                        buffer.AddRange(ge.Animation.Rotations);
                        var rotationAccessor = AddAccessor(accessors,
                                                            rotationBufferView,
                                                            0,
                                                            Accessor.ComponentTypeEnum.FLOAT,
                                                            ge.Animation.Rotations.Length / sizeof(float) / 4,
                                                            ge.Animation.RotationMin,
                                                            ge.Animation.RotationMax,
                                                            Accessor.TypeEnum.VEC4);

                        var rotationSampler = new AnimationSampler
                        {
                            Input = rotationTimeAccessor,  // time
                            Output = rotationAccessor,    // scale
                            Interpolation = AnimationSampler.InterpolationEnum.LINEAR
                        };

                        var rotationTarget = new AnimationChannelTarget
                        {
                            Node = nodeId,
                            Path = AnimationChannelTarget.PathEnum.rotation
                        };

                        var rotationChannel = new AnimationChannel()
                        {
                            Target = rotationTarget,
                            Sampler = animationSamplers.Count
                        };

                        animationSamplers.Add(rotationSampler);
                        channels.Add(rotationChannel);
                    }

                    var anim = new glTFLoader.Schema.Animation()
                    {
                        Channels = channels.ToArray(),
                        Samplers = animationSamplers.ToArray()
                    };

                    animations.Add(anim);
                }
            }

            if (e is ITessellate geo)
            {
                var mesh = new Geometry.Mesh();
                geo.Tessellate(ref mesh);
                if (mesh == null)
                {
                    return;
                }

                var gbuffers = mesh.GetBuffers();

                // TODO(Ian): Remove this cast to GeometricElement when we
                // consolidate mesh under geometric representations.
                meshId = AddTriangleMesh(e.Id + "_mesh",
                                         buffer,
                                         bufferViews,
                                         accessors,
                                         materialIndexMap[materialId],
                                         gbuffers,
                                         meshes);

                if (!meshElementMap.ContainsKey(e.Id))
                {
                    meshElementMap.Add(e.Id, new List<int>());
                }
                meshElementMap[e.Id].Add(meshId);

                var geom = (GeometricElement)e;
                if (!geom.IsElementDefinition)
                {
                    nodeId = NodeUtilities.CreateNodeForMesh(meshId, nodes, geom.Transform);
                }
            }
        }

        private static readonly Dictionary<string, MemoryStream> gltfCache = new Dictionary<string, MemoryStream>();

        private static void WriteGltfCacheForKey(string key, MemoryStream stream)
        {
            if (GltfCachePath == null)
            {
                return;
            }
            // write the gltfCache to disk
            var path = Path.Combine(GltfCachePath, GLTF_CACHE_FOLDER_NAME);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var filePath = Path.Combine(path, key + ".gltfcache");
            // we assume files aren't changing much, so if it's already been
            // written, we don't need to re-write it.
            if (!File.Exists(filePath))
            {
                using (var fileStream = File.Create(filePath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }
            }
        }
        private static void LoadGltfCacheFromDisk()
        {
            if (GltfCachePath == null)
            {
                return;
            }
            var path = Path.Combine(GltfCachePath, GLTF_CACHE_FOLDER_NAME);

            foreach (var file in Directory.GetFiles(path))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var filePath = Path.Combine(path, fileName + ".gltfcache");
                using (var fileStream = File.OpenRead(filePath))
                {
                    var stream = new MemoryStream();
                    fileStream.CopyTo(stream);
                    gltfCache.Add(fileName, stream);
                }
            }
        }

        /// <summary>
        /// Get a stream from a glb path, either file reference or remote.
        /// The streams are cached based on the location, so updates to files may
        /// not be reflected immediately, especially during long running parent processes.
        /// </summary>
        /// <param name="gltfLocation">The URI of the gltf binary file</param>
        public static Stream GetGlbStreamFromPath(string gltfLocation)
        {
            var gltfLocationSanitized = gltfLocation.Replace("/", "_");
            var responseStream = new MemoryStream();

            if (GltfCachePath != null && gltfCache.Count == 0)
            {
                LoadGltfCacheFromDisk();
            }
            if (gltfCache.TryGetValue(gltfLocationSanitized, out var foundStream))
            {
                foundStream.Position = 0;
                foundStream.CopyTo(responseStream);
                responseStream.Position = 0;
                return responseStream;
            }

            if (File.Exists(gltfLocation))
            {
                File.OpenRead(gltfLocation).CopyTo(responseStream);
            }
            else if (gltfLocation.StartsWith("https://"))
            {
                WebRequest request = WebRequest.Create(gltfLocation);
                var response = request.GetResponse();
                response.GetResponseStream().CopyTo(responseStream);
            }
            else
            {
                return Stream.Null;
            }
            responseStream.Position = 0;
            if (!gltfCache.ContainsKey(gltfLocationSanitized))
            {
                var cacheStream = new MemoryStream();
                responseStream.CopyTo(cacheStream);
                gltfCache.Add(gltfLocationSanitized, cacheStream);
                WriteGltfCacheForKey(gltfLocationSanitized, cacheStream);
            }
            return responseStream;
        }

        /// <summary>
        /// Returns the index of the mesh created while processing the Geometry.
        /// </summary>
        private static int ProcessGeometricRepresentation(Element e,
                                                           ref Dictionary<string, int> materialIndexMap,
                                                           ref List<byte> buffers,
                                                           List<BufferView> bufferViews,
                                                           List<Accessor> accessors,
                                                           List<glTFLoader.Schema.Mesh> meshes,
                                                           List<Node> nodes,
                                                           string materialId,
                                                           ref int meshId,
                                                           GeometricElement geometricElement,
                                                           out int nodeId,
                                                           bool mergeVertices = false)
        {
            geometricElement.UpdateRepresentations();
            geometricElement.UpdateBoundsAndComputeSolid();

            nodeId = -1;

            // TODO: Remove this when we get rid of UpdateRepresentation.
            // The only reason we don't fully exclude openings from processing
            // is to ensure that openings have some geometry that will be used
            // to compute csgs for their hosts.
            if (e.GetType() == typeof(Opening))
            {
                return -1;
            }

            if (geometricElement.Representation != null)
            {
                meshId = ProcessSolidsAsCSG(geometricElement,
                                    e.Id.ToString(),
                                    materialId,
                                    ref materialIndexMap,
                                    ref buffers,
                                    bufferViews,
                                    accessors,
                                    meshes);

                // If the id == -1, the mesh is malformed.
                // It may have no geometry.
                if (meshId == -1)
                {
                    return -1;
                }

                if (!geometricElement.IsElementDefinition)
                {
                    nodeId = NodeUtilities.CreateNodeForMesh(meshId, nodes, geometricElement.Transform);
                }
                return meshId;
            }
            return -1;
        }

        private static int ProcessSolidsAsCSG(GeometricElement geometricElement,
                                      string id,
                                      string materialId,
                                      ref Dictionary<string, int> materials,
                                      ref List<byte> buffer,
                                      List<BufferView> bufferViews,
                                      List<Accessor> accessors,
                                      List<glTFLoader.Schema.Mesh> meshes)
        {
            GraphicsBuffers buffers = null;

            // If we've explicitly skipped csg union or the element
            // only has one solid operation, we can perform this micro-optimization
            // of skipping CSG creation.
            if (geometricElement.Representation.SkipCSGUnion)
            {
                // There's a special flag on Representation that allows you to
                // skip CSG unions. In this case, we tessellate all solids
                // individually, and do no booleaning. Voids are also ignored.
                buffers = Tessellation.Tessellate<GraphicsBuffers>(geometricElement.Representation.SolidOperations.Select(so => new SolidTesselationTargetProvider(so.Solid, so.LocalTransform)),
                                        false,
                                        geometricElement.ModifyVertexAttributes);
            }
            else
            {
                buffers = geometricElement._csg.Tessellate(modifyVertexAttributes: geometricElement.ModifyVertexAttributes);
            }

            if (buffers.Vertices.Count == 0)
            {
                return -1;
            }

            return AddTriangleMesh(id + "_mesh",
                                   buffer,
                                   bufferViews,
                                   accessors,
                                   materials[materialId],
                                   buffers,
                                   meshes);
        }
    }
}
