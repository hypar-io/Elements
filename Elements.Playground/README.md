# Elements Playground
A playground for testing Elements in the browser.

This is a minimal example using .NET 5, Blazor webassembly, and Elements.

### Running
`dotnet watch run`

Visit http://localhost:5000.

### Publishing
`dotnet public -c release`

### Notes about hosting on Github Pages
- In the site's repo on github
  - Replace the base path in `index.html` with the path of the root of your site. For example, replace `/` with `/ElementsPlaygroundSite/` where `ElementsPlaygroundSite` is the name of the repo where the site is located.
  - Include a `.nojekyll` file to avoid the system skipping files and folders that start with `_`.
  - Add a `404.html` with contents identical to `index.html` to support redirection.
  - Add a `.gitattributes` file with `* binary` to avoid git modifying the assemblies in such a way that their hash becomes invalid.