using glTFLoader;
using glTFLoader.Schema;
using Hypar.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Hypar.Elements
{
    /// <summary>
    /// A container for Meshes and Materials
    /// </summary>
    public class Model
    {
        private List<byte> m_buffer = new List<byte>();
        private Dictionary<string, Material> m_materials = new Dictionary<string, Material>();
        private Dictionary<string, Element> m_elements = new Dictionary<string, Element>();

        public Dictionary<string, Material> Materials
        {
            get{return m_materials;}
        }

        public Dictionary<string,Element> Elements
        {
            get{return m_elements;}
        }


        public Model()
        {
            this.AddMaterial(Hypar.Elements.Materials.Default());
            this.AddMaterial(Hypar.Elements.Materials.Steel());
            this.AddMaterial(Hypar.Elements.Materials.Glass());
            this.AddMaterial(Hypar.Elements.Materials.Concrete());
        }

        /// <summary>
        /// Add a Material to the Model.
        /// </summary>
        /// <param name="m"></param>
        public void AddMaterial(Material m)
        {
            if(!this.m_materials.ContainsKey(m.Id))
            {
                this.m_materials.Add(m.Id, m);
            }
            else
            {
                throw new Exception("A Material with the same Id already exists in the Model.");
            }
        }

        /// <summary>
        /// Add a Mesh to the Model.
        /// </summary>
        /// <param name="m"></param>
        public void AddElement(Element m)
        {
            if(!this.m_elements.ContainsKey(m.Id.ToString()))
            {
                this.m_elements.Add(m.Id.ToString(), m);
            }
            else
            {
                throw new Exception("An Element with the same Id already exists in the Model.");
            }
        }

        /// <summary>
        /// Save the Model to a binary glTF file.
        /// </summary>
        /// <param name="path"></param>
        public void SaveGlb(string path)
        {
            var gltf = InitializeGlTF();

            if(File.Exists(path))
            {
                File.Delete(path);
            }

            gltf.SaveBinaryModel(m_buffer.ToArray(), path);
        }

        /// <summary>
        /// Save the Model to a glTF file.
        /// </summary>
        /// <param name="path"></param>
        public void SaveGltf(string path)
        {
            var gltf = InitializeGlTF();

            if(File.Exists(path))
            {
                File.Delete(path);
            }

            if(File.Exists("model.bin"))
            {
                File.Delete("model.bin");
            }

            var uri = Path.GetFileNameWithoutExtension(path) + ".bin";
            gltf.Buffers[0].Uri = uri;

            using (var fs = new FileStream(uri, FileMode.Create, FileAccess.Write))
            {
                fs.Write(m_buffer.ToArray(), 0, m_buffer.Count());
            }
            gltf.SaveModel(path);
        }

        private string SaveBase64()
        {
            var tmp = Path.GetTempFileName();
            var gltf = InitializeGlTF();
            gltf.SaveBinaryModel(m_buffer.ToArray(), tmp);
            var bytes = File.ReadAllBytes(tmp);
            return Convert.ToBase64String(bytes);
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string ToHypar()
        {
            var model = SaveBase64();

            var computed = new Dictionary<string,object>();
            foreach(var e in this.Elements)
            {
                if(e.Value is IDataProvider)
                {
                    var dp = e.Value as IDataProvider;
                    computed.Add(e.Value.Id.ToString(), dp.Data());
                }
            }

            var result = new Dictionary<string, object>();
            result["model"] = model;
            result["computed"] = computed;
            return JsonConvert.SerializeObject(result);
        }

        private Gltf InitializeGlTF()
        {
            var gltf = new Gltf();
            var asset = new Asset();
            asset.Version = "2.0";
            asset.Generator = "hypar-gltf";

            gltf.Asset = asset;

            var root = new Node();
            
            root.Translation = new[]{0.0f,0.0f,0.0f};
            root.Scale = new[]{1.0f,1.0f,1.0f};

            // Set Z up by rotating -90d around the X Axis
            var q = new Quaternion(new Vector3(1,0,0), -Math.PI/2);
            root.Rotation = new[]{
                (float)q.X, (float)q.Y, (float)q.Z, (float)q.W
            };

            gltf.Nodes = new[]{root};

            gltf.Scene = 0;
            var scene = new Scene();
            scene.Nodes = new[]{0};
            gltf.Scenes = new[]{scene};

            gltf.ExtensionsUsed = new[]{"KHR_materials_pbrSpecularGlossiness"};
            
            var materials = new Dictionary<string, int>();
            foreach(var kvp in this.m_materials)
            {
                var m = kvp.Value;
                var mId = gltf.AddMaterial(m.Id, m.Red, m.Green, m.Blue, m.Alpha, m.SpecularFactor, m.GlossinessFactor);
                materials.Add(m.Id, mId);
            }

            foreach(var kvp in this.m_elements)
            {
                var e = kvp.Value;
                if(e is IMeshProvider)
                {
                    var mp = e as IMeshProvider;
                    var mesh = mp.Tessellate();
                    gltf.AddTriangleMesh(m_buffer, mesh.Vertices.ToArray(), mesh.Normals.ToArray(), mesh.Indices.ToArray(), materials[e.Material.Id], null, e.Transform);

                }
            }

            var buff = new glTFLoader.Schema.Buffer();
            buff.ByteLength = m_buffer.Count();
            gltf.Buffers = new[]{buff};

            return gltf;
        }
    }
}