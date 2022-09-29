# Elements

![Tag and Publish Alpha](https://github.com/hypar-io/Elements/workflows/Tag%20and%20Publish%20Alpha/badge.svg)
![Build and Test on PR](https://github.com/hypar-io/Elements/workflows/Build%20and%20Test%20on%20PR/badge.svg)
![NuGet](https://img.shields.io/nuget/v/Hypar.Elements.svg)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

# Projects
|Name|Description|
|----|----|
|Elements|The core elements library containg the `Element` type, the geometry kernel, and all other core building element types.|
|Elements.Benchmarks|Benchmarks and tracing for Elements.|
|Elements.CodeGeneration|Methods for converting JSON schema of Element types to C#.|
|Elements.Components|Component creation for Elements.|
|Elements.Playground|A live code-editing application for generating elements in the browser.|
|Elements.Serialization.DXF|Methods for serializing a `Model` to and from DXF.|
|Elements.Serialization.IFC|Methods for serializing a `Model` to IFC.|

## Latest Updates
For the latest updates see the [`CHANGELOG`](CHANGELOG.md).
 
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
Elements is a cross-platform library for creating building elements. Elements is for architects, engineers, and other building professionals who want to write code that generates buildings.

When we started [Hypar](https://www.hypar.io) we needed a library that would generate building elements and run at the core of each function on the platform. Because we don't like rebuilding the wheel, we looked around for existing libraries that fulfilled the following requirements:
- The library must be small and fast.
- The library must run in micro-services on Linux.
- The library must have great visual documentation. If we're going to pass this library on as a recommendation to developers on Hypar, we want great docs.
- The library must be free of dependencies on host applications like Rhino or Revit.
- The library must be free of dependencies on proprietary geometry kernels.
- The library must be able to serialize data to formats that are useful to architects, engineers, contractors, and people building real-time visualization applications for AEC, like JSON, [IFC](https://www.buildingsmart.org/about/what-is-openbim/ifc-introduction/),and [glTF](https://www.khronos.org/gltf/).
- The library must be written in a language that supports developer productivity through things like type safety.
- The library should support code re-use in other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity.
- Serialization and deserialization of types that extend `Element` should be possible provided that those types are made up of primitives defined in this library.

We couldn't find anything quite right. So we started building this.

## Design Principles
- An `Element` is a uniquely identifiable piece of a building system.
- Elements can contain references to other elements. Consider a Truss which is made of individual structural framing elements.
- Elements can be instanced. The original element is considered the "base definition". An element instance contains a reference to the base element, a transform, and a name.
- Elements is a C# library presently, but we expect that Element types will be used in other languages in the future. Therefore, we shouldn't rely on capabilities of C#, like attributes, to convey meaning of the types or their properties.
- The core Element types will be defined in exactly the same way that third-party types will be defined.
- It is possible that over time these types (ex: Beam, Column, Wall, etc.) are removed from the library and only made available as schemas from which user elements can be derived.

## Why Not Use IFC?
In IFC, Revit, and other "BIM" applications, the building element ontology is fixed. If you want to introduce a new element which is key to your work process, you need to find the most closely matching category and put your element there. In Revit you might use the "Generic" category. In IFC you might use the `IFCBuildingElementProxy` type. This makes it very difficult for the recipient of a model to reason about the model semantically. Elements enables the user to create "first class" element types in the system. If you want to create a Curtain Wall Bracket, you simply create a class `CurtainWallBracket : Element` and users can search for your element by its defined type.

## Geometry
Elements contains a very simple BREP geometry kernel, and a small set of geometric types like vectors, lines, and polygons. Elements uses a right-handed coordinate system with +Z "up". Elements is unitless except as indicated when calling a geometric method. For example, arcs requires angles in degrees.

The geometry kernel that we've created for Elements is a very simple BREP kernel which does "flat stuff with holes in it" really well. We think Nurbs are sexy, and we'll definitely support more curvy stuff in the future, it's just that the effort required to support arbitrarily complex geometry for micro-services running in the cloud is not small. Professional geometry kernels, like the kind found in mechanical modeling applications, are also usually large, expensive, and not open source. They introduce cost and complexity, and restrict the open nature of code that you write with Elements.

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
dotnet run -c release --filter '<benchmark class name>'
```

## Documentation
When adding sample code you need to add a special block of text to the class or method where you want the code to appear. The value of `name` at the end of the URI will be matched with open and close tags in the test file. See Joist.cs and StructuralFramingTests.cs for an sample.
```
        /// <example>
        /// [!code-csharp[Main](../../Elements/test/StructuralFramingTests.cs?name=example)]
        /// </example>
```
You may add up to one sample glb file per class, and when you name it the name must match the namespace, and class you are trying to demonstrate with `_` instead of `.`.  For example `Elements.Spatial.Grid2d` sample glb is named `Elements_Spatial_Grid2d`.
### Building the Documentation
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
- [Csg](https://github.com/praeclarum/Csg) We work with a customized fork of this project.  Currently using branch `hypars-branch`
- [NetOctree](https://github.com/mcserep/NetOctree)

## Updating the Changelog
We use [`CHANGELOG.md`](CHANGELOG.md) to provide a list of changes to Elements. The easiest way to compile this log for new releases is to look at the commits that occurred between changes. This can be done as follows: `git log --pretty=oneline v0.3.6...v0.3.7`, where the tags are changed appropriately.
