using System.Linq;
using System.Threading.Tasks;
using Elements;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Elements.Spatial;
using Elements.Validators;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Text;

public static class ElementsAPI
{
    [JSInvokable]
    public static Task<Model> ModelFromJson(string json)
    {
        return Task.FromResult(Model.FromJson(json));
    }

    [JSInvokable]
    public static Task<string> ModelToGlbBase64(string json)
    {
        Validator.DisableValidationOnConstruction = true;

        var model = Model.GeometricElementModelFromJson(json);
        return Task.FromResult(model.ToBase64String());
    }

    [JSInvokable]
    public static Task<byte[]> ModelToGlbBytes(string json)
    {
        Validator.DisableValidationOnConstruction = true;

        var sw = new Stopwatch();
        sw.Start();
        var model = Model.GeometricElementModelFromJson(json);
        Debug.WriteLine($"{sw.ElapsedMilliseconds}ms for creating the model from json.");
        sw.Restart();
        var result = Task.FromResult(model.ToGlTF());
        Debug.WriteLine($"{sw.ElapsedMilliseconds}ms for creating the glb from json.");
        return result;
    }

    [JSInvokable]
    public static Task<TestResult> Test(int value)
    {
        Validator.DisableValidationOnConstruction = true;

        var sw = new Stopwatch();
        sw.Start();
        var sb = new StringBuilder();

        var model = new Model();
        var r = new Random();
        var size = 10;
        var profile = new Profile(Polygon.L(0.1, 0.1, 0.05));
        for (var i = 0; i < value; i++)
        {
            var start = new Vector3(r.NextDouble() * size, r.NextDouble() * size, r.NextDouble() * size);
            var end = new Vector3(r.NextDouble() * size, r.NextDouble() * size, r.NextDouble() * size);
            var line = new Line(start, end);
            // var c = new Color(r.NextDouble(), r.NextDouble(), r.NextDouble(), 1.0);
            // var m = new Material(Guid.NewGuid().ToString(), c);
            var beam = new Beam(line, profile, null, BuiltInMaterials.Steel);
            model.AddElement(beam);
        }
        sb.AppendLine($"{sw.ElapsedMilliseconds}ms for creating test beams.");
        sw.Restart();

        var json = model.ToJson();
        sb.AppendLine($"{sw.ElapsedMilliseconds}ms for serializing model.");
        sw.Restart();

        // var baseModel = Model.FromJson(json);
        // sb.AppendLine($"{sw.ElapsedMilliseconds}ms for deserializing model using base deserializer.");
        // sw.Restart();

        var newModel = Model.GeometricElementModelFromJson(json);
        sb.AppendLine($"{sw.ElapsedMilliseconds}ms for deserializing model using geometric elements");
        sw.Restart();

        var result = model.ToGlTF();
        sb.AppendLine($"{sw.ElapsedMilliseconds}ms for creating the glb.");
        return Task.FromResult<TestResult>(new TestResult()
        {
            Glb = result,
            Results = sb.ToString()
        });
    }

    public class TestResult
    {
        public byte[]? Glb { get; set; }
        public string? Results { get; set; }
    }
}
