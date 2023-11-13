using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using IFC;

namespace Elements.Serialization.IFC
{
    /// <summary>
    /// Extension methods for converting elements to IFC entities.
    /// </summary>
    internal static class ElementsExtensions
    {
        internal static List<IfcProduct> ToIfcProducts(this Element e,
                                                       IfcRepresentationContext context,
                                                       Document doc,
                                                       Dictionary<Guid, List<IfcStyleAssignmentSelect>> styleAssignments,
                                                       bool updateElementRepresentation = true)
        {
            var products = new List<IfcProduct>();

            IfcProductDefinitionShape shape = null;
            GeometricElement geoElement = null;
            Transform trans = null;
            Guid id = default(Guid);

            if (e is ElementInstance)
            {
                // If we're using an element instance, get the transform
                // and the id and use those to uniquely position and
                // identify the element.
                var instance = (ElementInstance)e;
                geoElement = instance.BaseDefinition;
                id = instance.Id;
                trans = instance.Transform;

                if (geoElement is IHasOpenings geoElementWithOpenings)
                {
                    for (int i = 0; i < geoElementWithOpenings.Openings.Count; i++)
                    {
                        Opening opening = geoElementWithOpenings.Openings[i];
                        var transform = opening.Transform.Concatenated(trans);
                        var newOpening = new Opening(opening.Perimeter, opening.DepthFront, opening.DepthBack, transform,
                            opening.Representation, opening.IsElementDefinition, default, opening.Name);

                        ToIfcProducts(newOpening, context, doc, styleAssignments, updateElementRepresentation);

                        geoElementWithOpenings.Openings[i] = newOpening;
                    }
                }
            }
            else if (e is GeometricElement)
            {
                // If we've go a geometric element, use its properties as-is.
                geoElement = (GeometricElement)e;
                id = geoElement.Id;
                trans = geoElement.Transform;
            }

            if (updateElementRepresentation)
            {
                geoElement.UpdateRepresentations();
            }

            var localPlacement = trans.ToIfcLocalPlacement(doc);
            doc.AddEntity(localPlacement);

            var geoms = new List<IfcRepresentationItem>();

            if (geoElement is MeshElement meshEl)
            {
                var lengths = meshEl.Mesh.Vertices.Select(v => v.Position.ToArray().Select(vi => new IfcLengthMeasure(vi)).ToList()).ToList();
                var pts = new IfcCartesianPointList3D(lengths);
                doc.AddEntity(pts);
                var indices = meshEl.Mesh.Triangles.Select(t => t.Vertices.Select(vx => new IfcPositiveInteger(vx.Index + 1)).ToList()).ToList();
                var idxs = new List<List<IfcPositiveInteger>>(indices);
                var geom = new IfcTriangulatedFaceSet(pts, indices);
                geom.Closed = false;
                doc.AddEntity(geom);
                geoms.Add(geom);
                shape = ToIfcProductDefinitionShape(geoms, "Tessellation", context, doc);
            }
            else
            {
                if (geoElement.Representation != null)
                {
                    foreach (var op in geoElement.Representation?.SolidOperations)
                    {
                        var ifcRepresentations = AddSolidOperationToDocument(doc, op);
                        foreach (var geom in ifcRepresentations)
                        {
                            List<IfcStyleAssignmentSelect> styles = null;
                            if (geoElement.Material != null)
                            {
                                styleAssignments.TryGetValue(geoElement.Material.Id, out styles);
                            }

                            var styledItem = new IfcStyledItem(geom, styles, null);
                            doc.AddEntity(styledItem);
                        }
                        geoms.AddRange(ifcRepresentations);
                    }
                }
                if (geoElement.RepresentationInstances != null)
                {
                    foreach (var representationInstance in geoElement.RepresentationInstances)
                    {
                        if (representationInstance.IsDefault && representationInstance.Representation is SolidRepresentation solidRepresentation)
                        {
                            List<IfcStyleAssignmentSelect> styles = null;
                            if (representationInstance.Material != null)
                            {
                                styleAssignments.TryGetValue(representationInstance.Material.Id, out styles);
                            }
                            foreach (var op in solidRepresentation.SolidOperations)
                            {
                                var ifcRepresentations = AddSolidOperationToDocument(doc, op);
                                foreach (var geom in ifcRepresentations)
                                {
                                    var styledItem = new IfcStyledItem(geom, styles, null);
                                    doc.AddEntity(styledItem);
                                }
                                geoms.AddRange(ifcRepresentations);
                            }
                        }
                    }
                }
                shape = ToIfcProductDefinitionShape(geoms, "SolidModel", context, doc);
                doc.AddEntity(shape);
            }


            // Can we use IfcMappedItem?
            // https://forums.buildingsmart.org/t/can-tessellation-typed-representation-hold-items-from-another-group/1621
            // var rep = new IfcShapeRepresentation(context, "Body", "Solids", geoms);
            // doc.AddEntity(rep);
            // var axisPt = Vector3.Origin.ToIfcCartesianPoint();
            // doc.AddEntity(axisPt);
            // var axis = new IfcAxis2Placement2D(axisPt);
            // doc.AddEntity(axis);
            // var repMap = new IfcRepresentationMap(new IfcAxis2Placement(axis), rep);
            // doc.AddEntity(repMap);
            // var x = trans.XAxis.ToIfcDirection();
            // var y = trans.YAxis.ToIfcDirection();
            // var z = trans.ZAxis.ToIfcDirection();
            // var origin = trans.Origin.ToIfcCartesianPoint();
            // var cart = new IfcCartesianTransformationOperator3D(x, y, origin, trans.XAxis.Length(), z);
            // doc.AddEntity(x);
            // doc.AddEntity(y);
            // doc.AddEntity(z);
            // doc.AddEntity(origin);
            // doc.AddEntity(cart);
            // var mappedItem = new IfcMappedItem(repMap, cart);
            // doc.AddEntity(mappedItem);
            // var shapeRep= new IfcShapeRepresentation(context, new List<IfcRepresentationItem>(){mappedItem});
            // doc.AddEntity(shapeRep);
            // shape = new IfcProductDefinitionShape(new List<IfcRepresentation>(){shapeRep});
            // doc.AddEntity(shape);

            var product = ConvertElementToIfcProduct(id, geoElement, localPlacement, shape);
            products.Add(product);
            doc.AddEntity(product);

            var ifcOpenings = doc.AllEntities.Where(ent => ent.GetType() == typeof(IfcOpeningElement)).Cast<IfcOpeningElement>();

            // If the element has openings, make opening relationships in
            // the IfcElement.
            if (geoElement is IHasOpenings openings)
            {
                if (openings.Openings.Count > 0)
                {
                    foreach (var o in openings.Openings)
                    {
                        var element = (IfcElement)product;
                        // TODO: Find the opening that we've already created that relates here
                        var opening = ifcOpenings.First(ifcO => ifcO.GlobalId == IfcGuid.ToIfcGuid(o.Id));
                        var voidRel = new IfcRelVoidsElement(IfcGuid.ToIfcGuid(Guid.NewGuid()), element, opening);
                        element.HasOpenings.Add(voidRel);
                        doc.AddEntity(voidRel);
                    }
                }
            }

            return products;
        }

