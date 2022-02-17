using System.Linq;
using System.Threading.Tasks;
using Elements;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Elements.Spatial;
using Microsoft.JSInterop;

public static class ElementsAPI
{
    [JSInvokable]
    public static Model ModelFromJson(string json)
    {
        return Model.FromJson(json, out _, false);
    }

    [JSInvokable]
    public static string ModelToGlb(string json)
    {
        var model = Model.FromJson(json, out _, false);
        return model.ToBase64String();
    }
}
