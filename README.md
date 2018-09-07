<img src="./hypar_logo.svg" width="300px" style="display: block;margin-left: auto;margin-right: auto;width: 50%;">

# Hypar SDK
[![Build Status](https://travis-ci.org/hypar-io/sdk.svg?branch=master)](https://travis-ci.org/hypar-io/sdk)

The Hypar SDK is a library for creating functions that execute on Hypar. A function is a piece of code that is executed in the cloud to build stuff. That stuff can be 3D geometry, which we store as glTF files, or data which we serialize to JSON and store in a document database. You create the function logic and publish the function to Hypar, then we execute it for you and store the results. The Hypar SDK provides object types that are useful for generating the built environment.

You can see some functions written using the Hypar SDK running on [Hypar Explore](https://explore.hypar.io/functions).

## Words of Warning
- The Hypar SDK is currently in beta. Please do not use it for production work.
- Why we chose C#:
  - C# is a strongly typed language. We want the code checking tools and the compiler to help you write code that you can publish with confidence. 
  - Microsoft is investing heavily in C# performance. There are lots of articles out there about Lambda performance. [Here's](https://read.acloud.guru/comparing-aws-lambda-performance-of-node-js-python-java-c-and-go-29c1163c2581) a good one.
  - Dotnet function packages are small. Smaller functions results in faster cold start times in serverless environments.
  - C# libraries can be reused in other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity.

## Examples
The best examples are those provided in the [tests](https://github.com/hypar-io/elements/tree/master/test), where we demonstrate usage of almost every function in the library.

## Build
`dotnet build`

## Test
`dotnet test`

## Third Party Libraries

- [LibTessDotNet](https://github.com/speps/LibTessDotNet)  
- [Verb](https://github.com/pboyer/verb)
- [GeoJson](http://geojson.org/)
- [glTF](https://www.khronos.org/gltf/).
