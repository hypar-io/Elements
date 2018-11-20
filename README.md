<img src="./hypar_logo.svg" width="200px" style="display: block;margin-left: auto;margin-right: auto;width: 50%;">

# elements
[![Build Status](https://travis-ci.org/hypar-io/sdk.svg?branch=master)](https://travis-ci.org/hypar-io/elements)
![NuGet](https://img.shields.io/nuget/v/HyparSDK.svg)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

Hypar Elements is a library for creating building elements like Walls, Beams, and Spaces. It's meant to be used by architects, engineers, and other building professionals who want to write code that generates buildings. Here's an example using Elements to create a `Beam`:
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

## Getting Started the Easy Way
The easiest way to get started is to clone the [starter](https://github.com/hypar-io/starter) repo, which already includes a reference to the Hypar SDK and some example code to get you started.
```bash
git clone https://github.com/hypar-io/starter
```

## Getting Started the Less Easy Way
The Hypar SDK is available as a [NuGet package](https://www.nuget.org/packages/HyparSDK).
To install for dotnet projects:
```bash
dotnet add package HyparSDK
```

## Examples
The best examples are those provided in the [tests](https://github.com/hypar-io/sdk/tree/master/csharp/test/Hypar.SDK.Tests), where we demonstrate usage of almost every function in the library.

## Building the SDK
You'll only need to do this if you want to contribute to the SDK, otherwise you can use the NuGet packages that are published regularly.

`dotnet build`

## Testing the SDK
```
dotnet test
```

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

## Hypar Elements
Hypar Elements is at the heart of the Hypar platform. A Hypar generator is a piece of code that is executed in the cloud to generate a building or a set of building components. You author the generator logic referencing the Hypar Elements library, and publish the generator to Hypar, then Hypar executes it for you and store the results. You can see some generators written using Hypar Elements running on [Hypar](https://hypar.io). Hypar is just one example of a business that can be built on top of this tool. We fully expect you'll go and build your own cool thing.

## Getting Started Developing for the Hypar Platform
The easiest way to get started is to clone the [starter](https://github.com/hypar-io/starter) repo, which already includes a reference to the Hypar SDK and some example code to get you started.
```bash
git clone https://github.com/hypar-io/starter
```
