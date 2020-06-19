# Getting Started with the Hypar Hub
The Hypar Hub acts as the single point of synchronization between your workflows on hypar and your local machine. The hub listens for update messages from hypar, and builds a local cache of data representating the output of the functions in your workflow. By having one hub on the machine manage this communication we remove this requirement from the host application plugins, and we provide an offline store of data.

After installing the hypar CLI, you can start the hypar hub like this:
```
hypar hub
```

## Where's My Hub?
The hub writes data into the current user's home directory in a folder called `.hypar` (this might be hidden on some machines). For example, on Windows, the hub writes its data to `C:/Users/{user name}/.hypar`. Log files are also written to this directory.


## Settings
You can control how the hub synchronizes with hypar by editing the `hub.json` file located in the `.hypar` directory. The `hub.json` file looks like this:
```json
{
    "2e534890-ab60-40ad-9fb0-681d313a08e6": {
        "hidden": [
            "JSON to Model",
            "Panels From Walls"
        ],
        "active": false
    },
    "8026cc62-e421-4790-89a5-a9332c3427a2": {
        "hidden": [
            "Envelope By Sketch",
            "Levels By Envelope"
        ],
        "active": false
    },
}
```
The keys in the `hub.json` file are the identifiers of Workflows on Hypar. You can see the id of a Workflow on Hypar by inspecting its url in the hypar web application:
```
https://hypar.io/workflows/8026cc62-e421-4790-89a5-a9332c3427a2 <-- There it is!
```
- The `hidden` property is an array of function names which you would like to be hidden from hub clients. It's often useful to hide certain functions, like those which provide datums: Levels, Grids, etc., so that your client application doesn't need to draw or import those.
- The `active` property is a boolean value specifying whether the workflow should be sent to the hub clients. A workflow with `"active": false` will still be synced to the hub, but its update notifications will not be sent to the clients.

## Hub Clients
Hub clients receive update messages from the hub when data has changed. For example, when the Revit hub client receives an update message it updates the visualization in the active 3D view to include what's in the hub. If you're interested in seeing how a hub client works internally, you can view the Revit hub source code on github.
