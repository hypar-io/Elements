<img src="./hypar_logo.svg" width="200px" style="display: block;margin-left: auto;margin-right: auto;width: 50%;">

# Elements
A BIM library for everybody.

[![Build Status](https://travis-ci.org/hypar-io/elements.svg?branch=master)](https://travis-ci.org/hypar-io/elements)
![NuGet](https://img.shields.io/nuget/v/Hypar.Elements.svg)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

Elements is a cross-platform library for creating building elements like Walls, Beams, and Spaces. It's meant to be used by architects, engineers, and other building professionals who want to write code that generates buildings. Here's an example using Elements to create a `Beam`:
```c#
var line = new Line(Vector3.Origin, new Vector3(5,5,5));
var beam = new Beam(line, new Profiles.WideFlangeProfile("testprofile"));
var model = new Model();
model.AddElement(beam);
var json = model.ToJson();
```
## Why
When we started [Hypar](https://www.hypar.io), we needed a small library of building elements that could run in micro-services executing on Linux, and was therefore free of dependencies on host applications like Rhino or Revit. We wanted it to have an API that took the best parts from the various object models and programming APIs available in the AEC space. We wanted it to serialize to formats like JSON and IFC that were useful to architects, engineers, and contractors. And even though the library needed to stand alone, we wanted it to be usable in add-ins to other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity. We looked around and nothing fit the bill, so we started building this. 

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
