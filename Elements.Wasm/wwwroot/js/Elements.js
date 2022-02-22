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

    async test() {
        return DotNet.invokeMethodAsync('Elements.Wasm', 'Test')
    }
}

window.elements = new Elements();