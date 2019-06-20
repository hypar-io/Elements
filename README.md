# Elements
A BIM library for everybody.

[![Build Status](https://travis-ci.org/hypar-io/elements.svg?branch=master)](https://travis-ci.org/hypar-io/elements)
![NuGet](https://img.shields.io/nuget/v/Hypar.Elements.svg)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

## Documentation
Find the documentation [here](https://hypar-io.github.io/Elements/index.html).

## What
Elements is a cross-platform library for creating building elements. It's meant to be used by architects, engineers, and other building professionals who want to write code that generates buildings. Here's an example using Elements to create a `Beam`:
```c#
var line = new Line(Vector3.Origin, new Vector3(5,5,5));
var beam = new Beam(line, new Profiles.WideFlangeProfile("testprofile"));
var model = new Model();
model.AddElement(beam);
var json = model.ToIfc(ifcPath);
```

## Why
One of the core philosophies at [Hypar](https://www.hypar.io) is that we shouldn't rebuild the wheel. When we started that project, we needed a library that would generate building elements and run at the core each function on the platform. We looked around for existing libraries that fulfilled the following requirements:
- The library can run in micro-services on Linux.
- The library is free of dependencies on host applications like Rhino or Revit.
- The library has an API that takes the best parts from the various object models and programming APIs available in the AEC space.
- The library can serialize data to formats like JSON, [IFC](https://www.buildingsmart.org/about/what-is-openbim/ifc-introduction/),and [glTF](https://www.khronos.org/gltf/), that are useful to architects, engineers, contractors, and people building real-time visualization applications for AEC.
- The library is written in a language that supports developer productivity through things like type safety, and which supports code re-use in other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity.

Nothing fit the bill. So we started building this. 

## Geometry
We are often asked whether the Elements library supports the ____ geometry kernel. It does not. Yet. The geometry kernel that we've created for Elements does "flat stuff with holes in it" really well. It's not that we don't think your Nurbs are sexy, it's just that the effort required to support ____ geometry kernel for micro-services running in the cloud is not small. Good geometry kernels are also usually large, expensive, and not open source, so they introduce a lot of concerns which are orthogonal to why we built this library in the first place. If you are interested in using Elements with another geometry library, we love pull requests.

## Donate
Hypar Elements is open source and will remain so **forever**. Your donation will directly support the development of the Hypar Elements. Hypar Elements has been demonstrated to work in Revit add-ins, Unity projects, and as Lambdas running on AWS. Send us a donation and open a feature request telling us what you'd like it to do.  
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=3HBW7BYRSBZYE)

## Relation To IFC
The following tables shows the types available in the Elements library and their corresponding types in IFC.

|Elements Geometry Type|IFC Type|
|--|--|
|Arc|
|Line|IfcLine|
|Polygon|IfcPolyline|
|Polyline|IfcPolyline|
|Vector3|IfcCartesianPoint|
|Transform|IfcAxisPlacement2D|
|Transform|IfcAxisPlacement3D|
||IfcTrimmedCurve|
||IfcBSplineCurve|
||IfcCompositeCurve|
||IfcTrimmedCurve|
||IfcOffsetCurve2D|
||IfcOffsetCurve3D|
||IfcCircle|
||IfcEllipse|

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
|Space|IfcSpace|
||IfcStair|
|Topography||
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