        private static List<IfcRepresentationItem> AddSolidOperationToDocument(Document doc, SolidOperation op)
        {
            var ifcRepresentations = new List<IfcRepresentationItem>();
            if (op is Sweep sweep)
            {
                // Neither of these entities, which are part of the 
                // IFC4 specification, and which would allow a sweep 
                // along a curve, are supported by many applications 
                // which are supposedly IFC4 compliant (Revit). For
                // Those applications where these entities appear,
                // the rotation of the profile is often wrong or 
                // inconsistent.
                // geom = sweep.ToIfcSurfaceCurveSweptAreaSolid(doc);
                // geom = sweep.ToIfcFixedReferenceSweptAreaSolid(geoElement.Transform, doc);

                // Instead, we'll divide the curve and create a set of 
                // linear extrusions instead.
                Polyline pline;
                if (sweep.Curve is Line)
                {
                    pline = sweep.Curve.ToPolyline(1);
                }
                else
                {
                    pline = sweep.Curve.ToPolyline();
                }
                foreach (var segment in pline.Segments())
                {
                    var position = segment.TransformAt(0.0).ToIfcAxis2Placement3D(doc);
                    var extrudeDepth = segment.Length();
                    var extrudeProfile = sweep.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
                    var extrudeDirection = Vector3.ZAxis.Negate().ToIfcDirection();
                    var geom = new IfcExtrudedAreaSolid(extrudeProfile, position,
                        extrudeDirection, new IfcPositiveLengthMeasure(extrudeDepth));

                    doc.AddEntity(extrudeProfile);
                    doc.AddEntity(extrudeDirection);
                    doc.AddEntity(position);
                    doc.AddEntity(geom);
                    ifcRepresentations.Add(geom);
                }
            }
            else if (op is Extrude extrude)
            {
                var geom = extrude.ToIfcExtrudedAreaSolid(doc);
                doc.AddEntity(geom);
                ifcRepresentations.Add(geom);
            }
            else if (op is Lamina lamina)
            {
                var geom = lamina.ToIfcShellBasedSurfaceModel(doc);
                doc.AddEntity(geom);
                ifcRepresentations.Add(geom);
            }
            else
            {
                throw new Exception("Only IExtrude, ISweepAlongCurve, and ILamina representations are currently supported.");
            }

            return ifcRepresentations;
        }

