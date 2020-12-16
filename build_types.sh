#!/usr/bin/env bash

BASE=$1

echo "Deleting the existing generated classes..."
rm -rf ./Elements/src/Generate

cd Schemas

#GeoJSON
hypar generate-types -u $BASE/Schemas/GeoJSON/Position.json -o ../Elements/src/Generate/GeoJSON

# Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Arc.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/BBox3.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Color.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Curve.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/CurveRepresentation.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Line.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Matrix.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Mesh.json -o ../Elements/src/Generate/Geometry
# hypar generate-types -u $BASE/Schemas/Geometry/MeshRepresentation.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Plane.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/PointsRepresentation.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Polygon.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Polyline.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Profile.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Representation.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/SolidRepresentation.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Transform.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Triangle.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/UV.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Vector3.json -o ../Elements/src/Generate/Geometry
hypar generate-types -u $BASE/Schemas/Geometry/Vertex.json -o ../Elements/src/Generate/Geometry

# Solids
hypar generate-types -u $BASE/Schemas/Geometry/Solids/Extrude.json -o -o ../Elements/src/Generate/Geometry/Solids
hypar generate-types -u $BASE/Schemas/Geometry/Solids/Lamina.json -o ../Elements/src/Generate/Geometry/Solids
hypar generate-types -u $BASE/Schemas/Geometry/Solids/SolidOperation.json -o ../Elements/src/Generate/Geometry/Solids
hypar generate-types -u $BASE/Schemas/Geometry/Solids/Sweep.json -o ../Elements/src/Generate/Geometry/Solids

# Properties
hypar generate-types -u $BASE/Schemas/Properties/NumericProperty.json -o ../Elements/src/Generate/Properties

hypar generate-types -u $BASE/Schemas/ContentElement.json -o ../Elements/src/Generate
hypar generate-types -u $BASE/Schemas/Element.json -o ../Elements/src/Generate
hypar generate-types -u $BASE/Schemas/GeometricElement.json -o ../Elements/src/Generate
hypar generate-types -u $BASE/Schemas/Material.json -o ../Elements/src/Generate
hypar generate-types -u $BASE/Schemas/Model.json -o ../Elements/src/Generate