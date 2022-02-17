using System.Linq;
using System.Threading.Tasks;
using Elements;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Elements.Spatial;
using Microsoft.JSInterop;
using System.Diagnostics;

public static class ElementsAPI
{
    [JSInvokable]
    public static Task<Model> ModelFromJson(string json)
    {
        return Task.FromResult(Model.FromJson(json, out _, false));
    }

    [JSInvokable]
    public static Task<string> ModelToGlbBase64(string json)
    {
        var model = Model.FromJson(json, out _, false);
        return Task.FromResult(model.ToBase64String());
    }

    [JSInvokable]
    public static Task<byte[]> ModelToGlbBytes(string json)
    {
        var sw = new Stopwatch();
        sw.Start();
        var model = Model.FromJson(json, out _, false);
        Console.WriteLine($"{sw.ElapsedMilliseconds}ms for creating the model from json.");
        sw.Restart();
        var result = Task.FromResult(model.ToGlTF());
        Console.WriteLine($"{sw.ElapsedMilliseconds}ms for creating the glb from json.");
        return result;
    }
}
