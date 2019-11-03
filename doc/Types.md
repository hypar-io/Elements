# Types

## NOTE: The following functionality is only available as of version 0.4.0.

Let's say that you're a mechanical engineer and you want to introduce a new type to Elements called "VAV Box". To introduce this new type, first you need to design it. Where will it fit in the existing Elements type hierarchy? Will an existing base type be used? If so, will the base type need to be changed? Once that's figured out you'll need to write code that represents your type (in the case of Elements as a C# class) and propose that change to the maintainers of the library. The maintainers approve the change and your "VAV Box" class is made availble in the next release. Any small iteration on the design of your type based on use or user feedback requires that you repeat the whole cycle. 

This scenario creates several challenges. First, it requires direct interaction with the maintainers of the Elements library. Most AEC developers don't know how to propose a change to the Elements code and requiring that they undertake our code review process just to introduce a container for data that represents a thing they know is challenging. Second, the maintainers of Elements now "own" the "VAV Box" type and are therefore required to make sure that they don't break it in the future. Imagine how many hundreds of thousands, or potentially millions, of different types of components are in a building. It's not reasonable to assume that one library is going to be able to encapsulate them all. Finally, although having the base description for a type be C# code works well in the short term, it limits the Elements' team's flexibility in the future to work with other languages. 

To address these challenges we've introduced the ability for an AEC developer to create a schema which describes their type using [JSON schema](https://json-schema.org/), and to generate source code from that schema. Schemas can be shared and extended allowing for the growth of a type system from the community.

## Getting Started

### Core Concepts

#### Element
The primary concept behind Elements types is the `Element`. An `Element` is the base type for all things that you will create. It has a unique identifier and a name. That's it. Everything else will be added by you.

#### Primitives
An Element is extended by adding properties to a schema. The properties can be of the following types.

- **Curve**
  - **Arc** - An arc defined by a center and a radius.
  - **Line** - A line defined by a start and end points.
  - **Polygon** - A collection of vertices describing an enclosed polygonal shape.
- **Material** - A material specified using the [physically based rendering](https://en.wikipedia.org/wiki/Physically_based_rendering) model.
- **NumericProperty** - A property value with a unit type.
- **Plane** - A plane described by its origin and normal vector.
- **Profile** - A composite type containing a perimeter `Polygon` and a collection of `Polygon`
- **Representation** - A container for solid operations like `Extrude` and `Sweep`.
- **Transform** - A right-handed coordinate system with +Z "up".
- **Vector3** - A vector with x, y, and z components.

### Create a Type
The first step is to define a schema that represents your type. Good examples for what a schema looks like can be found in the Hypar base schemas. Here's the schema for [`GeometricElement`](https://hypar.io/Schemas/GeometricElement.json), a type which extends `Element` to include a `Transform` and a `Representation`. 

JSON schemas can be authored in any text editor, although an editor with good JSON schema support, like [Visual Studio Code](https://code.visualstudio.com/), is recommended. Good editors have built in JSON schema validation and code completion. You can also use an [online validator](https://www.jsonschemavalidator.net/).

#### An Example Beam
The following schema describes a simple beam with a center line and a cross-section profile. Note that using the `allOf` field, we can inherit from `GeometricElement` so that a Beam will extend that base type.
```json
{
    "$id": "https://hypar.io/Schemas/Beam.json",
    "$schema": "http://json-schema.org/draft-07/schema#",
    "description": "A beam.",
    "title": "Beam",
    "x-namespace": "Elements",
    "type": ["object", "null"],
    "allOf": [{"$ref": "https://hypar.io/Schemas/GeometricElement.json"}],
    "required": ["CenterLine", "Profile"],
    "properties": {
        "CenterLine": {
            "description": "The center line of the beam.",
            "$ref": "https://hypar.io/Schemas/Geometry/Line.json"
        },
        "Profile": {
            "description": "The beam's cross section.",
            "$ref": "https://hypar.io/Schemas/Geometry/Profile.json"
        }
    },
    "additionalProperties": false
}
```

#### Generate Code for your Type
The [Hypar CLI](https://www.nuget.org/packages/Hypar.CLI/0.4.0) can be used to generate code for your type. 
```bash
hypar generate-types -u ./beam.json -o ./Structural
```

#### Generate Code for your Hypar Function
You can specify that your Hypar function requires additional types by declaring those types in the `element_types` property of the `hypar.json`. The `element_types` array contains uris, either as relative file paths or urls to JSON schemas representing types. When you run `hypar init` code will be generated from each of the schemas specified in the `element_types` array and placed in your function's `/src` directory. 