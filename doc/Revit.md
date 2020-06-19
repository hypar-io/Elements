# **NOTE: Hypar for Revit is currently in beta. Do not use Hypar for Revit for production work.**

# Getting Started with Hypar for Revit
Hypar for Revit is a Revit addin that connects Revit to the Hypar Hub. Hub workflows will be temporarily visualized in the active 3D view of your Revit document.

# Installation
- [Install the latest Hypar CLI](https://hypar-io.github.io/Elements/C-Sharp.html#installing-and-using-the-hypar-cli). You'll need this to use the `hub` command.
- Download the latest zip from the [releases page](https://github.com/hypar-io/Elements/releases).
- Unzip the addin to a location on your hard drive.
- Place the `Hypar.Revit.addin` file from the zip in `C:\ProgramData\Autodesk\Revit\Addins\2020` (change the Revit version as appropriate).
- Ensure that the `Hypar.Revit.Addin` points to the correct path of the `Hypar.Revit.dll`.
  - For example, if you unzipped the zip into `C:/Users/Ian/Documents/Hypar.Revit`, then all references to `Hypar.Revit.dll` will need to be updated as follows:
  ```xml
  ...
  <AddIn Type="Application">
    <Name>HyparRevit</Name>
    <FullClassName>Hypar.Revit.HyparHubApp</FullClassName>
    <Description>"Sync with a Hypar workflow."</Description>
    <Assembly>C:/Users/Ian/Documents/Hypar.Revit/Hypar.Revit.dll</Assembly>
    <AddInId>502fe383-2648-4e98-adf8-5e6047f9dc34</AddInId>
    <VendorDescription>"Hypar Inc., hypar.io"</VendorDescription>
    <VendorId>HYPR</VendorId>
  </AddIn>
  ...
  ```

# Running
- Start the hub. From the command line do `hypar hub`.
- Open Revit. If your addin is installed correctly, two new commands will appear in the External Tools button on the Revit Ribbon.  

  ![](./images/RevitExternalCommands.png)

- Clicking on Hypar Hub Start will start visualization the hub in your current active 3D view.
- Clicking on Hypar Hub Stop will stop syncing with the hub.

# Troubleshooting
- The Hypar Revit log is available at `C:/Users/{you}/./hypar/hypar-revit.log. If you run into problems, you can send that log to support@hypar.io.
