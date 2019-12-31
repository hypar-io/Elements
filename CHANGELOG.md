# Changelog

## 0.4.2
### Changed
- `Vector3` is now a struct.
- `Color` is now a struct.
- `ProfileServer.GetProfileByName(...)` is now deprecated in favor of `ProfileServer.GetProfileByType(...)`

### Added
- `Bezier`
- `WideFlangeProfileType`
- `HSSPipeProfileType`
- `Curve.MinimumChordLength` static property to allow the user to set the minimum chord length for subdivision of all curves for rendering.
- `Circle`

## 0.4.0
### Changed
- All element types are partial classes with one part of the class generated from its JSON schema.
- `Polygon.Rectangle` constructor no longer takes an origin.
- `Polygon.Clip` now takes an optional additional set of holes.
- `Wall` and `Floor` constructors no longer take collections of `Opening`s.
  - Openings can be added using `wall.Openings.Add(...)`.
- `Polygon` now has more robust checks during construction.
  - All vertices must be coplanar.
  - Zero length segments are not allowed.
  - Self-intersecting segments are not allowed.
- `Solid`, `Face`, `Vertex`, `Edge`, `HalfEdge`, and `Loop` are now marked `internal`.
- `Quaternion` is now marked `internal`.
- `Matrix` is now marked `internal`.
- `SolidConverter` is now marked `internal`.
- `Elements.Serialization.IFC.ModelExtensions` is now marked `internal`.
- All core type property setters are now `public`.
- The `elevation` parameter has been removed from `Floor`. Floor elevation is now set by passing a `Transform` with a Z coordinate.

### Added
- `ModelCurve` - Draw curves in 3D.
- `ModelPoints` - Draw collections of points in 3D.
- `Elements.Generate.TypeGenerator` class.
- `/Schemas` directory.
- Optional `rotation` on `StructuralFraming` constructors.
- `Model` now has Elements property which is `IDictionary<Guid,Element>`.
- `double Vector3.CCW(Vector3 a, Vector3 b, Vector3 c)`
- `bool Line.Intersects(Line l)`
- `Elements.Validators.IValidator` and the `Elements.Validators.Validator` singleton to provide argument validation during construction of user elements.
- `Line.DivideByLength()`
- `Line.DivideByCount()`
- `Ray` class.
- `Vector3.IsZero()`

### Removed
- The empty Dynamo project.
- `ElementType`, `WallType`, `FloorType`, `StructuralFramingType`
- `MaterialLayer`
- `Transform` constructor taking `start` and `end` parameters. The `Transform` constructor which takes an X and a Z axis should now be used.

### Fixed
- Transforms are now consistently right-handed.
- Transforms on curves are now consistently oriented with the +X axis oriented to the "right" and the +Z axis oriented along the inverse of the tangent of the curve.
- Built in materials for displaying transforms are now red, green, and blue. Previously they were all red.
- All classes deriving from `Element` now pass their `id`, `transform`, and `name` to the base constructor.
- Line/plane intersections now return null if the intersection is "behind" the start of the line.
- Beams whose setbacks total more than the length of the beam no longer fail.
- Plane construction no longer fails when the normal vector and the origin vector are "parallel".
- Fixed #209.
- Topography is now serialized to JSON.
- Built in materials now have an assigned Id.

## 0.3.8
### Changed
- Elements representing building components now return positive areas.
- Added Area property to:
Panel
Space
Added Volume property to:
- Floor
- Space
Added positive area calculation to:
- Floor
- Mass
- Added positive Volume calculation to:
- StructuralFraming
- Beam.Volume() throws an InvalidOperationException for non-linear beams.
- Added TODO to support Volume() for all beam curves.

## 0.3.6
### Changed
- Edges are no longer written to the glTF file.
- Large performance improvements made to glTF writing using `Buffer.BlockCopy` and writing buffers directly from tesselation to glTF buffer. 

### Fixed
- Fix #177.

## 0.3.4
### Changed
- Numerous comments were updated for clarity.
- Numerous styling changes were made to the documentation to align with the Hypar brand.

### Fixed
- Fixed an error where vertex colors were not correctly encoded in the glTF.

## 0.3.3
### Fixed
- Fix #173.
- Fix #7.

## 0.3.0
### Changed
- `Element.Id` is now a `Guid`.

### Fixed
- Fix #107.
- Fix #132.
- Fix #137.
- Fix #144.
- Fix #142.

## 0.2.17
### Added
- The `Kernel` singleton has been added to contain all geometry methods for creating solids.

### Fixed
- Fixed an error where, when writing edges to gltf, ushort would be overflowed and wrap back to 0 causing a loop not to terminate. 

## 0.2.16
### Added
- Materials are now serialized to IFC using `IfcStyledItem`.

### Fixed
- Fixed an error where we returned directly after processing child Elements of an `IAggregateElements`, before we could process the parent element.
- Fixed writing of gltf files so that the `.bin` file is located adjacent to the `.gltf`.

## 0.2.15
### Added
- `Space` elements are now serialized to IFC as `IfcSpace`.

## 0.2.5
### Changed
- `IHasOpenings.Openings[]` is now `IHasOpenings.List<Opening>[]`.

