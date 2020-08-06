This is the Dynamo package for working with Hypar elements in Dynamo.  
There are two parts:
- A view extension used to resolve a loading error (see below).
- A library that represents the zero touch nodes themselves.

The extension currently serves only one purpose, which is to resolve a .dll library reference error.  Currently .Net Framework 4.8 makes a mistake while loading the dll System.ComponentModel.Annotations library, and fails to find a valid version.  We must redirect this linked library loading to a working version version "manually" by intercepting all AssemblyResolve events.

The Zero-Touch node library is mostly a simple wrapper around the [RevitHyparTools project](https://github.com/hypar-io/Elements/tree/master/src/Revit/RevitHyparTools). That is, it creates nodes that mostly represent calls to logic that lives in the RevitHyparTools code.

Testing
To test these nodes:
- run `dotnet publish`
- Open Revit2020, open Dynamo, and test the nodes.  
- Option to test the `dyn` files found in the "test" folder adjacent to "src".