# Elements
A BIM library for everybody.

[![Build Status](https://travis-ci.org/hypar-io/elements.svg?branch=master)](https://travis-ci.org/hypar-io/elements)
![NuGet](https://img.shields.io/nuget/v/Hypar.Elements.svg)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

# What
Elements is a cross-platform library for creating building elements. It's meant to be used by architects, engineers, and other building professionals who want to write code that generates buildings. Here's an example using Elements to create a `Beam`:
```c#
var line = new Line(Vector3.Origin, new Vector3(5,5,5));
var beam = new Beam(line, new Profiles.WideFlangeProfile("testprofile"));
var model = new Model();
model.AddElement(beam);
var json = model.ToIfc(ifcPath);
```
In addition to having a friendly API, the Elements library offers the ability to serialize your data to [glTF](https://www.khronos.org/gltf/), JSON, and [IFC](https://www.buildingsmart.org/about/what-is-openbim/ifc-introduction/).

## Why
One of the core philosophies at [Hypar](https://www.hypar.io) is that we shouldn't rebuild the wheel. When we started that project, we needed a library that would generate building elements and run at the core each function on the platform. We looked around for existing libraries that fulfilled the following requirements:
- The library can run in micro-services on Linux.
- The library is free of dependencies on host applications like Rhino or Revit.
- The library has an API that takes the best parts from the various object models and programming APIs available in the AEC space.
- The library can serialize data to formats like JSON, IFC,and glTF, that are useful to architects, engineers, contractors, and people building real-time visualization applications for AEC.
- The library is written in a language that supports developer productivity through things like type safety, and which supports code re-use in other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity.

Nothing fit the bill. So we started building this. 

## Donate
Hypar Elements is open source and will remain so **forever**. Your donation will directly support the development of the Hypar Elements. Hypar Elements has been demonstrated to work in Revit add-ins, Unity projects, and as Lambdas running on AWS. Send us a donation and open a feature request telling us what you'd like it to do.  
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

## Relation To IFC
The following table shows the types available in the Elements library and their corresponding types in IFC.

|Elements Type|IFC Type|
|--|--|
|Beam|IfcBeam|
||IfcChimney|
|Column|IfcColumn|
||IfcCovering|
||IfcCurtainWall|
||IfcDoor|
||IfcMember|
|Panel|IfcPlate|
||IfcRailing|
||IfcRamp|
||IfcRoof|
||IfcShadingDevice|
|Floor|IfcSlab|
||IfcStair|
|Wall|IfcWall|
|StandardWall|IfcWallStandardCase|
||IfcWindow|

## Examples
The best examples are those provided in the [tests](https://github.com/hypar-io/elements/tree/master/csharp/test), where we demonstrate usage of almost every function in the library.

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

## Third Party Libraries and Specifications

- [LibTessDotNet](https://github.com/speps/LibTessDotNet)  
- [Clipper](http://www.angusj.com/delphi/clipper.php)
- [GeoJson](http://geojson.org/)
- [glTF](https://www.khronos.org/gltf/).
