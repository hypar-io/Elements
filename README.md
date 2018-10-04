<img src="./hypar_logo.svg" width="300px" style="display: block;margin-left: auto;margin-right: auto;width: 50%;">

# elements
[![Build Status](https://travis-ci.org/hypar-io/sdk.svg?branch=master)](https://travis-ci.org/hypar-io/sdk)
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
When we started [Hypar](https://www.hypar.io), we needed a small library of building elements that could run in microservices executing on Linux, and was therefore free of dependencies on host applications like Rhino or Revit. We wanted it to have an API that took the best parts from the various object models and programming APIs available in the AEC space. We wanted it to serialize to formats like JSON and IFC that were useful to architects, engineers, and contractors. And even though the library needed to stand alone, we wanted it to be usable in addins to other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity. We looked around and nothing fit the bill, so we started building this. 

## Donate
Hypar Elements is open source and will remain so **forever**. Your donation will directly support the development of the Hypar Elements. Hypar Elements has been demonstrated to work in Revit addins, Unity projects, and as Lambdas running on AWS. Send us a donation and open a feature request telling us what you'd like it to do.  
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

## Getting Started
Hypar Elements is available as a [nuget package](https://www.nuget.org/packages/HyparSDK).
To install for dotnet projects:
```bash
dotnet add package HyparSDK
```

## Examples
The best examples are those provided in the [tests](https://github.com/hypar-io/sdk/tree/master/csharp/test/Hypar.SDK.Tests), where we demonstrate usage of almost every function in the library.

## Building the SDK
You'll only need to do this if you want to contribute to the Elements library, otherwise you can use the Nuget packages that are published regularly.
```
dotnet build
```

## Testing the SDK
```
dotnet test
```

## Words of Warning
- Hypar Elements is currently in alpha. Please do not use it for production work.

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
