# Elements.Geometry

The Geometry capabilities in the Elements library are often described as being really good at creating "flat stuff with holes in it." As of version 0.2.0, the Elements library has added the `Solid` type with support for creating solids by sweeping faces. The `IGeometry3D` interface is implemented by element types which have 3D geometry for visualization.

## Rules of Thumb
- All units are SI units (meters), with the exception of angular measurements which are specified in degrees.
- All values are represented using `double` precision (64bits). 
- The coordinate system is [right-handed](http://mathworld.wolfram.com/Right-HandedCoordinateSystem.html).

## Types
- `Vector3` A 3D vector. The `Vector3` class is used for representing both points and vectors throughout Elements.
- `ICurve` An interface implemented by all curves:
  - `Line` A line between two points.
  - `Arc` An arc on a plane through a given number of degrees.
  - `Polyline` a set of contiguous vertices connected by linear segments.
  - `Polygon` a closed `Polyline`.
- `Plane` A plane defined by an origin, X, and Z axes.
- `Matrix` A column-ordered 4x4 matrix used by the `Transform` class to represent translation, rotation, and scale.
- `Transform` A coordinate system. 
- `Quaternion` A [quaternion](https://en.wikipedia.org/wiki/Quaternion).
- `Mesh` an indexed mesh class where vertices and indices are described by flat arrays of `double` and `int` respectively.
- `Color` and RGBA color with components specified in the range 0.0 to 1.0.
- `Solid` A [boundary representation](https://en.wikipedia.org/wiki/Boundary_representation) solid.
  - `Face` A face of the solid defined by a `Loop` of `HalfEdge`.
  - `Vertex` A vertex of a solid.
  - `Edge` An edge of a solid.
  - `HalfEdge` A half edge of an edge. Used to define a `Loop`.
- `BBox3` A 3D, axis-aligned, bounding box.

## Roadmap

### 0.2.1
- `Solid.Split()`

