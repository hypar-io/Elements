# What is Elements?

Elements is a cross-platform library for creating building elements. It's meant to be used by architects, engineers, and other building professionals who want to write code that generates buildings.

When we started [Hypar](https://www.hypar.io) we needed a library that would generate building elements and run at the core of each function on the platform. Because we don't like rebuilding the wheel, we looked around for existing libraries that fulfilled the following requirements:

- The library must be small and fast. Elements is currently ~1mb and we're working every day to make it smaller.
- The library must be cross-platform.
- The library must have great visual documentation. If we're going to pass this library on as a recommendation to developers on Hypar, we want great docs.
- The library must be free of dependencies on host applications like Rhino or Revit or geometry kernels like Open Cascade which, while really cool, become a black box in your system.
- The library must be able to serialize data to formats like JSON, [IFC](https://www.buildingsmart.org/about/what-is-openbim/ifc-introduction/),and [glTF](https://www.khronos.org/gltf/), that are useful to architects, engineers, contractors, and people building real-time visualization applications for AEC.
- The library must be written in a language that supports developer productivity through things like type safety, and which supports code re-use in other popular AEC applications like Dynamo, Grasshopper, Revit, and Unity.
- Serialization and deserialization of types that extend `Element` should be possible provided that those types are made up of primitives defined in this library.

We couldn't find anything quite right. So we started building this.
