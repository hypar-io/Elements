## Code Generation
- Elements constructs its primitive types from schemas in the ../Schemas directory. These schemas are provided as JSON schema.
- Elements uses NJsonSchema to generate C# classes from JSON schemas.
- C# classes can be generated using the Hypar CLI's hypar generate-types command. For users of Visual Studio Code, the "CLI Generate Elements" task can be used.
- The default collection type used is System.Collections.Generic.IList.
- Generated classes are marked as partial. You can add constructors using a separate partial class, but remember that those constructors will not be available to other developers unless you share them in a library (ex: a NuGet package).
- The custom class template for the code generator can be found in ./src/Templates.
- Core class definitions are generated as CSharpClassStyle.POCO using NJsonSchema. This results in class definitions without constructors.
- Deserialization into inherited types is handled in two ways:
  - Base types that live in the Elements library are decorated with one or more JsonInheritanceAttribute pointing to their derived types.
  - External types that inherit from Element must be decorated with the UserElement attribute. This is required because a type author doesn't have access to the base types, and must therefore signify to the serializer that it needs to load a specific type.