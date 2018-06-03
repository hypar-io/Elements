# Hypar Elements
The Hypar Elements library is a highly opinionated library for creating building elements like beams and slabs. It is intended to be easy enough to use for beginning developers, but extensible enough for experienced developers. Elements in Hypar can be serialized to JSON, and their 3D representations can be serialized to binary [glTF](https://www.khronos.org/gltf/) files. Serialization to IFC will be added in the near future.

## Creating a project to use Hypar
- Install [.net](https://www.microsoft.com/net/) - Hypar is compatible with .net standard 2.1.
- Create a dotnet class lib.  
`dotnet new classlib -n MyNewLib`
- Install Hypar.  
`nuget install hypar`

## Examples
The best examples are those provided in the tests, where we demonstrate usage of almost every function in the library.

## Third Party Libraries

[LibTessDotNet](https://github.com/speps/LibTessDotNet)

