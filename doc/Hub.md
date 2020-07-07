# Getting Started with the Hypar Hub
The Hypar Hub acts as the single point of synchronization between your workflows on Hypar and your local machine. The hub listens for update messages from Hypar, and builds a local cache of data representating the output of the functions in your workflow. To learn how to use the hub, you can do `hypar hub -h` from the command line
```
hub:
  Work with the hypar hub.

Usage:
  Hypar hub [command]

Commands:
  add       Add a workflow to a hub.
  remove    Remove a workflow from the hub.
  hide      Hide a function in a workflow.
  show      Show a function in a workflow.
```
When run without modifiers, the `hypar hub` command will use the settings provided by the `hub.json` in your [hypar directory](#Hypar-Directory). If a `hub.json` file cannot be found, one will be created for you.

The first time a `hub.json` file is created, it will ask you to add a workflow to the hub. You can find the id of a workflow by looking at its url in Hypar. The workflow id is the last part of the url. Here's an example: `https://hypar.io/w26cc62-e421-4790-89a5-a9332c3427a2orkflows/80`. The workflow id is `8026cc62-e421-4790-89a5-a9332c3427a2`.

The next thing the hub command will ask for is a Revit file name. If you associate a Revit file name with a workflow in the hub, the workflow's contents will be visible inside that Revit project. The file name should be added withtout the path, like `Building.rvt`. You can leave this blank. If you do, the newly added workflow won't show up in any Revit projects.

If you've already got a `hub.json` and would like to add or remove a workflow, you can do `hypar hub add` or `hypar hub remove` to add or remove workflows. You can see some more examples of how to use the hub [here](#Examples)

Once you've got a valid `hub.json` file, you can run the hub like this:
```
hypar hub
```
Once started, the hub will do two things. First, it syncs the workflows specified in your `hub.json` from Hypar to your local hypar directory. Then it creates a .NET assembly containing all of the types that are used by all the functions in your workflows. The hub writes data into the current user's home directory in a folder called `.hypar` (this might be hidden on some machines). For example, on Windows, the hub writes its data to `C:/Users/{user name}/.hypar`. Log files are also written to this directory.

When changes are made to workflows that are being watched by the hub, the hub will receive those changes and update the local data store. It will then emit change notifications to all connected clients.

## Hypar Directory
The hub stores information locally in a `.hypar` directory in the user's "home" folder. For Windows users this will be something like 
```
C:\Users\<you>\.hypar
```
For *nix users this will be
```
~/.hypar
```

## Examples
Run the hub:
```
hypar hub
```
Add a workflow to the hub:
```
hypar hub add --id 8026cc62-e421-4790-89a5-a9332c3427a2
```
Remove a workflow from the hub:
```
hypar hub remove --id 8026cc62-e421-4790-89a5-a9332c3427a2
```
Associate a Revit file with a workflow:
```
hypar hub add revit --id 8026cc62-e421-4790-89a5-a9332c3427a2 --file-name Building.rvt
```
Remove the association between a Revit file and a workflow:
```
hypar hub remove revit --id 8026cc62-e421-4790-89a5-a9332c3427a2 --file-name Building.rvt
```
Hide a function in a workflow:
```
hypar hub hide --id 8026cc62-e421-4790-89a5-a9332c3427a2 --function-name Levels by Envelope
```

## Settings
The `hub.json` file provides configuration information for the hub. You can edit it manually or use the Hypar CLI to add or remove workflows, control visibility of functions in workflows, or sync specific workflows with Revit files.

- A `hub.json` file looks like this:
```json
{
    "8026cc62-e421-4790-89a5-a9332c3427a2": {
        "hidden": [
            "Envelope By Sketch",
            "Levels By Envelope"
        ],
        "revit": {
            "file_name": "HyparTest.rvt"
        },
        "active": false
    },
}
```
- The `hidden` property is an array of function names which you would like to be hidden from hub clients. It's often useful to hide certain functions, like those which provide datums: Levels, Grids, etc., so that your client application doesn't need to draw or import those.
- The `active` property is a boolean value indicating whether the workflow should be sent to the hub clients. A workflow with `"active": false` will still be synced to the hub, but its update notifications will not be sent to the clients.
- The `revit` property contains information for syncing with Revit. The `file_name` property on this object specifies the file name (without directory) of the Revit file in which this workflow will be made visible.

## Hub Clients
Hub clients receive update messages from the hub when data has changed. For example, when the Revit hub client receives an update message it updates the visualization in the active 3D view to include what's in the hub. If you're interested in seeing how a hub client works internally, you can view the Revit hub source code on github.

## Known Limitations
- The hub creates dlls containing all the types required by your synced workflows. For applications which will deserialize models from JSON, these assemblies are loaded to provide type definitions for deserialization. In applications like Revit, assemblies can not be unloaded and are locked by the system. After the first load of an assembly, you can not update it or reload it. If you add a function to a workflow, the hub will attempt to create a new assembly containing the types required by that workflow, and will not be able to. You will need to restart the hub.