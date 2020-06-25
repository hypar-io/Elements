# **NOTE: Hypar for Revit is currently in beta. Do not use Hypar for Revit for production work.**

# Getting Started with Hypar for Revit
Hypar for Revit is a Revit addin that connects Revit to the Hypar Hub. Hub workflows will be temporarily visualized in the active 3D view of your Revit document.

## Installation (beta)
- [Install the latest Hypar CLI](https://hypar-io.github.io/Elements/C-Sharp.html#installing-and-using-the-hypar-cli). You'll need this to use the `hub` command.
- Download the latest zip from the [releases page](https://github.com/hypar-io/Elements/releases).
- Unzip into `C:\ProgramData\Autodesk\Revit\Addins\2020`. After unzipping you should see a `Hypar.Revit.addin` at the top level of the addins folder and a `Hypar.Revit` subfolder.

## Running
- Start the hub. From the command line do `hypar hub`.
- Open Revit. If your addin is installed correctly, two new commands will appear in the External Tools button on the Revit Ribbon.  

  ![](./images/RevitExternalCommands.png)

- Clicking on Hypar Hub Start will start visualization the hub in your current active 3D view.
- Clicking on Hypar Hub Stop will stop syncing with the hub.

## Troubleshooting
- The Hypar Revit log is available at `C:/Users/{you}/./hypar/hypar-revit.log. If you run into problems, you can send that log to support@hypar.io.

## Known Limitations
- When the hub is first started, you will not see any visualizations until you zoom, pan, or rotate. This is a limitation of Revit's `RefreshActiveView` API which does not work as advertised.
- The visualization only works in `Shaded` mode in Revit. In other modes, the visualization will be all black.
