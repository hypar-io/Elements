class Elements {
    compilerReference = null;
    testCode = 'var model = new Model(); var mass = new Mass(Polygon.Rectangle(1,1), Inputs.ContainsKey("height") ? Inputs["height"]: 5, BuiltInMaterials.Wood); Console.WriteLine($"The volume of the mass is {mass.Volume()}."); model.AddElement(mass); return model;';

    async compile(code) {
        return DotNet.invokeMethodAsync('Elements.Playground', 'Compile', code)
    }

    async run() {
        return DotNet.invokeMethodAsync('Elements.Playground', 'Run');
    }

    async updateInputs(inputs) {
        return DotNet.invokeMethod('Elements.Playground', 'UpdateInputs', inputs);
    }

    loadModel(bytes) {
        // console.debug(bytes);
        return true;
    }
}

window.elements = new Elements();