<img src="./hypar_logo.svg" width="300px" style="display: block;margin-left: auto;margin-right: auto;width: 50%;">

# SDK
[![Build Status](https://travis-ci.org/hypar-io/sdk.svg?branch=master)](https://travis-ci.org/hypar-io/sdk)
![NuGet](https://img.shields.io/nuget/v/HyparSDK.svg)

The Hypar SDK is a library for creating building elements like Walls, Beams, and Spaces. It's meant to be used by architects, engineers, and other building professionals who want to write code that generates buildings. Here's an example using the SDK to create a `Beam`:
```c#
var line = new Line(Vector3.Origin, new Vector3(5,5,5));
var beam = new Beam(line, new[]{Profiles.WideFlangeProfile()});
var model = new Model();
model.AddElement(beam);
var json = model.ToJson();
```

The Hypar SDK is also at the heart of the Hypar platform. A Hypar generator is a piece of code that is executed in the cloud to generate a building or a set of building components. You author the generator logic referencing the Hypar SDK, and publish the generator to Hypar, then Hypar executes it for you and store the results. You can see some generators written using the Hypar SDK running on [Hypar](https://hypar.io).

## Donate
<form action="https://www.paypal.com/cgi-bin/webscr" method="post" target="_top">
<input type="hidden" name="cmd" value="_s-xclick">
<input type="hidden" name="hosted_button_id" value="3HBW7BYRSBZYE">
<input type="image" src="https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif" border="0" name="submit" alt="PayPal - The safer, easier way to pay online!">
<img alt="" border="0" src="https://www.paypalobjects.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>

## Getting Started the Easy Way
The easiest way to get started is to clone the [starter](https://github.com/hypar-io/starter) repo, which already includes a reference to the Hypar SDK and some example code to get you started.
```bash
git clone https://github.com/hypar-io/starter
```

## Getting Started the Less Easy Way
The Hypar SDK is available as a [nuget package](https://www.nuget.org/packages/HyparSDK).
To install for dotnet projects:
```bash
dotnet add package HyparSDK
```

## Examples
The best examples are those provided in the [tests](https://github.com/hypar-io/sdk/tree/master/csharp/test/Hypar.SDK.Tests), where we demonstrate usage of almost every function in the library.

## Building the SDK
You'll only need to do this if you want to contribute to the SDK, otherwise you can use the Nuget packages that are published regularly.

`dotnet build`

## Testing the SDK
`dotnet test`

## Words of Warning
- The Hypar SDK is currently in alpha. Please do not use it for production work.
- Why we chose C#:
  - C# is a strongly typed language. We want the code checking tools and the compiler to help you write code that you can publish with confidence. 
  - Microsoft is investing heavily in C# performance. There are lots of articles out there about Lambda performance. [Here's](https://read.acloud.guru/comparing-aws-lambda-performance-of-node-js-python-java-c-and-go-29c1163c2581) a good one.
  - Dotnet function packages are small. Smaller functions results in faster cold start times in serverless environments.
  - C# libraries can be reused in other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity.

## Third Party Libraries

- [LibTessDotNet](https://github.com/speps/LibTessDotNet)  
- [Clipper](http://www.angusj.com/delphi/clipper.php)
- [GeoJson](http://geojson.org/)
- [glTF](https://www.khronos.org/gltf/).
