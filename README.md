# Hypar Elements
The Hypar Elements library is a highly opinionated library for creating building elements like beams and slabs. It is intended to be easy enough to use for beginning developers, but extensible enough for experienced developers.

An Element's 3D representations can be serialized to binary [glTF](https://www.khronos.org/gltf/) files.

## Creating a project that uses the Hypar Elements library.
- Install [.net](https://www.microsoft.com/net/) - Hypar Elements is compatible with .net standard 2.1.
- From the command line...  
```
dotnet new classlib -n MyNewLib
cd MyNewLib
dotnet add MyNewLib.csproj package hypar
```

## Examples
The best examples are those provided in the [tests](https://github.com/hypar-io/elements/tree/master/test), where we demonstrate usage of almost every function in the library.

## Third Party Libraries

[LibTessDotNet](https://github.com/speps/LibTessDotNet)

