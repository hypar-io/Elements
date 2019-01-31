# Changelog

## [0.2.0] - 2019-01-31
### Added
- IFC implementation has begun with `Model.FromIFC(...)`. Support includes reading of Walls, Slabs, Spaces, Beams, and Columns. Brep booleans required for Wall and Slab openings are not yet supported and are instead converted to Polygon openings in Wall and Floor profiles.
- The `Elements.Geometry.Profiles` namespace has been added. All profile servers can now be found here.
- The `Elements.Geometry.Solids` namespace has been added.
- The Frame type has been added to represent a continuous extrusion of a profile around a polygonal perimeter.
### Changed
- Element identifiers are now of type `long`.
- Breps have been re-implemented in the `Solid` class. Currently only planar trimmed faces are supported.
### Removed
- The `IProfile` interface has been removed.
- The `Extrusion` class and `IBrep` have been replaced with the `Solid` class. The IGeometry interface now returns a `Solid[]`.
- Many uses of `System.Linq` have been removed. 
- Many uses of `IEnumerable<T>` have been replaced with `T[]`.