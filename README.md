<img src="./hypar_logo.svg" width="200px" style="display: block;margin-left: auto;margin-right: auto;width: 50%;">

# elements
[![Build Status](https://travis-ci.org/hypar-io/sdk.svg?branch=master)](https://travis-ci.org/hypar-io/elements)
![NuGet](https://img.shields.io/nuget/v/HyparSDK.svg)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

Elements is a library for creating building elements like Walls, Beams, and Spaces. It's meant to be used by architects, engineers, and other building professionals who want to write code that generates buildings. Here's an example using Elements to create a `Beam`:
```c#
var line = new Line(Vector3.Origin, new Vector3(5,5,5));
var beam = new Beam(line, Profiles.WideFlangeProfile());
var model = new Model();
model.AddElement(beam);
var json = model.ToJson();
```
## Why
When we started [Hypar](https://www.hypar.io), we needed a small library of building elements that could run in micro-services executing on Linux, and was therefore free of dependencies on host applications like Rhino or Revit. We wanted it to have an API that took the best parts from the various object models and programming APIs available in the AEC space. We wanted it to serialize to formats like JSON and IFC that were useful to architects, engineers, and contractors. And even though the library needed to stand alone, we wanted it to be usable in add-ins to other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity. We looked around and nothing fit the bill, so we started building this. 

## Donate
Hypar Elements is open source and will remain so **forever**. Your donation will directly support the development of the Hypar Elements. Hypar Elements has been demonstrated to work in Revit add-ins, Unity projects, and as Lambdas running on AWS. Send us a donation and open a feature request telling us what you'd like it to do.  
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

## Getting Started Developing for the Hypar Platform
Elements is at the heart of the Hypar platform. A Hypar generator is a piece of code that is executed in the cloud to generate a building or a set of building components. You author the generator logic referencing the Elements library, and publish the generator to Hypar, then Hypar executes it for you and store the results. You can see some generators written using Hypar Elements running on [Hypar](https://hypar.io). Hypar is just one example of a business that can be built on top of this tool. We fully expect you'll go and build your own cool thing.

1. Install the CLI
The Hypar command line interface (CLI) is a tool that helps you publish your generator to Hypar. The CLI works on Windows from the windows command line and on Mac and Linux from your favorite terminal.
- Download for:
  - [Windows](https://s3-us-west-1.amazonaws.com/hypar-cli/hypar-win-x64.zip)
  - [Mac](https://s3-us-west-1.amazonaws.com/hypar-cli/hypar-osx.10.12-x64.zip)
  - [Linux](https://s3-us-west-1.amazonaws.com/hypar-cli/hypar-linux-x64.zip)
- Link the executable to make it available from your command line:
  - On Mac and Linux: `ln -s <path to hypar executable> /usr/local/bin/hypar`
  - On windows add `<path to hypar>` to your user `PATH`.
2. Create a generator.
Using the CLI, you can create a new generator by doing `hypar init <generator id>`. This will clone the generator repo to the folder `<generator id>`. Of course, you can replace `<generator-id>` with anything you like. The generator project is a buildable .net class library which references the Elements [NuGet package](https://www.nuget.org/packages/Hypar.Elements/).
3. Edit the `hypar.json`.
The `hypar.json` file describes the interface for your generator. The `inputs` in your generator will describe the data that needs to be supplied to your generator for it to run. These inputs will show up in Hypar (https://hypar.io/) as controls for the user to add some data. The supported input types are:
- `location` - A location defined as GEOJson.
- `range` - A numeric range from `min` to `max`, with intermediate values defined by `step`.
- `data` - A data input with an optional `content-type` property. 
  - `content-type` can be one of the following:
    - `text/csv` - CSV data.
    - `text/plain` - Raw text.
    - `application/json` - JSON.
4. Use the CLI to generate input and output classes and a function stub. From the same directory as your `hypar.json` do:
`hypar init`. This will generate `Input.gs.cs` and `Output.g.cs` classes which have properties which match your input and output properties. Addtionally, it will generate a `<function-id>.g.cs` whose `Execute(...)` method is where you put your business logic.

## Examples
The best examples are those provided in the [tests](https://github.com/hypar-io/sdk/tree/master/csharp/test/Hypar.SDK.Tests), where we demonstrate usage of almost every function in the library.

## Words of Warning
- The Hypar SDK is currently in beta. Please do not use it for production work.
- Why we chose C#:
  - C# is a strongly typed language. We want the code checking tools and the compiler to help you write code that you can publish with confidence. 
  - Microsoft is investing heavily in C# performance. There are lots of articles out there about Lambda performance. [Here's](https://read.acloud.guru/comparing-aws-lambda-performance-of-node-js-python-java-c-and-go-29c1163c2581) a good one.
  - Dotnet function packages are small. Smaller functions results in faster cold start times in serverless environments.
  - C# libraries can be reused in other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity.

## Third Party Libraries and Specifications

- [LibTessDotNet](https://github.com/speps/LibTessDotNet)  
- [Clipper](http://www.angusj.com/delphi/clipper.php)
- [GeoJson](http://geojson.org/)
- [glTF](https://www.khronos.org/gltf/).

## Building the SDK
You'll only need to do this if you want to contribute to the SDK, otherwise you can use the NuGet packages that are published regularly.

`dotnet build`

## Testing the SDK
```
dotnet test
```
