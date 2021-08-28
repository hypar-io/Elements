using Microsoft.JSInterop;
using Elements;
using Elements.Geometry;
using Elements.Validators;
using Elements.Serialization.glTF;
using System.Diagnostics;

namespace Elements.Wasm
{
    public static class ElementsInterop
    {
        [JSInvokable]
        public static void Test()
        {
            Validator.DisableValidationOnConstruction = true;
            Console.WriteLine("Here you are!");
            var model = new Model();
            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            var beam = new Beam(line, Polygon.Rectangle(0.1, 0.1));
            model.AddElement(beam);
            Console.WriteLine($"Wrote model with {model.Elements.Count()} elements.");
        }

        [JSInvokable]
        public static Task<Byte[]> ModelToBytes(string modelJson)
        {
            var sw = new Stopwatch();
            sw.Start();
            var model = Model.FromJson(modelJson, out List<string> errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            sw.Stop();
            Console.WriteLine($"{sw.Elapsed} for loading model with {model.Elements.Count()} elements.");
            sw.Reset();
            sw.Start();
            var glb = model.ToGlTF();
            sw.Stop();
            Console.WriteLine($"{sw.Elapsed} for creating glb.");

            return Task.FromResult<Byte[]>(glb);
        }
    }
}
