class Elements {
    async modelFromJson(json) {
        return DotNet.invokeMethodAsync('Elements.Wasm', 'ModelFromJson', json)
    }

    async modelToGlbBase64(json) {
        return DotNet.invokeMethodAsync('Elements.Wasm', 'ModelToGlbBase64', json)
    }

    async modelToGlbBytes(json) {
        return DotNet.invokeMethodAsync('Elements.Wasm', 'ModelToGlbBytes', json)
    }

    async test(value) {
        return DotNet.invokeMethodAsync('Elements.Wasm', 'Test', value)
    }
}

window.elements = new Elements();