### Fixed
- `Opening` elements are now serialized to IFC as `IfcOpeningElement`.

## 0.2.4.4
### Changed
- `Solid.Slice()` has been made internal. It's not yet ready for consumers. See [#103](https://github.com/hypar-io/elements/issues/103)

## 0.2.4.3
### Fixed
- Spaces are now correctly colored. See [#134](https://github.com/hypar-io/elements/issues/134).

## 0.2.4.2
### Added
- Added `ToIfcWall()` extension method to save a `Wall` to an `IfcWall`.
### Fixed
- `Space.Profile` is set in the constructor when a `Space` is constructed with a profile. [#132](https://github.com/hypar-io/elements/pull/132)
- Sub-elements of `IAggregateElements` are now added to the `Model`. [#137](https://github.com/hypar-io/elements/pull/137)

## 0.2.4.1
### Added
- Added `StandardWall`, for walls defined along a curve. `Wall` continues to be for walls defined by a planar profile extruded to a height.
- Added `Polygon.L`.

### Changed
- `Floor` constructors no longer have `material` parameter. Materials are now specified through the `FloorType`.
- `IAggregateElement` is now `IAggregateElements`.
- `Panel` now takes `Polygon` instead of `Vector3[]`.

## 0.2.4
### Changed
- `IGeometry3D` is now `ISolid`.
- `ISolid` (formerly `IGeometry3D`) now contains one solid, not an array of solids.

### Removed
- `Solid.Material`. Elements are now expected to implement the `IMaterial` interface or have an `IElementType<T>` which specifies a material.

## 0.2.3
### Added
- `MaterialLayer`
- `StructuralFramingType` - `StructuralFramingType` combines a `Profile` and a `Material` to define a type for framing elements.

### Changed
- `IProfileProvider` is now `IProfile`
- `IElementTypeProvider` is now `IElementType`
- All structural framing type constructors now take a `StructuralFramingType` in place of a `Profile` and a `Material`.
- All properties serialize to JSON using camel case.
- Many expensive properties were converted to methods.
- A constructor has been added to `WallType` that takes a collection of `MaterialLayer`.

## 0.2.2
### Added
- `Matrix.Determinant()`
- `Matrix.Inverse()`
- `Transform.Invert()`
- `Model.ToIFC()`
- `Elements.Serialization.JSON` namespace.
- `Elements.Serialization.IFC` namespace.
- `Elements.Serialization.glTF` namespace.

### Changed
- Wall constructor which uses a center line can now have a Transform specified.
- `Profile.ComputeTransform()` now finds the first 3 non-collinear points for calculating its plane. Previously, this function would break when two of the first three vertices were co-linear.
- Using Hypar.IFC2X3 for interaction with IFC.
- `Line.Thicken()` now throws an exception when the line does not have the same elevation for both end points.
- `Model.SaveGlb()` is now `Model.ToGlTF()`.

## 0.2.1
### Added
- The `Topography` class has been added.
- `Transform.OfPoint(Vector3 vector)` has been added to transform a vector as a point with translation. This was previously `Transform.OfVector(Vector3 vector)`. All sites previously using `OfVector(...)` are now using `OfPoint(...)`.
- `Material.DoubleSided`
- `Loop.InsertEdgeAfter()`
- `Solid.Slice()`
- `Model.Extensions`
### Changed
- `Transform.OfVector(Vector3 vector)` now does proper vector transformation without translation.
- Attempting to construct a `Vector3` with NaN or Infinite arguments will throw an `ArgumentOutOfRangeException`.


## 0.2.0
### Added
- IFC implementation has begun with `Model.FromIFC(...)`. Support includes reading of Walls, Slabs, Spaces, Beams, and Columns. Brep booleans required for Wall and Slab openings are not yet supported and are instead converted to Polygon openings in Wall and Floor profiles.
- The `Elements.Geometry.Profiles` namespace has been added. All profile servers can now be found here.
- The `Elements.Geometry.Solids` namespace has been added.
- The Frame type has been added to represent a continuous extrusion of a profile around a polygonal perimeter.
- The `ModelTest` base class has been added. Inheriting from this test class enables a test to automatically write its `Model` to a `.glb` file and to serialize and deserialize to/from JSON to ensure the stability of serialization.
- The `Solid.ToGlb` extension method has been added to enable serializing one `Solid` to glTF for testing.
### Changed
- Element identifiers are now of type `long`.
- Breps have been re-implemented in the `Solid` class. Currently only planar trimmed faces are supported.
- Many improvements to JSON serialization have been added, including the ability to serialize breps.
- '{element}.AddParameter' has been renamed to '{element}.AddProperty'.
- The `Hypar.Geometry` namespace is now `Elements.Geometry`.
- The `Hypar.Elements` namespace is now `Elements`.
### Removed
- The `IProfile` interface has been removed.
- The `Extrusion` class and `IBrep` have been replaced with the `Solid` class. The IGeometry interface now returns a `Solid[]`.
- Many uses of `System.Linq` have been removed. 
- Many uses of `IEnumerable<T>` have been replaced with `T[]`.