        private static IfcOpeningElement ToIfc(this Opening opening, Guid id, IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcOpening = new IfcOpeningElement(IfcGuid.ToIfcGuid(id),
                                                   null,
                                                   CreateIfcSafeLabelString(opening.Name),
                                                   null,
                                                   null,
                                                   localPlacement,
                                                   shape,
                                                   null,
                                                   IfcOpeningElementTypeEnum.OPENING);
            return ifcOpening;
        }

        private static IfcDoor ToIfc(this Door door, Guid id, IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcDoor = new IfcDoor(IfcGuid.ToIfcGuid(id),
                null,
                CreateIfcSafeLabelString(door.Name),
                null,
                null,
                localPlacement,
                shape,
                null,
                new IfcPositiveLengthMeasure(new IfcLengthMeasure(door.ClearHeight)),
                new IfcPositiveLengthMeasure(new IfcLengthMeasure(door.ClearWidth)),
                IfcDoorTypeEnum.DOOR,
                door.GetIfcDoorTypeOperation(),
                null
            );

            return ifcDoor;
        }

        private static IfcDoorTypeOperationEnum GetIfcDoorTypeOperation(this Door door)
        {
            if (door.OpeningType == DoorOpeningType.SingleSwing)
            {
                switch (door.OpeningSide)
                {
                    case DoorOpeningSide.LeftHand:
                        return IfcDoorTypeOperationEnum.SINGLE_SWING_LEFT;
                    case DoorOpeningSide.RightHand:
                        return IfcDoorTypeOperationEnum.SINGLE_SWING_RIGHT;
                    case DoorOpeningSide.DoubleDoor:
                        return IfcDoorTypeOperationEnum.DOUBLE_DOOR_SINGLE_SWING;
                }
            }
            else if (door.OpeningType == DoorOpeningType.DoubleSwing)
            {
                switch (door.OpeningSide)
                {
                    case DoorOpeningSide.LeftHand:
                        return IfcDoorTypeOperationEnum.DOUBLE_SWING_LEFT;
                    case DoorOpeningSide.RightHand:
                        return IfcDoorTypeOperationEnum.DOUBLE_SWING_RIGHT;
                    case DoorOpeningSide.DoubleDoor:
                        return IfcDoorTypeOperationEnum.DOUBLE_DOOR_DOUBLE_SWING;
                }
            }


            return IfcDoorTypeOperationEnum.NOTDEFINED;
        }

        internal static IfcLocalPlacement ToIfcLocalPlacement(this Transform transform, Document doc, IfcObjectPlacement parent = null)
        {
            var placement = transform.ToIfcAxis2Placement3D(doc);
            var localPlacement = new IfcLocalPlacement(new IfcAxis2Placement(placement));
            if (parent != null)
            {
                localPlacement.PlacementRelTo = parent;
            }

            doc.AddEntity(placement);
            return localPlacement;
        }

        internal static IfcExtrudedAreaSolid ToIfcExtrudedAreaSolid(this Extrude extrude, Document doc)
        {
            // Add local transform to the IFC placement
            var transform = extrude.LocalTransform ?? new Transform();
            var position = transform.ToIfcAxis2Placement3D(doc);

            var extrudeDepth = extrude.Height;
            var extrudeProfile = extrude.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
            var extrudeDirection = extrude.Direction.ToIfcDirection(); ;

            var solid = new IfcExtrudedAreaSolid(extrudeProfile, position,
                extrudeDirection, new IfcPositiveLengthMeasure(extrude.Height));

            doc.AddEntity(extrudeProfile);
            doc.AddEntity(extrudeDirection);
            doc.AddEntity(position);
            doc.AddEntity(solid);

            return solid;
        }

