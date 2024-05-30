# Elements Playground
The elements web assembly API, for using Elements in the browser.

### Running Locally
`./serve.sh` - to serve Elements wasm assets
Edit `wwwroot/index.html` to use localhost:5001, instead of the `https://elements.hypar.io` url.
Serve `wwroot` (e.g. `python3 -m http.server`).

### Publishing
`dotnet publish -c release`

### Deploy
The elements web assembly assets are served from S3 via cloud front at https://elements.hypar.io. The deploy script will publish the "app" and copy and deploy all necessary files.
```
./deploy.sh
```

### Building an application with Elements in the browser.
This repo contains a demo application that demonstrates the usage of the Elements web assembly APIs. The demo application has two input elements, a slider and a button. The button compiles some test code which relies on an input value provided by the slider. 

See the `index.html` in the demo application for an example of how to do the following.
- Add a script tag in your `<head>` which loads `elements.js`. `Elements.js` contains the API for elements.
- Add an `<app>` tag which hangs the application in the DOM.
- Add a a script tag which loads `blazor.webassembly.js` in the body, and ensure it is set to `autostart=false`.
- Add a call to `Blazor.start` in a subsequent script to fetch Blazor assets from a preferred location, or from disk.

**NOTE** The Elements web assembly API does not need to interact with the DOM, but we still need to create a blazor component to inject some of the infrastructure required by the compiler, like the `HttpClient` instance and the `IJSRuntime` instance. You will need to inlcude the `app` element in your application, which puts an empty div in the DOM. We're looking at ways to remove this requirement in the future.

