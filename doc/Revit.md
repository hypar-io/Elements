# **NOTE: Hypar for Revit is currently in beta. Do not use Hypar for Revit for production work.**

# Getting Started with Hypar for Revit
Hypar for Revit is a Revit addin that connects Revit to the Hypar Hub. Hub workflows will be temporarily visualized in the active 3D view of your Revit document.

## Installation (beta)
- [Install the latest Hypar CLI](https://hypar-io.github.io/Elements/C-Sharp.html#installing-and-using-the-hypar-cli). You'll need this to use the `hub` command.
- Download the latest zip from the [releases page](https://github.com/hypar-io/Elements/releases).
- Unzip into `C:\Users\<you>\AppData\Roaming\Autodesk\Revit\Addins\2020`. After unzipping you should see a `Hypar.Revit.addin` at the top level of the addins folder and a `Hypar.Revit` subfolder.

## Running
- Start the hub. From the command line do `hypar hub`.
- Open Revit. If your addin is installed correctly, two new commands will appear in the External Tools button on the Revit Ribbon.  

  ![](./images/RevitExternalCommands.png)

- Clicking on Hypar Hub Start will start visualization of the hub in your current active 3D view. You may need to rotate/pan/zoom to get the visualization to show.
- Clicking on Hypar Hub Stop will stop syncing with the hub.
- NOTE: If after rotate/pan/zoom, you still don't see visualization of your workflow geometry in the view, the hub may be misconfigured. Ensure that the hub references your workflow, and that the workflow is set to sync with the active Revit model. For more information on setting up the hub go [here](./Hub.md).

## Troubleshooting
- The Hypar Revit log is available at `C:/Users/{you}/./hypar/hypar-revit.log. If you run into problems, you can send that log to support@hypar.io.

## Known Limitations
- When the hub is first started, you will not see any visualizations until you zoom, pan, or rotate. This is a limitation of Revit's `RefreshActiveView` API which does not work as advertised.
- The visualization only works in `Hidden Line` and `Shaded` modes in Revit. This is a limitation of Revit's `DirectContext3D` API.
- Transparency is not currently supported.