        private static IfcBoundedCurve ToIfcCurve(this ICurve curve, Document doc)
        {
            if (curve is Line line)
            {
                return line.ToIfcTrimmedCurve(doc);
            }
            else if (curve is Arc arc)
            {
                return arc.ToIfcTrimmedCurve(doc);
            }
            // Test Polygon before Polyline to avoid
            // Polygons being treated as Polylines.
            else if (curve is Polygon polygon)
            {
                return polygon.ToIfcPolyline(doc);
            }
            else if (curve is Polyline polyline)
            {
                return polyline.ToIfcPolyline(doc);
            }
            else
            {
                throw new Exception($"The curve type, {curve.GetType()}, is not yet supported.");
            }
        }

        private static IfcProduct ConvertElementToIfcProduct(Guid id, GeometricElement element, IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            try
            {
                IfcProduct e = null;
                if (element is Beam beam)
                {
                    e = beam.ToIfc(id, localPlacement, shape);
                }
                else if (element is Brace brace)
                {
                    e = brace.ToIfc(id, localPlacement, shape);
                }
                else if (element is Column column)
                {
                    e = column.ToIfc(id, localPlacement, shape);
                }
                else if (element is StandardWall wall)
                {
                    e = wall.ToIfc(id, localPlacement, shape);
                }
                else if (element is Wall wall1)
                {
                    e = wall1.ToIfc(id, localPlacement, shape);
                }
                else if (element is Floor floor)
                {
                    e = floor.ToIfc(id, localPlacement, shape);
                }
                else if (element is Door door)
                {
                    e = door.ToIfc(id, localPlacement, shape);
                }
                else if (element is Space space)
                {
                    e = space.ToIfc(id, localPlacement, shape);
                }
                else if (element is Panel panel)
                {
                    e = panel.ToIfc(id, localPlacement, shape);
                }
                else if (element is Mass mass)
                {
                    e = mass.ToIfc(id, localPlacement, shape);
                }
                else if (element is Opening opening)
                {
                    e = opening.ToIfc(id, localPlacement, shape);
                }
                else
                {
                    e = element.ToIfc(id, localPlacement, shape);
                }
                return e;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine($"{element.GetType()} cannot be serialized to IFC.");
            }
            return null;
        }

        private static IfcFixedReferenceSweptAreaSolid ToIfcFixedReferenceSweptAreaSolid(this Sweep sweep, Transform transform, Document doc)
        {
            var position = transform.ToIfcAxis2Placement3D(doc);
            var sweptArea = sweep.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
            var directrix = sweep.Curve.ToIfcCurve(doc);
            var refDir = sweep.Curve.TransformAt(0.0).XAxis.ToIfcDirection();
            var solid = new IfcFixedReferenceSweptAreaSolid(sweptArea, position, directrix, 0, 1, refDir);

            doc.AddEntity(refDir);
            doc.AddEntity(position);
            doc.AddEntity(sweptArea);
            doc.AddEntity(directrix);

            doc.AddEntity(solid);
            return solid;
        }

        private static IfcSurfaceCurveSweptAreaSolid ToIfcSurfaceCurveSweptAreaSolid(this Sweep sweep, Document doc)
        {
            var position = new Transform().ToIfcAxis2Placement3D(doc);
            var sweptArea = sweep.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
            var directrix = sweep.Curve.ToIfcCurve(doc);
            var profile = new IfcArbitraryOpenProfileDef(IfcProfileTypeEnum.CURVE, directrix);

            var extrudeDir = Vector3.ZAxis.ToIfcDirection();
            var extrudeSurfPosition = new Transform(0, 0, -100).ToIfcAxis2Placement3D(doc);
            doc.AddEntity(extrudeSurfPosition);

            var surface = new IfcSurfaceOfLinearExtrusion(profile, position, extrudeDir, 100);

            // You must use the version of this constructor that has position, startParam,
            // and endParam. If you don't, ArchiCAD (and possibly others) will call
            // the geometry invalid.
            var solid = new IfcSurfaceCurveSweptAreaSolid(sweptArea, position, directrix, 0, 1, surface);

            doc.AddEntity(position);
            doc.AddEntity(sweptArea);
            doc.AddEntity(directrix);

            doc.AddEntity(extrudeDir);
            doc.AddEntity(profile);

            doc.AddEntity(surface);
            doc.AddEntity(solid);

            return solid;
        }

