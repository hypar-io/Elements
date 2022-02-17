class Elements {
    async modelFromJson(json) {
        return DotNet.invokeMethodAsync('Elements.Wasm', 'ModelFromJson', json)
    }

    async modelToGlb(json) {
        return DotNet.invokeMethodAsync('Elements.Wasm', 'ModelToGlb', json)
    }
}

window.elements = new Elements();