# Elements.Wasm
This is a minimal web assembly project.

### Build and Deploy
The elements web assembly assets are served from S3 via cloud front at https://elements.hypar.io. The deploy script will publish the "app" and copy and deploy all necessary files.
```
./deploy.sh
```

### Run
After using the deployment script above, you can run a local application against the deployed files by doing the following.
```
cd wwwroot
python3 -m http.server
```

### Files
`Elements.js`
- The javascript elements API. Elements is made available on the window object.

`ElementsAPI.cs`
- A wrapper around .net elements that is called by the javascript API.