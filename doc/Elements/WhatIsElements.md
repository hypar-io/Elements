# What is Elements?
Elements is a cross-platform library for creating building elements. It's meant to be used by architects, engineers, and other building professionals who want to write code that generates buildings.

When we started [Hypar](https://www.hypar.io) we needed a library that would generate building elements and run at the core of each function on the platform. Because we don't like rebuilding the wheel, we looked around for existing libraries that fulfilled the following requirements:
- The library must be small and fast. Elements is currently ~300kb and we're working every day to make it smaller.
- The library must run in micro-services on Linux.
- The library must have great visual documentation. If we're going to pass this library on as a recommendation to developers on Hypar, we want great docs.
- The library must be free of dependencies on host applications like Rhino or Revit or geometry kernels like Open Cascade which, while really cool, become a black box in your system.
- The library must be able to serialize data to formats like JSON, [IFC](https://www.buildingsmart.org/about/what-is-openbim/ifc-introduction/),and [glTF](https://www.khronos.org/gltf/), that are useful to architects, engineers, contractors, and people building real-time visualization applications for AEC.
- The library must be written in a language that supports developer productivity through things like type safety, and which supports code re-use in other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity.

We couldn't find anything quite right. So we started building this.

## Where Can I Get Elements?
If you want to use Hypar in your .NET project, it's available as a [NuGet package](https://www.nuget.org/packages/Hypar.Elements/). It can be installed like this:
```bash
dotnet add package Hypar.Elements --version 0.3.3
```
Elements is also open source. The code is available in the [github repository](https://github.com/hypar-io/Elements).

## Where Can I Use Elements?
Elements will work in any project that supports the .NET Standard 2.0 API set. This includes Revit Addins (including Dynamo), Rhino addins (including Grasshopper), Unity games, and more! You can find out about more about .NET Standard API coverage [here](https://github.com/dotnet/standard/blob/master/docs/versions.md).

Code you write with Elements is **just code**, you are free to use it wherever it is compatible. One place we recommend is [Hypar](../index.html)

## Where Can I Get Some Code Examples?
The Elements library API documentation is located [here](../api/Elements.html).

## Does Elements Create Geometry?
Elements contains a very simple geometry library full of classes like `Vector3`, `Line`, `Polygon`, and `Arc`. It contains [boundary representation](https://en.wikipedia.org/wiki/Boundary_representation)(BREP) classes with methods for creating extrusions and sweeps, a `Mesh` class for stuff like topographies that don't represent well using BREPs, and classes for working with [GeoJSON](https://geojson.org/). We like to joke that Elements' geometry library does "flat stuff with holes in it" really well.

## I'd Like a Feature or Something is Broken
All conversations about Elements happen on the [github repository](https://github.com/hypar-io/Elements). Please open a new issue there to start a discussion.
