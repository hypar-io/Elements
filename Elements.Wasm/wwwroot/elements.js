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

// We don't autostart blazor so that we can capture
// the load paths and redirect them to the correct location.
// https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/startup?view=aspnetcore-6.0
Blazor.start({
    loadBootResource: function (type, name, defaultUri, integrity) {
        // console.log(`Loading: '${type}', '${name}', '${defaultUri}', '${integrity}'`);
        switch (type) {
            case 'manifest':
            case 'assembly':
            case 'globalization':
            case 'dotnetjs':
            case 'dotnetwasm':
            case 'timezonedata':
                return `elements/_framework/${name}`;
        }
    }
});

window.elements = new Elements();