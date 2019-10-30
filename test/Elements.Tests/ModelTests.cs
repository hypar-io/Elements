using System;
using System.IO;
using Xunit;
using Elements.Geometry;
using Elements.Serialization.glTF;
using System.Collections.Generic;

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
            var model = QuadPanelModel();
            model.Origin = new GeoJSON.Position(10.0, 10.0);
            var json = model.ToJson();
            var newModel = Model.FromJson(json);
            Assert.Equal(model.Origin, newModel.Origin);
        }

        [Fact]
        public void SkipsUnknownTypesDuringDeserialization()
        {
            // We've changed an Elements.Beam to Elements.Foo
            var modelStr = "{'Origin':[0.0,0.0],'Elements':{'37f161d6-a892-4588-ad65-457b04b97236':{'discriminator':'Elements.Geometry.Profiles.WideFlangeProfile','d':1.1176,'tw':0.025908,'bf':0.4064,'tf':0.044958,'Perimeter':{'discriminator':'Elements.Geometry.Polygon','Vertices':[{'X':-0.2032,'Y':0.5588,'Z':0.0},{'X':-0.2032,'Y':0.51384199999999991,'Z':0.0},{'X':-0.012954,'Y':0.51384199999999991,'Z':0.0},{'X':-0.012954,'Y':-0.51384199999999991,'Z':0.0},{'X':-0.2032,'Y':-0.51384199999999991,'Z':0.0},{'X':-0.2032,'Y':-0.5588,'Z':0.0},{'X':0.2032,'Y':-0.5588,'Z':0.0},{'X':0.2032,'Y':-0.51384199999999991,'Z':0.0},{'X':0.012954,'Y':-0.51384199999999991,'Z':0.0},{'X':0.012954,'Y':0.51384199999999991,'Z':0.0},{'X':0.2032,'Y':0.51384199999999991,'Z':0.0},{'X':0.2032,'Y':0.5588,'Z':0.0}]},'Voids':null,'Id':'37f161d6-a892-4588-ad65-457b04b97236','Name':'W44x335'},'6b77d69a-204e-40f9-bc1f-ed84683e64c6':{'discriminator':'Elements.Material','Color':{'Red':0.60000002384185791,'Green':0.5,'Blue':0.5,'Alpha':1.0},'SpecularFactor':0.0,'GlossinessFactor':0.0,'Id':'6b77d69a-204e-40f9-bc1f-ed84683e64c6','Name':'steel'},'fd35bd2c-0108-47df-8e6d-42cc43e4eed0':{'discriminator':'Elements.Foo','Curve':{'discriminator':'Elements.Geometry.Arc','Center':{'X':0.0,'Y':0.0,'Z':0.0},'Radius':2.0,'StartAngle':0.0,'EndAngle':90.0},'StartSetback':0.25,'EndSetback':0.25,'Profile':'37f161d6-a892-4588-ad65-457b04b97236','Transform':{'Matrix':{'Components':[1.0,0.0,0.0,0.0,0.0,1.0,0.0,0.0,0.0,0.0,1.0,0.0]}},'Material':'6b77d69a-204e-40f9-bc1f-ed84683e64c6','Representation':{'SolidOperations':[{'discriminator':'Elements.Geometry.Solids.Sweep','Profile':'37f161d6-a892-4588-ad65-457b04b97236','Curve':{'discriminator':'Elements.Geometry.Arc','Center':{'X':0.0,'Y':0.0,'Z':0.0},'Radius':2.0,'StartAngle':0.0,'EndAngle':90.0},'StartSetback':0.25,'EndSetback':0.25,'Rotation':0.0,'IsVoid':false}]},'Id':'fd35bd2c-0108-47df-8e6d-42cc43e4eed0','Name':null}}}";
            var errors = new List<string>();
            var model = Model.FromJson(modelStr, errors);
            Assert.Equal(2, model.Elements.Count);
            Assert.Equal(1, errors.Count);
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
