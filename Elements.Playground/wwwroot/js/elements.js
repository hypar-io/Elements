// Note: If updating this file to expose new APIs, please also update the adjacent type definition file at elements.d.ts.
class Elements {
    testCode = `
    var model = new Model(); 
    // This class can be modified to suit your needs. It
    // should be a model of the JSON you plan to send through updateInputs.
    class InputClass {
        public double? Height {get; set;}
    }
    var input = JsonSerializer.Deserialize<InputClass>(InputJson);
    var mass = new Mass(Polygon.Rectangle(1,1), input.Height ?? 5, BuiltInMaterials.Wood); 
    Console.WriteLine($"The volume of the mass is {mass.Volume()}."); 
    model.AddElement(mass); 
    return model;`

    async compile (code) {
        return DotNet.invokeMethodAsync('Elements.Playground', 'Compile', code)
    }

    async run () {
        return DotNet.invokeMethodAsync('Elements.Playground', 'Run');
    }

    async updateInputs (inputs) {
        return DotNet.invokeMethod('Elements.Playground', 'UpdateInputs', inputs);
    }

    loadModel (bytes) {
        // console.debug(bytes);
        return true;
    }
}

window.elements = new Elements();