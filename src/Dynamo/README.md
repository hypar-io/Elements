This is the Dynamo package for working with Hypar elements in Dynamo.  There are two parts
- An extension to resolve a loading error (see below)
- A library that represents the zero touch nodes themselves.

The extension currently serves only one purpose, which is to resolve a .dll library reference error.  Currently .Net Framework 4.8 makes a mistake while loading the dll System.ComponentModel.Annotations library, and fails to find a valid version.  We must redirect this linked library loading to a working version version "manually" by intercepting all AssemblyResolve events.

The Zero-Touch node library is mostly a simple wrap around the RevitHyparTools project. That is, it creates nodes that mostly represent calls to logic that lives in the RevitHyparTools code.

Testing
To test these nodes:
- Run the `deploy/deployForTesting.sh` bash script.  It basically removes the old package, runs `dotnet publish` and then copies the newly created package (found in the "deploy" folder) to the Dynamo Revit 2.3 packages folder.
- Then open Revit2020, open Dynamo, and test the nodes.  Optionally you may simply open the graph found in the "test" folder adjacent to "src" which contains sample dynamo graphs.