using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Elements;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Elements.Serialization.JSON;

namespace Elements.Tests
{
    public class ModelTests
    {
        [Fact]
        public void Construct()
        {
            var model = new Model();
            Assert.NotNull(model);
        }

        [Fact]
        public void SaveToGltf()
        {
            var model = QuadPanelModel();
            model.ToGlTF("models/SaveToGltf.gltf", false);
            Assert.True(File.Exists("models/SaveToGltf.gltf"));
        }

        [Fact]
        public void SaveToGlb()
        {
            var model = QuadPanelModel();
            model.ToGlTF("models/SaveToGlb.glb");
            Assert.True(File.Exists("models/SaveToGlb.glb"));
        }

        [Fact]
        public void SaveToBase64()
        {
            var model = QuadPanelModel();
            var base64 = model.ToBase64String();
            var bytes = Convert.FromBase64String(base64);
            File.WriteAllBytes("models/SaveFromBase64String.glb", bytes);
            Assert.True(File.Exists("models/SaveFromBase64String.glb"));
        }

        [Fact]
        public void HasOriginAfterSerialization()
        {
            var model = new Model();
            model.Origin = new GeoJSON.Position(10.0, 10.0);
            var json = model.ToJson();
            var newModel = Model.FromJson(json);
            Assert.Equal(model.Origin, newModel.Origin);
        }
  
        private Model QuadPanelModel()
        {
            var model = new Model();
            var a = new Vector3(0,0,0);
            var b = new Vector3(1,0,0);
            var c = new Vector3(1,0,1);
            var d = new Vector3(0,0,1);
            var panel = new Panel(new Polygon(new[]{a,b,c,d}), BuiltInMaterials.Glass);
            model.AddElement(panel);
            return model;
        }
    }
}
