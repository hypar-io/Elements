# Elements Playground
A playground for testing Elements in the browser.

This is a minimal example using .NET 6, Blazor webassembly, and Elements.

### Running
`dotnet watch run`

### Publishing
`dotnet publish -c release`

### Deploy
The elements web assembly assets are served from S3 via cloud front at https://elements.hypar.io. The deploy script will publish the "app" and copy and deploy all necessary files.
```
./deploy.sh
```
### Building an application with Elements in the browser.
See `index.html` for examples of the following.
- Add a script tag in your `<head>` which loads `elements.js`. `Elements.js` contains the API for elements.
- Add an `<app>` tag which hangs the application in the DOM.
- Add a a script tag which loads `blazor.webassembly.js` in the body, and ensure it is set to `autostart=false`.
- Add a call to `Blazor.start` in a subsequent script to fetch Blazor assets from a preferred location, or from disk.