        private static IfcShellBasedSurfaceModel ToIfcShellBasedSurfaceModel(this Lamina lamina, Document doc)
        {
            var plane = lamina.Perimeter.Plane().ToIfcPlane(doc);
            var outer = lamina.Perimeter.ToIfcCurve(doc);
            var bplane = new IfcCurveBoundedPlane(plane, outer, new List<IfcCurve> { });

            var bounds = new List<IfcFaceBound> { };
            var loop = lamina.Perimeter.ToIfcPolyLoop(doc);
            var faceBounds = new IfcFaceBound(loop, true);
            bounds.Add(faceBounds);

            var face = new IfcFaceSurface(bounds, bplane, true);
            var openShell = new IfcOpenShell(new List<IfcFace> { face });

            var shell = new IfcShell(openShell);
            var ssm = new IfcShellBasedSurfaceModel(new List<IfcShell> { shell });

            doc.AddEntity(plane);
            doc.AddEntity(outer);
            doc.AddEntity(bplane);
            doc.AddEntity(loop);
            doc.AddEntity(faceBounds);
            doc.AddEntity(face);
            doc.AddEntity(openShell);

            return ssm;
        }

        private static Plane ToPlane(this ICurve curve)
        {
            if (curve is Line l)
            {
                if (l.Direction().Equals(Vector3.ZAxis))
                {
                    return new Plane(l.Start, Vector3.ZAxis);
                }

                var normal = l.Direction().Cross(Vector3.ZAxis);
                return new Plane(l.Start, normal);

            }
            else if (curve is Arc arc)
            {
                return arc.Plane();
            }
            else if (curve is Polyline polyline)
            {
                return polyline.Plane();
            }
            else if (curve is Polygon polygon)
            {
                return polygon.Plane();
            }
            else
            {
                throw new Exception($"The curve type, {curve.GetType()}, is not yet supported.");
            }
        }

        private static IfcProductDefinitionShape ToIfcProductDefinitionShape(this List<IfcRepresentationItem> geoms, string shapeType, IfcRepresentationContext context, Document doc)
        {
            var rep = new IfcShapeRepresentation(context, "Body", shapeType, geoms);
            var shape = new IfcProductDefinitionShape(new List<IfcRepresentation> { rep });

            doc.AddEntity(rep);

            return shape;
        }

        private static IfcBuildingElementProxy ToIfc(this GeometricElement element, Guid id, IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var proxy = new IfcBuildingElementProxy(IfcGuid.ToIfcGuid(id),
                                                    null,
                                                    CreateIfcSafeLabelString(element.Name),
                                                    $"A {element.GetType().Name} created in Hypar.",
                                                    $"{element.GetType().FullName}",
                                                    localPlacement,
                                                    shape,
                                                    null,
                                                    IfcBuildingElementProxyTypeEnum.ELEMENT);
            return proxy;
        }

        private static IfcSlab ToIfc(this Floor floor, Guid id,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var slab = new IfcSlab(IfcGuid.ToIfcGuid(id),
                                   null,
                                   CreateIfcSafeLabelString(floor.Name),
                                   null,
                                   null,
                                   localPlacement,
                                   shape,
                                   null,
                                   IfcSlabTypeEnum.FLOOR);
            return slab;
        }

        // TODO: There is a lot of duplicate code used to create products.
        // Can we make a generic method like ToIfc<TProduct>()? There are
        // exceptions for which this won't work like IfcSpace.

        private static IfcSpace ToIfc(this Space space, Guid id,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcSpace = new IfcSpace(IfcGuid.ToIfcGuid(id),
                                        null,
                                        CreateIfcSafeLabelString(space.Name),
                                        null,
                                        null,
                                        localPlacement,
                                        shape,
                                        null,
                                        IfcElementCompositionEnum.ELEMENT,
                                        IfcSpaceTypeEnum.NOTDEFINED,
                                        new IfcLengthMeasure(space.Transform.Origin.Z));
            return ifcSpace;
        }

