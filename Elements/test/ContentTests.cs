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
using Elements.Serialization.JSON;

namespace Elements.Tests
{
    public class ContentTests
    {
        private class TestContentElem : ContentElement
        {
            public TestContentElem(string @gltfLocation, BBox3 @bBox, Transform @transform, double scale, Material @material, Representation @representation, bool @isElementDefinition, System.Guid @id, string @name)
                        : base(gltfLocation, bBox, scale, transform, material, representation, isElementDefinition, id, name)
            { }
        }

        [Fact]
        public void CatalogSerialization()
        {
            ContentElement boxType = new ContentElement("../../../models/MergeGlTF/High Back.glb",
                                      new BBox3(new Vector3(-0.5, -0.5, 0), new Vector3(0.5, 0.5, 3)),
                                      1,
                                      new Transform(new Vector3(), Vector3.ZAxis),
                                      BuiltInMaterials.Default,
                                      null,
                                      true,
                                      Guid.NewGuid(),
                                      "BoxyType");
            ContentElement boxType2 = new ContentElement("../../../models/MergeGlTF/Box.glb",
                                      new BBox3(new Vector3(-1, -1, 0), new Vector3(1, 1, 2)),
                                      1,
                                      new Transform(new Vector3(), Vector3.YAxis),
                                      BuiltInMaterials.Default,
                                      null,
                                      true,
                                      Guid.NewGuid(),
                                      "BoxyType");
            var str = boxType2.ToString();
            var testCatalog = new ContentCatalog(new List<ContentElement> { boxType, boxType2 }, Guid.NewGuid(), "test");

            var savePath = "../../../ContentCatalog.json";
            var catalogJson = JsonConvert.SerializeObject(testCatalog, Formatting.Indented, new JsonSerializerSettings()
            {
                Converters = null
            });

            File.WriteAllText(savePath, catalogJson);
        }

        [Fact]
        public void InstanceContentElement()
        {
            var model = new Model();
            var boxType = new TestContentElem("../../../models/MergeGlTF/High Back.glb",
                                      new BBox3(new Vector3(-0.5, -0.5, 0), new Vector3(0.5, 0.5, 3)),
                                      new Transform(new Vector3(), Vector3.ZAxis),
                                      1,
                                      BuiltInMaterials.Default,
                                      null,
                                      true,
                                      Guid.NewGuid(),
                                      "BoxyType");
            var boxType2 = new TestContentElem("../../../models/MergeGlTF/Box.glb",
                                      new BBox3(new Vector3(-1, -1, 0), new Vector3(1, 1, 2)),
                                      new Transform(new Vector3(), Vector3.YAxis),
                                      1,
                                      BuiltInMaterials.Default,
                                      null,
                                      true,
                                      Guid.NewGuid(),
                                      "BoxyType");
            var newBox = boxType.CreateInstance(new Transform(), "first one");
            model.AddElement(newBox);
            // var twoBox = boxType2.CreateInstance(new Transform(new Vector3(5, 0, 0)), "then two");
            // model.AddElement(twoBox);
            // var threeBox = boxType2.CreateInstance(new Transform(new Vector3(15, 0, 0)), "then two");
            // model.AddElement(threeBox);
            var beam = new Beam(new Line(new Vector3(), new Vector3(0, 5, -0.5)), new Circle(new Vector3(), 0.3).ToPolygon());
            model.AddElement(beam);
            model.ToGlTF("../../../ContentInstancing.gltf", false);
            model.ToGlTF("../../../ContentInstancing.glb");
        }
    }
}