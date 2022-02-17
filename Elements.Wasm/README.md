# Elements.Wasm
This is a minimal web assembly project.

### Run
`dotnet watch run`

### Publish
`dotnet publish -c release`

### Files
`Elements.js`
- The javascript elements API. Elements is made available on the window object.

`ElementsAPI.cs`
- A wrapper around .net elements that is called by the javascript API.