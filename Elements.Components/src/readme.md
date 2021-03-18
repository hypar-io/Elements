# Elements.Components

### About
This library is a work-in-progress set of utilities for working with "Components" — arrangements of elements that can be instantiated into variable boundaries. It is not thoroughly tested or documented, so use at your own risk.

### Concepts
The idea behind components is that you create a `ComponentDefinition`, and then use that definition to create various instances. 

A `ComponentDefinition` is composed of a set of rules, and a set of reference points. The reference points represent the prototypical anchors of the component — usually the corners of a boundary. The rules are any of a whole host of classes which implement `IComponentPlacementRule` — and you are able to implement your own rule classes as well. A `ComponentDefinition` can have multiple rules of different types.

To create actual elements from a `ComponentDefinition`, call `definition.Instantiate(anchors)` with a modified set of anchors. If the definition had 5 anchors, you should also instantiate with 5 anchors, in the same order. `Instantiate()` returns a `ComponentInstance` element, which is really just a thin wrapper around a collection of other elements. If you add a `ComponentInstance` to your function's `Model`, the other elements it contains will get added as well.  

### Rules
The currently available Rule types are:
* `PositionPlacementRule`
  * Place Elements at a fixed displacement relative to an anchor.
* `PolylinePlacementRule`
  * Create a polyline whose vertices move with associated anchors.
* `ArrayPlacementRule`
  * Place an array of elements along a curve that transforms with anchors. (Same curve distortion logic as `PolylinePlacementRule`) 
* `SizeBasedPlacementRule`
  * Choose from among a collection of elements with fixed clearances, and place the first element that fits
* `GridPlacementRule`
  * Create a Grid2d in a distorted boundary, and place elements in resulting grid cells
* `ComponentPlacementRule`
  * Instantiate a sub-component (another `ComponentDefinition`) inside the parent component. 


Note: Many Rules have a `FromClosestPoints()` method, which simplifies setup, by automatically associating the closest anchor with each point on the geometry.


### Example Usage
```csharp
// establish reference points for rule definition.
// here a 20x20 rectangle with its corners and the centerpoint.
var referencePoints = new List<Vector3> {
    new Vector3(0,0,0),
    new Vector3(20,0,0),
    new Vector3(20,20,0),
    new Vector3(0, 20,0),
    new Vector3(10,10, 0)
};

// create objects, positioned within the reference boundary

// red cube in the upper-right corner
var red_cube = new Mass(
                new Profile(Polygon.Rectangle(1, 1)),
                7,
                new Material("Red", new Color(1, 0, 0, 1)),
                new Transform(19, 19, 0)
                );

// blue cube in the lower-right corner
var blue_cube = new Mass(
    new Profile(Polygon.Rectangle(1, 1)),
    3,
    new Material("Blue", new Color(0, 0, 1, 1)),
    new Transform(18, 2, 0)
    );

// create a set of position placement rules, using the 
// closest point from each object to the reference points
// to determine each object's anchor.
var rules = PositionPlacementRule.FromClosestPoints(new[] { red_cube, blue_cube }, referencePoints);

// define component
var definition = new ComponentDefinition(rules, referencePoints.ToList());

// create a target boundary, following the same vertex order as the 
// reference boundary:
var targetPoints = new List<Vector3> {
    new Vector3(0,0,0),
    new Vector3(30,0,0),
    new Vector3(30,20,0),
    new Vector3(0, 20,0),
    new Vector3(15,10, 0)
};

// instantiate the component
var instance = definition.Instantiate(targetPoints);

// add to the model
Model.AddElement(instance);
```


For more examples, be sure to check out the tests! 