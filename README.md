# Elements

[![Build Status](https://travis-ci.org/hypar-io/elements.svg?branch=master)](https://travis-ci.org/hypar-io/elements)
![NuGet](https://img.shields.io/nuget/v/Hypar.Elements.svg)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

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
The best examples are those provided in the [tests](https://github.com/hypar-io/elements/tree/master/test/Examples), where we demonstrate usage of almost every function in the library.

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

## Code Generation
Elements constructs its primitive types from schemas in the `/Schemas` directory. These schemas are provided as JSON schema. The generator mechanism can be found in the `/src/Generate` directory. Some things to note about code generation:
- Elements uses [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) to generate C# classes from JSON schemas.
- The default collection type used is `System.Collections.Generic.IList`. Primitive types have constructors which take `T[]` for historical support, but these will be upgraded in the future to `IList<T>`.
- Generated classes are marked as `partial` and do not contain constructors. The constructors are contained in the their partial counterparts.
- Elements uses a custom class liquid template for the code generator which can be found in `/Generate/Templates`.
- Core class definitions are generated as `CSharpClassStyle.POCO` using NJsonSchema. This results in class definitions without constructors. For user-defined types, class definitions will be generated as `CSharpClassStyle.Record` with JSON constructors.


## Geometry
We are often asked whether the Elements library supports the ____ geometry kernel. It does not. Yet. The geometry kernel that we've created for Elements is a very simple BREP kernel which does "flat stuff with holes in it" really well. We think Nurbs are sexy, and we'll definitely support more curvy stuff in the future, it's just that the effort required to support ____ geometry kernel for micro-services running in the cloud is not small. Good geometry kernels are also usually large, expensive, and not open source, so they introduce a lot of concerns which are orthogonal to why we built this library in the first place. If you are interested in using Elements with another geometry library, we love pull requests.

## Donate
Hypar Elements is open source and will remain so **forever**. Your donation will directly support the development of the Hypar Elements. Hypar Elements has been demonstrated to work in Revit add-ins, Unity projects, and as Lambdas running on AWS. Send us a donation and open a feature request telling us what you'd like it to do.  
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

## Relation To IFC
It's critical that Elements data be able to serialize to open standards that we use in AEC. The standard we have is IFC. The table below shows a mapping of Elements types to IFC types. 

|Elements Type|IFC Type|
|--|--|
|Beam|IfcBeam|
|Brace||
|Column|IfcColumn|
|Floor|IfcSlab|
|Opening|IfcOpeningElement|
|Panel||
|Space|IfcSpace|
|Topography||
|Wall|IfcWall|
|StandardWall|IfcWallStandardCase|

## Words of Warning
- The Elements library is currently undergoing rapid development and breaking API changes. Until we achieve a 1.0 release, we are playing a little fast and loose with semantic versioning. Updates will be written to the `CHANGELOG`.

## Build
You'll only need to do this if you want to contribute to the library, otherwise you can use the [NuGet package](https://www.nuget.org/) that is published regularly.

```
dotnet build
```

## Test
```
dotnet test
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

## Updating the Changelog
We use `CHANGELOG.md` to provide a list of changes to Elements. The easiest way to compile this log for new releases is to look at the commits that occurred between changes. This can be done as follows: `git log --pretty=oneline v0.3.6...v0.3.7`, where the tags are changed appropriately.
