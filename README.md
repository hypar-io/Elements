<img src="./hypar_logo.svg" width="300px" style="display: block;margin-left: auto;margin-right: auto;width: 50%;">

# SDK
[![Build Status](https://travis-ci.org/hypar-io/sdk.svg?branch=master)](https://travis-ci.org/hypar-io/sdk)

The Hypar SDK is a library for creating generators that execute on Hypar. A generator is a piece of code that is executed in the cloud to build stuff. The Hypar SDK contains types for a variety of building elements. You author the generator logic and publish the generator to Hypar, then we execute it for you and store the results. You can see some generators written using the Hypar SDK running on [Hypar](https://hypar.io/functions).

## Getting Started the Easy Way
The easiest way to get started is to clone the [starter](https://github.com/hypar-io/starter) repo, which already includes a reference to the Hypar SDK and some example code to get you started.
```
git clone https://github.com/hypar-io/starter
```

## Getting Started the Less Easy Way
The Hypar SDK is available as a [nuget package](https://www.nuget.org/packages/HyparSDK).
To install for dotnet projects:
```
dotnet add package HyparSDK
```

## Words of Warning
- The Hypar SDK is currently in alpha. Please do not use it for production work.
- Why we chose C#:
  - C# is a strongly typed language. We want the code checking tools and the compiler to help you write code that you can publish with confidence. 
  - Microsoft is investing heavily in C# performance. There are lots of articles out there about Lambda performance. [Here's](https://read.acloud.guru/comparing-aws-lambda-performance-of-node-js-python-java-c-and-go-29c1163c2581) a good one.
  - Dotnet function packages are small. Smaller functions results in faster cold start times in serverless environments.
  - C# libraries can be reused in other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity.

## Examples
The best examples are those provided in the [tests](https://github.com/hypar-io/sdk/tree/master/csharp/test/Hypar.SDK.Tests), where we demonstrate usage of almost every function in the library.

## Building the SDK
You'll only need to do this if you want to contribute to the SDK, otherwise you can use the Nuget packages that are published regularly.

`dotnet build`

## Testing the SDK
`dotnet test`

## Third Party Libraries

- [LibTessDotNet](https://github.com/speps/LibTessDotNet)  
- [Clipper](http://www.angusj.com/delphi/clipper.php)
- [GeoJson](http://geojson.org/)
- [glTF](https://www.khronos.org/gltf/).
