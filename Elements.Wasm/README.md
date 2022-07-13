# Elements.Wasm
This is a minimal web assembly project.

### Deploy
The elements web assembly assets are served from S3 via cloud front. The deploy script will copy and deploy all necessary files to that bucket.
```
./deploy.sh
```

### Run
After using the deployment script above, you can run a local application against the deployed files by doing the following.
```
cd deploy
python3 -m http.server
```

### Files
`Elements.js`
- The javascript elements API. Elements is made available on the window object.

`ElementsAPI.cs`
- A wrapper around .net elements that is called by the javascript API.