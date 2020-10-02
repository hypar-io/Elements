# **NOTE: Hypar for Revit is currently in beta. Do not use Hypar for Revit for production work.**

# Getting Started with Hypar for Revit
Hypar for Revit is a Revit addin that allows you to connect your Revit models to your Hypar workflows in two ways.  If you want live visualization of your hypar workflow as context inside your Revit model you can use the Hypar Hub, to stream the data from your workflow directly into the Revit view.  If you would like to integrate Hypar into your design work, you can extract Revit models into Hypar json files, use that json model as part of your Hypar workflow, and then use custom converters to bring your results back into Revit.  See below for more information on these two paths.  
  ![](./images/RevitHyparAddin.png)

## Installation (beta)
- Download and install the latest Revit plugin installer from [releases page](https://github.com/hypar-io/Elements/releases).
- [Install the latest Hypar CLI](https://hypar-io.github.io/Elements/C-Sharp.html#installing-and-using-the-hypar-cli). You'll need this to use the `hub` command. 
  - **NOTE: For betas, you'll need to install a beta release of the Hypar CLI which has a slightly different syntax.** 
  ```
  dotnet tool install -g hypar.cli --version 0.7.3-beta.1
  ```

# Live Visualization with Hypar Hub
## Running
- Start the hub. From the command line do `hypar hub`. See the hub instructions [here](./Hub.md).
- Open Revit, go to the addins tab, and look for the Hypar Panel. These two buttons start and stop the connection to the hyapr hub.

    ![](./images/RevitHyparAddinHub.png)

- Clicking on Hypar Hub Start will start visualization of the hub in your current active 3D view. You may need to rotate/pan/zoom to get the visualization to show.
- Clicking on Hypar Hub Stop will stop syncing with the hub.
- NOTE: If after rotate/pan/zoom, you still don't see visualization of your workflow geometry in the view, the hub may be misconfigured. Ensure that the hub references your workflow, and that the workflow is set to sync with the active Revit model. For more information on setting up the hub go [here](./Hub.md).

## Troubleshooting
- The Hypar Revit log is available at `C:/Users/{you}/./hypar/hypar-revit.log`. If you run into problems, you can send that log to support@hypar.io.

## Known Limitations
- When the hub is first started, you will not see any visualizations until you zoom, pan, or rotate. This is a limitation of Revit's `RefreshActiveView` API which does not work as advertised.
- Transparency is not currently supported.

# Converting Revit to and from Hypar
The conversion process is designed so anyone using the Elements library can add their own custom converters that will be run by the addin.  For more information about writing Element Converters refer to the 

## Extracting a Revit model to Elements
There are three buttons that support different filters while extracting your Revit model into a Hypar model.  
> Each button will only send elements if there are available converters that support that element type.

![](./images/RevitHyparAddinToHypar.png)
- *All to Hypar* will send everything in the model that can be sent.  Some items that are view specific, like Areas, won't be sent in this mode. 
- *Selection to Hypar* will send only the items that are currently selected.
- *Open Views to Hypar* sends all items visible in all open views.  Only items from the currently active model will be sent, not all open models.

Once you've chosen one of these button the converter will ask you to save a json file, and that json file can be imported into Hypar using the JSON to Model function.

## Loading an Elements model
Loading Elements into your Revit model requires converters that are designed to 
work with a given Revit template / project.  
![](./images/RevitHyparAddinFromHypar.png)

There are currently no converters installed by default, but we have designed the system so that you can build your own specific to your company's needs.  See the sample sample converter and documentation [here](https://github.com/hypar-io/ElementConverterSamples).  In the future as more converters come online we will be looking at ways for hte community to automatically share their converters, and if you would like further assistance please reach out to support@hypar.io