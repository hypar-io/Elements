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

### Files
`Elements.js`
- The javascript elements API. Elements is made available on the window object.

`ElementsAPI.cs`
- A wrapper around .net elements that is called by the javascript API.