using System;
using System.IO;
using Xunit;
using Elements.Geometry;
using Elements.Serialization.glTF;
using System.Collections.Generic;
using Elements.Generate;
using Newtonsoft.Json;
using Elements.Geometry.Solids;
using System.Linq;
using Xunit.Abstractions;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Elements.Tests
{
    public class TestMapping : MappingBase
    {
        public string MapProp = "test";
    }

    class ColumnSharedObject : SharedObject
    {
        public string Name { get; set; }
        public Material Material { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> AdditionalProperties { get; set; } = new Dictionary<string, object>();
    }

    public class ModelTests
    {
        private ITestOutputHelper _output;

        public ModelTests(ITestOutputHelper output)
        {
            this._output = output;
        }

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
            model.Transform = new Transform(new Vector3(10.0, 10.0));
            var json = model.ToJson();
            var newModel = Model.FromJson(json);
            Assert.Equal(model.Transform, newModel.Transform);
        }

        [Fact]
        public void UnknownTypesConvertToBaseElementDuringDeserialization()
        {
            // We've changed an Elements.Beam to Elements.Foo
            var modelStr = "{'Transform':{'Matrix':{'Components':[1.0,0.0,0.0,0.0,0.0,1.0,0.0,0.0,0.0,0.0,1.0,0.0]}},'Elements':{'c6d1dc68-f800-47c1-9190-745b525ad569':{'discriminator':'Elements.Baz'}, '37f161d6-a892-4588-ad65-457b04b97236':{'discriminator':'Elements.Geometry.Profiles.WideFlangeProfile','d':1.1176,'tw':0.025908,'bf':0.4064,'tf':0.044958,'Perimeter':{'discriminator':'Elements.Geometry.Polygon','Vertices':[{'X':-0.2032,'Y':0.5588,'Z':0.0},{'X':-0.2032,'Y':0.51384199999999991,'Z':0.0},{'X':-0.012954,'Y':0.51384199999999991,'Z':0.0},{'X':-0.012954,'Y':-0.51384199999999991,'Z':0.0},{'X':-0.2032,'Y':-0.51384199999999991,'Z':0.0},{'X':-0.2032,'Y':-0.5588,'Z':0.0},{'X':0.2032,'Y':-0.5588,'Z':0.0},{'X':0.2032,'Y':-0.51384199999999991,'Z':0.0},{'X':0.012954,'Y':-0.51384199999999991,'Z':0.0},{'X':0.012954,'Y':0.51384199999999991,'Z':0.0},{'X':0.2032,'Y':0.51384199999999991,'Z':0.0},{'X':0.2032,'Y':0.5588,'Z':0.0}]},'Voids':null,'Id':'37f161d6-a892-4588-ad65-457b04b97236','Name':'W44x335'},'6b77d69a-204e-40f9-bc1f-ed84683e64c6':{'discriminator':'Elements.Material','Color':{'Red':0.60000002384185791,'Green':0.5,'Blue':0.5,'Alpha':1.0},'SpecularFactor':0.0,'GlossinessFactor':0.0,'Id':'6b77d69a-204e-40f9-bc1f-ed84683e64c6','Name':'steel'},'fd35bd2c-0108-47df-8e6d-42cc43e4eed0':{'discriminator':'Elements.Foo','Curve':{'discriminator':'Elements.Geometry.Arc','Center':{'X':0.0,'Y':0.0,'Z':0.0},'Radius':2.0,'StartAngle':0.0,'EndAngle':90.0},'StartSetback':0.25,'EndSetback':0.25,'Profile':'37f161d6-a892-4588-ad65-457b04b97236','Transform':{'Matrix':{'Components':[1.0,0.0,0.0,0.0,0.0,1.0,0.0,0.0,0.0,0.0,1.0,0.0]}},'Material':'6b77d69a-204e-40f9-bc1f-ed84683e64c6','Representation':{'SolidOperations':[{'discriminator':'Elements.Geometry.Solids.Sweep','Profile':'37f161d6-a892-4588-ad65-457b04b97236','Curve':{'discriminator':'Elements.Geometry.Arc','Center':{'X':0.0,'Y':0.0,'Z':0.0},'Radius':2.0,'StartAngle':0.0,'EndAngle':90.0},'StartSetback':0.25,'EndSetback':0.25,'Rotation':0.0,'IsVoid':false}]},'Id':'fd35bd2c-0108-47df-8e6d-42cc43e4eed0','Name':null}}}";
            var model = Model.FromJson(modelStr, out var errors);
            foreach (var e in errors)
            {
                this._output.WriteLine(e);
            }

            // We expect three geometric elements,
            // and one base element (Elements.Baz)
            Assert.Equal(4, model.Elements.Count);
            bool containsBaz = model.Elements.Select(x => x.Value.AdditionalProperties["discriminator"]).Contains("Elements.Baz");
            Assert.True(containsBaz);
            Assert.Single(errors);
        }

        /// <summary>
        /// Test whether two models, containing user defined types, can be
        /// deserialized and merged into one model.
        /// </summary>
        [Fact(Skip = "ModelMerging")]
        public async Task MergesModelsWithUserDefinedTypes()
        {
            var schemas = new[]{
                "../../../models/Merge/Envelope.json",
                "../../../models/Merge/FacadePanel.json",
                "../../../models/Merge/Level.json"
            };

            var asm = await TypeGenerator.GenerateInMemoryAssemblyFromUrisAndLoadAsync(schemas);
            var facadePanelType = asm.Assembly.GetType("Elements.FacadePanel");
            Assert.NotNull(facadePanelType);
            var envelopeType = asm.Assembly.GetType("Elements.Envelope");
            Assert.NotNull(envelopeType);
            var model1 = JsonConvert.DeserializeObject<Model>(File.ReadAllText("../../../models/Merge/facade.json"));
            var count1 = model1.Elements.Count;

            var model2 = JsonConvert.DeserializeObject<Model>(File.ReadAllText("../../../models/Merge/structure.json"));
            var count2 = model2.Elements.Count;

            var merge = new Model();
            merge.AddElements(model1.Elements.Values);
            merge.AddElements(model2.Elements.Values);
            merge.ToGlTF("models/Merge.glb");
        }

        [Fact]
        public void ElementWithDeeplyNestedElementSerializesCorrectly()
        {
            var p = new Profile(Polygon.Rectangle(1, 1));
            // Create a mass overiding its representation.
            // This will introduce a profile into the serialization for the
            // representation. This should be serialized correctly.
            var mass1 = new Mass(p,
                                1,
                                BuiltInMaterials.Mass,
                                new Transform(),
                                new Representation(new List<SolidOperation> { new Extrude(p, 2, Vector3.ZAxis, false) }));
            // A second mass that uses a separate embedded profile.
            // This is really a mistake because the user wants the profile
            // that they supply in the constructor to be used, but the profile
            // supplied in the representation will override it.
            var mass2 = new Mass(p,
                                1,
                                BuiltInMaterials.Mass,
                                new Transform(),
                                new Representation(new List<SolidOperation> { new Extrude(new Profile(Polygon.Rectangle(1, 1)), 2, Vector3.ZAxis, false) }));
            var model = new Model();
            model.AddElement(mass1);
            model.AddElement(mass2);

            Assert.True(model.AllElementsOfType<Profile>().Count() == 2);
            Assert.True(model.AllElementsOfType<Mass>().Count() == 2);
            Assert.Single<Material>(model.AllElementsOfType<Material>());

            var json = model.ToJson();
            File.WriteAllText("./deepSerialize.json", json);

            var newModel = Model.FromJson(json);
            Assert.True(newModel.AllElementsOfType<Profile>().Count() == 2);
            Assert.True(newModel.AllElementsOfType<Mass>().Count() == 2);
            Assert.Single<Material>(newModel.AllElementsOfType<Material>());
        }

        [Fact]
        public void SaveMappingDictionaryWithoutRepeatingMappings()
        {
            var cube = new Mass(Polygon.Rectangle(1, 1), 1, BuiltInMaterials.Mass, isElementDefinition: true);
            var mapping = new TestMapping();
            var inst1 = cube.CreateInstance(new Transform(0, 0, 0), "inst 1");
            inst1.SetMapping("Revit", mapping);
            var inst2 = cube.CreateInstance(new Transform(0, 0, 0), "inst 2");
            inst2.SetMapping("Revit", mapping);

            var model = new Model(new List<Element> { inst1, inst2 });
            var json = model.ToJson(true);
            var newModel = Model.FromJson(json, out var errors);
            Assert.Empty(errors);

            //Check that only a single mapping was serialized.
            var m = System.Text.RegularExpressions.Regex.Matches(json, @"""discriminator"":\s*""Elements.Tests.TestMapping""");
            Assert.Single(m);
            Assert.Single(newModel.AllElementsOfType<TestMapping>());

            // confirm that we only serialize the Mapping Property if it exists
            m = System.Text.RegularExpressions.Regex.Matches(json, @"""Mappings"": null");
            Assert.Empty(m);
        }

        [Fact]
        public void CoreElementTransformsAreIdempotentDuringSerialization()
        {
            var model = new Model();
            // Create a floor with an elevation.
            // This will mutate the element's transform.
            var floor = new Floor(Polygon.L(20, 20, 5), 0.1, new Transform(0, 0, 0.5));
            model.AddElement(floor);

            var beam = new Beam(new Line(Vector3.Origin, new Vector3(5, 5, 5)), Polygon.Rectangle(0.1, 0.1));
            model.AddElement(beam);

            var wall = new StandardWall(new Line(Vector3.Origin, new Vector3(20, 0, 0)), 0.2, 3.5);
            model.AddElement(wall);

            // Serialize the floor, recording the mutated transform.
            var json = model.ToJson();

            // Deserialize the floor, which will deserialize the elevation
            // and add it to the transform.
            var newModel = Model.FromJson(json);

            var newFloor = newModel.AllElementsOfType<Floor>().First();
            Assert.Equal(floor.Transform, newFloor.Transform);

            var newBeam = newModel.AllElementsOfType<Beam>().First();
            Assert.Equal(beam.Transform, newBeam.Transform);

            var newWall = newModel.AllElementsOfType<StandardWall>().First();
            Assert.Equal(wall.Transform, newWall.Transform);
        }


        [Fact]
        public void SerializationKeepsDiscriminatorOnFallbackBaseElementTypes()
        {
            // Reading in a model with a SpaceBoundary input
            var json = File.ReadAllText("../../../models/Elements/spaceinputs.json");
            var model = Model.FromJson(json);

            // Since SpaceBoundary isn't in Elements, it will be deserialized as a `GeometricElement`
            Assert.True(model.Elements.First().Value.GetType() == typeof(GeometricElement));

            var newModel = model.ToJson();

            // When we serialize again, we should still have the discriminator of `Elements.SpaceBoundary`
            Assert.Contains("Elements.SpaceBoundary", newModel);
        }

        [Fact]
        public void DeserializationHandlesZeroThicknessWalls()
        {
            var json = File.ReadAllText("../../../models/Elements/zeroThicknessWalls.json");
            var model = Model.FromJson(json);
            var walls = model.AllElementsOfType<StandardWall>();
            Assert.True(walls.Any((w) => w.Thickness == 0));
        }

        [Fact]
        public void DeserializationSkipsUnknownProperties()
        {
            var column = new Column(Vector3.Origin, 5, null, new Profile(Polygon.Rectangle(1, 1)));
            var model = new Model();
            model.AddElement(column);
            var json = model.ToJson(true);
            // https://www.newtonsoft.com/json/help/html/ModifyJson.htm
            var obj = JObject.Parse(json);
            var elements = obj["Elements"];
            var c = (JObject)elements.Values().ElementAt(2);

            // Inject an unknown property.
            c.Property("Curve").AddAfterSelf(new JProperty("Foo", "Bar"));
            var newModel = Model.FromJson(obj.ToString());
            Assert.Single(newModel.AllElementsOfType<Column>());
            var newColumn = newModel.AllElementsOfType<Column>().First();
            Assert.Equal(column.Curve, newColumn.Curve);
            Assert.Equal(column.Profile, newColumn.Profile);
        }

        [Fact]
        public void DeserializationConstructsWithMissingProperties()
        {
            var column = new Column(new Vector3(5, 0), 5, null, new Profile(Polygon.Rectangle(1, 1)));
            var model = new Model();
            model.AddElement(column);
            var json = model.ToJson(true);
            // https://www.newtonsoft.com/json/help/html/ModifyJson.htm
            var obj = JObject.Parse(json);
            var elements = obj["Elements"];
            var c = (JObject)elements.Values().ElementAt(2); // the column

            // Remove the Location property
            c.Property("Location").Remove();
            var newModel = Model.FromJson(obj.ToString(), out var errors);
            var newColumn = newModel.AllElementsOfType<Column>().First();
            Assert.Equal(Vector3.Origin, newColumn.Location);
        }

        [Fact]
        public void DeserializationSkipsNullProperties()
        {
            var material = BuiltInMaterials.Mass;
            var model = new Model();
            model.AddElement(material);
            var json = model.ToJson(true);
            // https://www.newtonsoft.com/json/help/html/ModifyJson.htm
            var obj = JObject.Parse(json);
            var elements = obj["Elements"];
            var c = (JObject)elements.Values().ElementAt(0); // the material

            // Nullify a property.
            c.Property("Color").Value = null;
            var newModel = Model.FromJson(obj.ToString(), out var errors);
            foreach (var e in errors)
            {
                this._output.WriteLine(e);
            }
            Assert.Empty(newModel.AllElementsOfType<Material>());
        }

        [Fact]
        public void DeserializesToGeometricElementsWhenTypeIsUnknownAndRepresentationExists()
        {
            var profile = new Profile(Polygon.Rectangle(1, 1));
            var red = new Material("Red", Colors.Red);
            var green = new Material("Green", Colors.Green);
            var column = new Column(new Vector3(5, 0), 5, null, profile, material: red);
            var beam = new Beam(new Line(Vector3.Origin, new Vector3(5, 5, 5)), profile, material: green);
            var model = new Model();
            model.AddElements(beam, column);
            var json = model.ToJson(true);

            // We want to test that unknown element types will still deserialize
            // to geometric elements.
            json = json.Replace("Elements.Beam", "Foo");
            json = json.Replace("Elements.Column", "Bar");

            var newModel = Model.FromJson(json);
            Assert.Equal(2, newModel.AllElementsOfType<Material>().Count());
            Assert.Equal(2, newModel.AllElementsOfType<GeometricElement>().Count());
            var modelPath = $"models/geometric_elements.glb";
            newModel.ToGlTF(modelPath, true);
        }

        [Fact]
        public void SubElementIsAddedToModel()
        {
            var model = new Model();
            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            var ue = new TestUserElement(line, new Profile(Polygon.L(1, 2, 0.5)));
            model.AddElement(ue);

            // The profile of the user element and the one
            // created inside UpdateRepresentation.
            Assert.Equal(2, model.AllElementsOfType<Profile>().Count());
        }

        [Fact]
        public void SubListElementIsAddedToModel()
        {
            var model = new Model();
            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            var ue = new TestUserElement(line, new Profile(Polygon.L(1, 2, 0.5)));
            ue.SubElements.AddRange(new[]{
                new Mass(Polygon.Rectangle(1,1)),
                new Mass(Polygon.L(2,2,1))});
            model.AddElement(ue);

            // The profiles from the user element, the two sub elements
            // and one profile generated in UpdateRepresentations.
            Assert.Equal(4, model.AllElementsOfType<Profile>().Count());
        }

        [Fact]
        public void SubDictionaryOfElementIsAddedToModel()
        {
            var model = new Model();
            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            var ue = new TestUserElement(line, new Profile(Polygon.L(1, 2, 0.5)));
            ue.DictionaryElements["foo"] = BuiltInMaterials.XAxis;
            ue.DictionaryElements["bar"] = BuiltInMaterials.YAxis;
            model.AddElement(ue);
            Assert.Equal(3, model.AllElementsOfType<Material>().Count());
        }

        [Fact]
        public void ProfilesInRepresentationsAreAddedToModel()
        {
            var model = new Model();
            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            var ue = new TestUserElement(line, new Profile(Polygon.L(1, 2, 0.5)));
            ue.UpdateRepresentations();
            model.AddElement(ue);
            Assert.Equal(2, model.AllElementsOfType<Profile>().Count());
        }

        [Fact]
        public void HandlesAllDocumentedSubElements()
        {
            // List types should be valid.
            Assert.True(Model.IsValidForRecursiveAddition(typeof(Element[])));
            Assert.True(Model.IsValidForRecursiveAddition(typeof(Floor[])));
            Assert.True(Model.IsValidForRecursiveAddition(typeof(IList<Floor>)));
            Assert.True(Model.IsValidForRecursiveAddition(typeof(List<Floor>)));

            // Dictionary types should be valid.
            Assert.True(Model.IsValidForRecursiveAddition(typeof(IDictionary<string, Floor>)));
            Assert.True(Model.IsValidForRecursiveAddition(typeof(Dictionary<Guid, Floor>)));

            // List types of solid operations should be valid.
            Assert.True(Model.IsValidForRecursiveAddition(typeof(IList<SolidOperation>)));
            Assert.True(Model.IsValidForRecursiveAddition(typeof(List<SolidOperation>)));
            Assert.True(Model.IsValidForRecursiveAddition(typeof(Element)));

            // Representations should be valid.
            Assert.True(Model.IsValidForRecursiveAddition(typeof(Representation)));

            // Nullable<T> should work without exploding.
            Assert.False(Model.IsValidForRecursiveAddition(typeof(Guid?)));

            // Stuff that shouldn't work
            Assert.False(Model.IsValidForRecursiveAddition(typeof(List<double>)));
            Assert.False(Model.IsValidForRecursiveAddition(typeof(Dictionary<Guid, double>)));
            Assert.False(Model.IsValidForRecursiveAddition(typeof(double)));
            Assert.False(Model.IsValidForRecursiveAddition(typeof(string)));
            Assert.False(Model.IsValidForRecursiveAddition(typeof(object)));
        }

        [Fact]
        public void AllElementsAssignableFromType()
        {
            var column = new Column(new Vector3(5, 5, 5), 2.0, null, Polygon.Rectangle(1, 1));
            var beam = new Beam(new Line(Vector3.Origin, new Vector3(5, 5, 5)), Polygon.Rectangle(1, 1));
            var brace = new Brace(new Line(Vector3.Origin, new Vector3(5, 5, 5)), Polygon.Rectangle(1, 1));
            var model = new Model();
            model.AddElements(column, beam, brace);
            Assert.Equal(3, model.AllElementsAssignableFromType<StructuralFraming>().Count());
        }

        private Model QuadPanelModel()
        {
            var model = new Model();
            var a = new Vector3(0, 0, 0);
            var b = new Vector3(1, 0, 0);
            var c = new Vector3(1, 0, 1);
            var d = new Vector3(0, 0, 1);
            var panel = new Panel(new Polygon(new[] { a, b, c, d }), BuiltInMaterials.Glass);
            model.AddElement(panel);
            return model;
        }

        [Fact]
        public void SharedObjectsAreSerialized()
        {
            var model = new Model();

            var column1 = new Column(new Vector3(5, 5, 5), 2.0, null, Polygon.Rectangle(1, 1));
            string commonPropertyName = "CommonProperty";
            string commonPropertyValue = "CommonValue";
            column1.AdditionalProperties.Add(commonPropertyName, commonPropertyValue);

            var column2 = new Column(new Vector3(5, 5, 5), 2.0, null, Polygon.Rectangle(1, 1));
            column2.AdditionalProperties.Add(commonPropertyName, commonPropertyValue);

            var column1SharedObject = new ColumnSharedObject
            {
                AdditionalProperties = new Dictionary<string, object>()
                {
                    { commonPropertyName, commonPropertyValue },
                },
            };

            string sharedObjectId = column1SharedObject.Id.ToString();
            column1.SharedObject = column1SharedObject;
            column2.SharedObject = column1SharedObject;
            model.AddElement(column1);
            model.AddElement(column2);

            var json = model.ToJson();
            // one shared object is serialized
            var savedSharedObjects = System.Text.RegularExpressions.Regex.Matches(json, @"""discriminator"":\s*""Elements.Tests.ColumnSharedObject""");
            Assert.Single(savedSharedObjects);

            // shared object is added as Id to elements
            var sharedObjectUsage = System.Text.RegularExpressions.Regex.Matches(json, $@"""SharedObject"":\s*""{sharedObjectId}""");
            Assert.Equal(2, sharedObjectUsage.Count);
        }

        [Fact]
        public void ElementPropertiesEqualToSharedObjectPropertiesNotSerialized()
        {
            var model = new Model();
            var red = new Material("Red", Colors.Red);
            string columnName = "Column Test";

            var column1 = new Column(new Vector3(5, 5, 5), 2.0, null, Polygon.Rectangle(1, 1));
            column1.Material = red;
            string commonPropertyName = "CommonProperty";
            string commonPropertyValue = "CommonValue";
            column1.AdditionalProperties.Add(commonPropertyName, commonPropertyValue);
            column1.Name = columnName;

            var column2 = new Column(new Vector3(5, 5, 5), 2.0, null, Polygon.Rectangle(1, 1));
            column2.Material = red;
            column2.AdditionalProperties.Add(commonPropertyName, commonPropertyValue);
            column2.Name = columnName;

            var column1SharedObject = new ColumnSharedObject
            {
                AdditionalProperties = new Dictionary<string, object>()
                {
                    { commonPropertyName, commonPropertyValue },
                },
                Name = columnName,
                Material = red
            };

            column1.SharedObject = column1SharedObject;
            column2.SharedObject = column1SharedObject;
            model.AddElement(column1);
            model.AddElement(column2);
            model.AddElement(red);

            string materialId = red.Id.ToString();

            var json = model.ToJson();
            // Material was saved to shared object
            var sharedMaterial = System.Text.RegularExpressions.Regex.Matches(json, $@"""Material"":\s*""{materialId}""");
            Assert.Single(sharedMaterial);
            // material was deleted from elements
            sharedMaterial = System.Text.RegularExpressions.Regex.Matches(json, $@"""Material"":\s*");
            Assert.Single(sharedMaterial);

            // common property from AdditionalProperties was saved to shared object once
            var commonProperty = System.Text.RegularExpressions.Regex.Matches(json, $@"""{commonPropertyName}"":\s*""{commonPropertyValue}""");
            Assert.Single(commonProperty);
            // common property was deleted from elements
            commonProperty = System.Text.RegularExpressions.Regex.Matches(json, $@"""{commonPropertyName}"":\s*");
            Assert.Single(commonProperty);

            // name property was saved to shared object and deleted from elements
            var nameProperty = System.Text.RegularExpressions.Regex.Matches(json, $@"""Name"":\s*""{columnName}""");
            Assert.Single(nameProperty);
        }

        [Fact]
        public void ElementPropertiesNotEqualToSharedObjectPropertiesSerializedToELements()
        {
            var model = new Model();
            var red = new Material("Red", Colors.Red);
            string columnName = "Column Test";

            var column1 = new Column(new Vector3(5, 5, 5), 2.0, null, Polygon.Rectangle(1, 1));
            column1.Material = red;
            string commonPropertyName = "CommonProperty";
            string commonPropertyValue = "CommonValue";
            column1.AdditionalProperties.Add(commonPropertyName, commonPropertyValue);
            column1.AdditionalProperties.Add("UniqueProperty", "UniqueValue");
            column1.Name = columnName;

            var column2 = new Column(new Vector3(5, 5, 5), 2.0, null, Polygon.Rectangle(1, 1));
            column2.Material = red;
            column2.AdditionalProperties.Add(commonPropertyName, commonPropertyValue);
            column2.AdditionalProperties.Add("UniqueProperty", "UniqueValue");
            column2.Name = columnName;

            var column3 = new Column(new Vector3(5, 5, 5), 2.0, null, Polygon.Rectangle(1, 1));
            column3.Material = red;
            column3.AdditionalProperties.Add(commonPropertyName, "DifferentValue");
            column3.Name = columnName;

            var column1SharedObject = new ColumnSharedObject
            {
                AdditionalProperties = new Dictionary<string, object>()
                {
                    { commonPropertyName, commonPropertyValue },
                },
                Name = columnName,
                Material = red
            };

            column1.SharedObject = column1SharedObject;
            column2.SharedObject = column1SharedObject;
            model.AddElement(column1);
            model.AddElement(column2);
            model.AddElement(column3);
            model.AddElement(red);

            string materialId = red.Id.ToString();

            var json = model.ToJson();
            // Values not from shared object are serialized to both elements
            var uniqueProperties = System.Text.RegularExpressions.Regex.Matches(json, $@"""UniqueProperty"":\s*""UniqueValue""");
            Assert.Equal(2, uniqueProperties.Count);

            // common property from AdditionalProperties was saved to shared object and deleted from elements
            var commonProperty = System.Text.RegularExpressions.Regex.Matches(json, $@"""{commonPropertyName}"":\s*");
            Assert.Equal(2, commonProperty.Count);
        }
    }
}