        private static IfcProduct ToIfc(this StandardWall wall, Guid id,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcWall = new IfcWallStandardCase(IfcGuid.ToIfcGuid(id),
                                                  null,
                                                  CreateIfcSafeLabelString(wall.Name),
                                                  null,
                                                  null,
                                                  localPlacement,
                                                  shape,
                                                  null,
                                                  IfcWallTypeEnum.NOTDEFINED);
            return ifcWall;
        }

        private static IfcWall ToIfc(this Wall wall, Guid id,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcWall = new IfcWall(IfcGuid.ToIfcGuid(id),
                                      null,
                                      CreateIfcSafeLabelString(wall.Name),
                                      null,
                                      null,
                                      localPlacement,
                                      shape,
                                      null,
                                      IfcWallTypeEnum.NOTDEFINED);
            return ifcWall;
        }

        private static IfcBeam ToIfc(this Beam beam, Guid id,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcBeam = new IfcBeam(IfcGuid.ToIfcGuid(id),
                                      null,
                                      CreateIfcSafeLabelString(beam.Name),
                                      null,
                                      null,
                                      localPlacement,
                                      shape,
                                      null,
                                      IfcBeamTypeEnum.BEAM);
            return ifcBeam;
        }

        private static IfcColumn ToIfc(this Column column, Guid id,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var ifcColumn = new IfcColumn(IfcGuid.ToIfcGuid(id),
                                          null,
                                          CreateIfcSafeLabelString(column.Name),
                                          null,
                                          null,
                                          localPlacement,
                                          shape,
                                          null,
                                          IfcColumnTypeEnum.COLUMN);
            return ifcColumn;
        }

        private static IfcMember ToIfc(this Brace column, Guid id,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var member = new IfcMember(IfcGuid.ToIfcGuid(id),
                                       null,
                                       CreateIfcSafeLabelString(column.Name),
                                       null,
                                       null,
                                       localPlacement,
                                       shape,
                                       null,
                                       IfcMemberTypeEnum.NOTDEFINED);
            return member;
        }

        private static IfcPlate ToIfc(this Panel panel, Guid id,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var plate = new IfcPlate(IfcGuid.ToIfcGuid(id),
                                     null,
                                     CreateIfcSafeLabelString(panel.Name),
                                     null,
                                     null,
                                     localPlacement,
                                     shape,
                                     null,
                                     IfcPlateTypeEnum.NOTDEFINED);
            return plate;
        }

        private static IfcBuildingElementProxy ToIfc(this Mass mass, Guid id,
            IfcLocalPlacement localPlacement, IfcProductDefinitionShape shape)
        {
            var proxy = new IfcBuildingElementProxy(IfcGuid.ToIfcGuid(id),
                                                    null,
                                                    CreateIfcSafeLabelString(mass.Name),
                                                    null,
                                                    null,
                                                    localPlacement,
                                                    shape,
                                                    null,
                                                    IfcBuildingElementProxyTypeEnum.ELEMENT);
            return proxy;
        }

        private static IfcLoop ToIfcPolyLoop(this Polygon polygon, Document doc)
        {
            var loop = new IfcPolyLoop(polygon.Vertices.ToIfcCartesianPointList(doc));
            return loop;
        }

        private static List<IfcCartesianPoint> ToIfcCartesianPointList(this IList<Vector3> pts, Document doc)
        {
            var icps = new List<IfcCartesianPoint>();
            foreach (var pt in pts)
            {
                var icp = pt.ToIfcCartesianPoint();
                doc.AddEntity(icp);
                icps.Add(icp);
            }
            return icps;
        }

        private static IfcArbitraryClosedProfileDef ToIfcArbitraryClosedProfileDef(this Polygon polygon, Document doc)
        {
            var pline = polygon.ToIfcPolyline(doc);
            var profile = new IfcArbitraryClosedProfileDef(IfcProfileTypeEnum.AREA, pline);
            doc.AddEntity(pline);
            return profile;
        }

        private static IfcPolyline ToIfcPolyline(this Polygon polygon, Document doc)
        {
            var points = new List<IfcCartesianPoint>();
            foreach (var v in polygon.Vertices)
            {
                var p = v.ToIfcCartesianPoint();
                doc.AddEntity(p);
                points.Add(p);
            }
            // Add the first point to close the curve.
            points.Add(points.First());
            return new IfcPolyline(points);
        }

