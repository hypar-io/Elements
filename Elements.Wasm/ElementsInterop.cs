using Microsoft.JSInterop;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Elements.Validators;
using System.Diagnostics;
using Elements.Geometry.Profiles;

namespace Elements.Wasm
{
    public static class ElementsInterop
    {
        [JSInvokable]
        public static void Test()
        {
            Validator.DisableValidationOnConstruction = true;

            var sw = new Stopwatch();
            sw.Start();
            var model = new Model();

            var total = 10;
            for(var x=0; x<10; x++)
            {
                for(var y=0; y<10; y++)
                {
                    for(var z=0;z<10; z++)
                    {
                        var profile = new Profile(Polygon.Rectangle(0.01 + x/total, 0.01 + y/total));
                        var mass = new Mass(profile, 0.01 + z/total);
                        model.AddElement(mass);
                    }
                }
            }
            sw.Stop();
            Console.WriteLine($"{sw.Elapsed} for creating all elements.");
            sw.Reset();

            sw.Start();
            var json = model.ToJson();
            sw.Stop();
            Console.WriteLine($"{sw.Elapsed} for serializing to JSON.");
            sw.Reset();

            sw.Start();
            var newModel = Model.FromJson(json);
            sw.Stop();
            Console.WriteLine($"{sw.Elapsed} for deserializing JSON to Model.");
            sw.Reset();
            
            sw.Start();
            newModel.ToGlTF(false, false);
            sw.Stop();
            Console.WriteLine($"{sw.Elapsed} for writing to gltf.");
        }

        [JSInvokable]
        public static Task<Byte[]> ModelToBytes(string modelJson)
        {
            Validator.DisableValidationOnConstruction = true;

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
