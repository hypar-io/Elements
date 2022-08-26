class Elements {
    compilerReference = null;
    test = 'var model = new Model(); var mass = new Mass(Polygon.Rectangle(1,1), 1, BuiltInMaterials.Wood); Console.WriteLine(mass.Volume()); model.AddElement(mass); return model;';

    async compile(code) {
        return DotNet.invokeMethodAsync('Elements.Playground', 'Compile', code)
    }

    async compileAndRun(code) {
        return DotNet.invokeMethodAsync('Elements.Playground', 'CompileAndRun', code)
    }

    async run() {
        return DotNet.invokeMethodAsync('Elements.Playground', 'Run')
    }

    loadModel(bytes) {
        console.debug(bytes)
        return true;
    }
}

window.elements = new Elements();