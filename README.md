# Elements

![Tag and Publish Alpha](https://github.com/hypar-io/Elements/workflows/Tag%20and%20Publish%20Alpha/badge.svg)
![Build and Test on PR](https://github.com/hypar-io/Elements/workflows/Build%20and%20Test%20on%20PR/badge.svg)
![NuGet](https://img.shields.io/nuget/v/Hypar.Elements.svg)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

# Projects
|Name|Description|
|----|----|
|Elements|The core elements library containing base geometric types.|
|Elements.CodeGeneration|Methods for converting JSON schema of Element types to C#.|
|Elements.Serialization.IFC|Methods for serializing a `Model` to IFC.|

# Words of Warning
- The Elements library is currently undergoing rapid development and breaking API changes. Until we achieve a 1.0 release, we are playing a little fast and loose with semantic versioning. Updates will be written to the [`CHANGELOG`](CHANGELOG.md).

## Getting Started
In a .net core project:
```
> dotnet add package Hypar.Elements
```
In Visual Studio:
```
PM> Install-Package Hypar.Elements
```
## Documentation
Find the documentation [here](https://hypar-io.github.io/Elements/index.html).

## Examples
The best examples are those provided in the [tests](https://github.com/hypar-io/Elements/tree/master/Elements/test), where we demonstrate usage of almost every function in the library.

## What
Elements is a cross-platform library for creating building elements. It's meant to be used by architects, engineers, and other building professionals who want to write code that generates buildings.

When we started [Hypar](https://www.hypar.io) we needed a library that would generate building elements and run at the core of each function on the platform. Because we don't like rebuilding the wheel, we looked around for existing libraries that fulfilled the following requirements:
- The library must be small and fast. Elements is currently ~300kb and we're working every day to make it smaller.
- The library must run in micro-services on Linux.
- The library must have great visual documentation. If we're going to pass this library on as a recommendation to developers on Hypar, we want great docs.
- The library must be free of dependencies on host applications like Rhino or Revit or geometry kernels like Open Cascade which, while really cool, become a black box in your system.
- The library must be able to serialize data to formats like JSON, [IFC](https://www.buildingsmart.org/about/what-is-openbim/ifc-introduction/),and [glTF](https://www.khronos.org/gltf/), that are useful to architects, engineers, contractors, and people building real-time visualization applications for AEC.
- The library must be written in a language that supports developer productivity through things like type safety, and which supports code re-use in other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity.
- Serialization and deserialization of types that extend `Element` should be possible provided that those types are made up of primitives defined in this library.

We couldn't find anything quite right. So we started building this. 

## Design Principles
- There is one base type: Element.
  - Elements have a unique identifier and a name.
  - An Element can have any number of properties whose types are defined in the provided schemas.
- The library is schema first. 
  - Elements is a C# library presently, but we expect that Element types will be used in other languages in the future. Therefore, we shouldn't rely on capabilities of C# (ex: attributes) to convey meaning of the types or their properties. 
- The core Element types will be defined in exactly the same way that third-party types will be defined. 
  - It is possible that over time these types (ex: Beam, Column, Wall, etc.) are removed from the library and only made available as schemas from which user elements can be derived.
- User-defined element schemas should perform as first class citizens in the system.

## Code Generation
- Elements constructs its primitive types from schemas in the `/Schemas` directory. These schemas are provided as JSON schema. 
- Elements uses [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) to generate C# classes from JSON schemas.
- C# classes can be generated using the Hypar CLI's `hypar generate-types` command. For users of Visual Studio Code, the "CLI Generate Elements" task can be used.
- The default collection type used is `System.Collections.Generic.IList`.
- Generated classes are marked as `partial`. You can add constructors using a separate partial class, but remember that those constructors will not be available to other developers unless you share them in a library (ex: a NuGet package).
- The custom class template for the code generator can be found in `/Generate/Templates`.
- Core class definitions are generated as `CSharpClassStyle.POCO` using NJsonSchema. This results in class definitions without constructors.
- Deserialization into inherited types is handled in two ways:
  - Base types that live in the Elements library are decorated with one or more `JsonInheritanceAttribute` pointing to their derived types.
  - External types that inherit from `Element` must be decorated with the `UserElement` attribute. This is required because a type author doesn't have access to the base types, and must therefore signify to the serializer that it needs to load a specific type.


## Geometry
Elements contains a very simple BREP geometry kernel, and a small set of geometric types like vectors, lines, and polygons. Elements uses a right-handed coordinate system with +Z "up". Elements is unitless except as indicated when calling a geometric method (ex: arcs requires angles in degrees).

We are often asked whether the Elements library supports the ____ geometry kernel. It does not. Yet. The geometry kernel that we've created for Elements is a very simple BREP kernel which does "flat stuff with holes in it" really well. We think Nurbs are sexy, and we'll definitely support more curvy stuff in the future, it's just that the effort required to support ____ geometry kernel for micro-services running in the cloud is not small. Good geometry kernels are also usually large, expensive, and not open source, so they introduce a lot of concerns which are orthogonal to why we built this library in the first place. If you are interested in using Elements with another geometry library, we love pull requests.

## Precision
Geometry operations in Elements use `Vector3.Epsilon=1e-05` to compare values that should be considered equal. This is important as geometric operations using floating point numbers are imprecise. In addition, .NET will return different values for these operations _on different systems_. We have seen intersection tests that pass on a mac and fail on linux. Please use the provided methods like `double.IsAlmostEqualTo(...)`, `Vector3.IsZero()`, and `Vector3.IsAlmostEqualTo(...)` which account for precision.

## Donate
Hypar Elements is open source and will remain so **forever**. Your donation will directly support the development of the Hypar Elements. Hypar Elements has been demonstrated to work in Revit add-ins, Unity projects, and as Lambdas running on AWS. Send us a donation and open a feature request telling us what you'd like it to do.  
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

## Build
You'll only need to do this if you want to contribute to the library, otherwise you can use the [NuGet package](https://www.nuget.org/) that is published regularly.

```
dotnet build
```

## Test
```
dotnet test
```

## Benchmark
```
cd Elements.Benchmarks
dotnet run -c Release
```

## Building the Documentation
```
cd doc
docfx -f --serve
```

## Third Party Libraries and Specifications

- [LibTessDotNet](https://github.com/speps/LibTessDotNet)  
- [Clipper](http://www.angusj.com/delphi/clipper.php)
- [GeoJson](http://geojson.org/)
- [glTF](https://www.khronos.org/gltf/).
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
- [SixLabors.ImageSharp.Drawing](https://github.com/SixLabors/ImageSharp.Drawing)
- [SixLabors.Fonts](https://github.com/SixLabors/Fonts)
- [NJsonSchema](https://github.com/RicoSuter/NJsonSchema)
- [Csg](https://github.com/praeclarum/Csg)
- [NetOctree](https://github.com/mcserep/NetOctree)

## Updating the Changelog
We use [`CHANGELOG.md`](CHANGELOG.md) to provide a list of changes to Elements. The easiest way to compile this log for new releases is to look at the commits that occurred between changes. This can be done as follows: `git log --pretty=oneline v0.3.6...v0.3.7`, where the tags are changed appropriately.
