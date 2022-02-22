using System.Linq;
using System.Threading.Tasks;
using Elements;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Elements.Spatial;
using Elements.Validators;
using Microsoft.JSInterop;
using System.Diagnostics;

public static class ElementsAPI
{
    [JSInvokable]
    public static Task<Model> ModelFromJson(string json)
    {
        return Task.FromResult(Model.FromJsonNew(json));
    }

    [JSInvokable]
    public static Task<string> ModelToGlbBase64(string json)
    {
        Validator.DisableValidationOnConstruction = true;

        var model = Model.FromJsonNew(json);
        return Task.FromResult(model.ToBase64String());
    }

    [JSInvokable]
    public static Task<byte[]> ModelToGlbBytes(string json)
    {
        Validator.DisableValidationOnConstruction = true;

        var sw = new Stopwatch();
        sw.Start();
        var model = Model.FromJsonNew(json);
        Console.WriteLine($"{sw.ElapsedMilliseconds}ms for creating the model from json.");
        sw.Restart();
        var result = Task.FromResult(model.ToGlTF());
        Console.WriteLine($"{sw.ElapsedMilliseconds}ms for creating the glb from json.");
        return result;
    }
}