        private static IfcPolyline ToIfcPolyline(this Polyline polygon, Document doc)
        {
            var points = new List<IfcCartesianPoint>();
            foreach (var v in polygon.Vertices)
            {
                var p = v.ToIfcCartesianPoint();
                doc.AddEntity(p);
                points.Add(p);
            }
            return new IfcPolyline(points);
        }

        private static IfcTrimmedCurve ToIfcTrimmedCurve(this Line line, Document doc)
        {
            var start = line.Start.ToIfcCartesianPoint();
            var end = line.End.ToIfcCartesianPoint();

            var dir = line.Direction().ToIfcVector(doc);

            var ifcLine = new IfcLine(start, dir);
            var trim1 = new IfcTrimmingSelect(start);
            var trim2 = new IfcTrimmingSelect(end);
            var tc = new IfcTrimmedCurve(ifcLine, new List<IfcTrimmingSelect> { trim1 }, new List<IfcTrimmingSelect> { trim2 },
                true, IfcTrimmingPreference.CARTESIAN);

            doc.AddEntity(start);
            doc.AddEntity(end);
            doc.AddEntity(dir);
            doc.AddEntity(ifcLine);

            return tc;
        }

        private static IfcTrimmedCurve ToIfcTrimmedCurve(this Arc arc, Document doc)
        {
            var placement = new Transform().ToIfcAxis2Placement3D(doc);
            var ifcCircle = new IfcCircle(new IfcAxis2Placement(placement), new IfcPositiveLengthMeasure(arc.Radius));
            var trim1 = new IfcTrimmingSelect(arc.StartAngle);
            var trim2 = new IfcTrimmingSelect(arc.EndAngle);
            var tc = new IfcTrimmedCurve(ifcCircle, new List<IfcTrimmingSelect> { trim1 }, new List<IfcTrimmingSelect> { trim2 },
                true, IfcTrimmingPreference.PARAMETER);

            doc.AddEntity(placement);
            doc.AddEntity(ifcCircle);

            return tc;
        }

        private static IfcVector ToIfcVector(this Vector3 v, Document doc)
        {
            var dir = v.ToIfcDirection();
            var vector = new IfcVector(dir, new IfcLengthMeasure(0));
            doc.AddEntity(dir);
            return vector;
        }

        private static IfcAxis2Placement3D ToIfcAxis2Placement3D(this Transform transform, Document doc)
        {
            var origin = transform.Origin.ToIfcCartesianPoint();
            var z = transform.ZAxis.ToIfcDirection();
            var x = transform.XAxis.ToIfcDirection();
            var placement = new IfcAxis2Placement3D(origin,
                z, x);
            doc.AddEntity(origin);
            doc.AddEntity(z);
            doc.AddEntity(x);
            return placement;
        }

        private static IfcDirection ToIfcDirection(this Vector3 direction)
        {
            return new IfcDirection(new List<IfcReal> { new IfcReal(direction.X), new IfcReal(direction.Y), new IfcReal(direction.Z) });
        }

        private static IfcCartesianPoint ToIfcCartesianPoint(this Vector3 point)
        {
            return new IfcCartesianPoint(new List<IfcLengthMeasure> { point.X, point.Y, point.Z });
        }

        private static IfcPlane ToIfcPlane(this Plane plane, Document doc)
        {
            var t = new Transform(plane.Origin, plane.Normal);
            var position = t.ToIfcAxis2Placement3D(doc);
            var ifcPlane = new IfcPlane(position);

            doc.AddEntity(position);

            return ifcPlane;
        }

        internal static IfcColourRgb ToIfcColourRgb(this Color color)
        {
            var red = new IfcNormalisedRatioMeasure(new IfcRatioMeasure(color.Red));
            var green = new IfcNormalisedRatioMeasure(new IfcRatioMeasure(color.Green));
            var blue = new IfcNormalisedRatioMeasure(new IfcRatioMeasure(color.Blue));

            var ifcColor = new IfcColourRgb(red, green, blue);

            return ifcColor;
        }

        private static string CreateIfcSafeLabelString(string label)
        {
            if (label == null)
            {
                return null;
            }

            return label.Replace("'", "''");
        }
    }
}