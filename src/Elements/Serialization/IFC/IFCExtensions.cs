using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using IFC;
using STEP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Elements.Serialization.IFC
{
    /// <summary>
    /// Extension methods for converting IFC types to Element types.
    /// </summary>
    public static class IFCExtensions
    {
        /// <summary>
        /// Load a model from IFC.
        /// </summary>
        /// <param name="path">The path to an IFC STEP file.</param>
        /// <returns>A model.</returns>
        internal static Model FromIFC(string path)
        {
            List<STEPError> errors;
            var ifcModel = new Document(path, out errors);
            var ifcSlabs = ifcModel.AllInstancesOfType<IfcSlab>();
            var ifcSpaces = ifcModel.AllInstancesOfType<IfcSpace>();
            var ifcWalls = ifcModel.AllInstancesOfType<IfcWallStandardCase>();
            var ifcBeams = ifcModel.AllInstancesOfType<IfcBeam>();
            var ifcColumns = ifcModel.AllInstancesOfType<IfcColumn>();
            var ifcVoids = ifcModel.AllInstancesOfType<IfcRelVoidsElement>();
            var ifcMaterials = ifcModel.AllInstancesOfType<IfcRelAssociatesMaterial>();

            var openings = new List<Opening>();
            foreach (var v in ifcVoids)
            {
                var element = v.RelatingBuildingElement;
                // var elementTransform = element.ObjectPlacement.ToTransform();
                var o = ((IfcOpeningElement)v.RelatedOpeningElement).ToOpening();
                openings.Add(o);
            }

            var wallType = new WallType("Default Wall", 1.0);

            var slabs = ifcSlabs.Select(s => s.ToFloor(ifcVoids.Where(v=>v.RelatingBuildingElement == s).Select(v=>v.RelatedOpeningElement).Cast<IfcOpeningElement>()));
            var spaces = ifcSpaces.Select(sp => sp.ToSpace());
            var walls = ifcWalls.Select(w => w.ToWall(
                ifcVoids.Where(v=>v.RelatingBuildingElement == w).Select(v=>v.RelatedOpeningElement).Cast<IfcOpeningElement>(),
                wallType));
            var beams = ifcBeams.Select(b => b.ToBeam());
            var columns = ifcColumns.Select(c => c.ToColumn());

            var model = new Model();
            model.AddElements(slabs);
            model.AddElements(spaces);
            model.AddElements(walls);
            model.AddElements(beams);
            model.AddElements(columns);
            if (openings.Any())
            {
                model.AddElements(openings);
            }

            return model;
        }
        
        /// <summary>
        /// Write the model to IFC.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="path">The path to the generated IFC STEP file.</param>
        public static void ToIFC(this Model model, string path)
        {
            var ifc = new Document("elements", "elements", Environment.UserName, 
                                    null, null, null, "elements", null, null,
                                    null, null, null, null, null, null
                                    );

            var proj = ifc.AllInstancesOfType<IfcProject>().FirstOrDefault();

            // Add a site
            var site = new IfcSite(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, IfcElementCompositionEnum.ELEMENT);
            var projAggregate = new IfcRelAggregates(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, proj, new List<IfcObjectDefinition>{site});

            // Add building and building storey
            var building = new IfcBuilding(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, IfcElementCompositionEnum.ELEMENT);
            var storey = new IfcBuildingStorey(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, IfcElementCompositionEnum.ELEMENT);
            var aggregate = new IfcRelAggregates(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, building, new List<IfcObjectDefinition>{storey});
            
            // Aggregate the building into the site
            var siteAggregate = new IfcRelAggregates(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, site, new List<IfcObjectDefinition>{building});

            ifc.AddEntity(site);
            ifc.AddEntity(projAggregate);
            ifc.AddEntity(building);
            ifc.AddEntity(storey);
            ifc.AddEntity(aggregate);
            ifc.AddEntity(siteAggregate);

            // Materials
            // foreach(var m in model.Materials.Values)
            // {
            //     var ifcMaterial = new IfcMaterial(m.Name);
            //     ifc.AddEntity(ifcMaterial);
            // }

            var products = new List<IfcProduct>();
            var context = ifc.AllInstancesOfType<IfcGeometricRepresentationContext>().FirstOrDefault();
            foreach(var e in model.Elements.Values)
            {
                if(e is Wall)
                {
                    var w = (Wall)e;
                    var ifcWall = w.ToIfcWallStandardCase(context, ifc);
                    products.Add(ifcWall);
                }

                if(e is Beam)
                {
                    var b = (Beam)e;
                    var ifcBeam = b.ToIfcBeam(context, ifc);
                    products.Add(ifcBeam);
                }
            }

            var spatialRel = new IfcRelContainedInSpatialStructure(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, products, storey);
            ifc.AddEntity(spatialRel);

            if(File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllText(path, ifc.ToSTEP(path));
        }

        private static ElementType ToElementType(this IfcElementType elementType)
        {
            if(elementType is IfcCoveringType)
            {
                throw new NotImplementedException();
            }
            if(elementType is IfcBeamType)
            {
                return ((IfcBeamType)elementType).ToStructuralFramingType();
            }
            if(elementType is IfcMemberType)
            {
                throw new NotImplementedException();
            }
            if(elementType is IfcColumnType)
            {
                return ((IfcColumnType)elementType).ToStructuralFramingType();
            }
            if(elementType is IfcWallType)
            {
                throw new NotImplementedException();
            }
            if(elementType is IfcSlabType)
            {
                return ((IfcSlabType)elementType).ToFloorType();
            }
            if(elementType is IfcStairFlightType)
            {
                throw new NotImplementedException();
            }
            if(elementType is IfcRampFlightType)
            {
                throw new NotImplementedException();
            }
            if(elementType is IfcCurtainWallType)
            {
                throw new NotImplementedException();
            }
            if(elementType is IfcRailingType)
            {
                throw new NotImplementedException();
            }
            if(elementType is IfcBuildingElementProxyType)
            {
                throw new NotImplementedException();
            }
            return null;
        }

        private static StructuralFramingType ToStructuralFramingType(this IfcBeamType beamType)
        {
            throw new NotImplementedException();
        }

        private static StructuralFramingType ToStructuralFramingType(this IfcColumnType columnType)
        {
            throw new NotImplementedException();
        }

        private static FloorType ToFloorType(this IfcSlabType slabType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert an IfcMaterialSelect to a material or a layered material.
        /// </summary>
        /// <param name="materialSelect">An IfcMaterialSelect.</param>
        private static dynamic ToMaterialOrMaterialLayers(this IfcMaterialSelect materialSelect)
        {
            if(materialSelect.Choice is IfcMaterial)
            {
                var m = (IfcMaterial)materialSelect.Choice;
                return m.ToMaterial();
            }
            else if(materialSelect.Choice is IfcMaterialList)
            {
                throw new NotImplementedException("IfcMaterialList is not yet supported.");
            }
            else if (materialSelect.Choice is IfcMaterialLayerSetUsage)
            {
                // Return a material layer set.
                var m = (IfcMaterialLayerSetUsage)materialSelect.Choice;
                return m.ToMaterialLayers();
            }
            else if(materialSelect.Choice is IfcMaterialLayerSet)
            {
                // Return a material layer set.
                var m = (IfcMaterialLayerSet)materialSelect.Choice;
                return m.ToMaterialLayers();
            }
            else if(materialSelect.Choice is IfcMaterialLayer)
            {
                // Return a material layer.
                var m = ((IfcMaterialLayer)materialSelect.Choice).ToMaterialLayer();
            }
            return null;
        }

        private static List<MaterialLayer> ToMaterialLayers(this IfcMaterialLayerSetUsage usage)
        {
            return usage.ForLayerSet.ToMaterialLayers();
        }

        private static List<MaterialLayer> ToMaterialLayers(this IfcMaterialLayerSet set)
        {
            var layers = new List<MaterialLayer>();
            foreach(var s in set.MaterialLayers)
            {
                layers.Add(s.ToMaterialLayer());
            }
            return layers;
        }

        private static MaterialLayer ToMaterialLayer(this IfcMaterialLayer layer)
        {
            return new MaterialLayer(layer.Material.ToMaterial(), (IfcLengthMeasure)layer.LayerThickness);
        }

        private static Material ToMaterial(this IfcMaterial material)
        {
            // IFC materials is a fucking trainwreck. Just return a default.
            return new Material(material.Name, Colors.Gray, 0.0f, 0.0f);
        }

        /// <summary>
        /// Convert a wall to an IfcWallStandardCase
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="context"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static IfcWallStandardCase ToIfcWallStandardCase(this Wall wall, IfcRepresentationContext context, Document doc)
        {
            var sweptArea = wall.CenterLine.Thicken(wall.Thickness()).ToIfcArbitraryClosedProfileDef(doc);
            var extrudeDirection = Vector3.ZAxis.ToIfcDirection();

            // We don't use the Wall's transform for positioning, because
            // our walls have a transform that lays the wall "flat". Just
            // use a identity transform.
            var position = new Transform().ToIfcAxis2Placement3D(doc);
            var repItem = new IfcExtrudedAreaSolid(sweptArea, position, 
                extrudeDirection, new IfcPositiveLengthMeasure(wall.Height));
            var rep = new IfcShapeRepresentation(context, "Body", "SweptSolid", new List<IfcRepresentationItem>{repItem});
            var productRep = new IfcProductDefinitionShape(new List<IfcRepresentation>{rep});
            var id = IfcGuid.ToIfcGuid(Guid.NewGuid());
            var localPlacement = new Transform().ToIfcLocalPlacement(doc);
            var ifcWall = new IfcWallStandardCase(new IfcGloballyUniqueId(id), 
                null, wall.Name, null, null, localPlacement, productRep, null);

            doc.AddEntity(sweptArea);
            doc.AddEntity(extrudeDirection);
            doc.AddEntity(position);
            doc.AddEntity(repItem);
            doc.AddEntity(rep);
            doc.AddEntity(localPlacement);
            doc.AddEntity(productRep);
            doc.AddEntity(ifcWall);

            return ifcWall;
        }

        private static IfcBeam ToIfcBeam(this Beam beam, IfcRepresentationContext context, Document doc)
        {
            var sweptArea = beam.ElementType.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
            var line = (Line)beam.Curve;
            
            // We use the Z extrude direction because the direction is 
            // relative to the local placement, which is a transform at the
            // beam's end with the Z axis pointing along the direction.
            
            var extrudeDirection = Vector3.ZAxis.ToIfcDirection();

            var position = new Transform().ToIfcAxis2Placement3D(doc);
            var repItem = new IfcExtrudedAreaSolid(sweptArea, position, 
                extrudeDirection, new IfcPositiveLengthMeasure(beam.Curve.Length()));
            var localPlacement = beam.Curve.TransformAt(0.0).ToIfcLocalPlacement(doc);
            var rep = new IfcShapeRepresentation(context, "Body", "SweptSolid", new List<IfcRepresentationItem>{repItem});
            var productRep = new IfcProductDefinitionShape(new List<IfcRepresentation>{rep});
            var ifcBeam = new IfcBeam(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, null, null, null, localPlacement, productRep, null);
            
            doc.AddEntity(sweptArea);
            doc.AddEntity(extrudeDirection);
            doc.AddEntity(position);
            doc.AddEntity(repItem);
            doc.AddEntity(rep);
            doc.AddEntity(localPlacement);
            doc.AddEntity(productRep);
            doc.AddEntity(ifcBeam);

            return ifcBeam;
        }

        /// <summary>
        /// Convert a polygon to an IfcArbitraryClosedProfileDef.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static IfcArbitraryClosedProfileDef ToIfcArbitraryClosedProfileDef(this Polygon polygon, Document doc)
        {
            var pline = polygon.ToIfcPolyline(doc);
            var profile = new IfcArbitraryClosedProfileDef(IfcProfileTypeEnum.AREA, pline);
            doc.AddEntity(pline);
            return profile;
        }

        /// <summary>
        /// Convert a polyline to an IfcPolyline.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static IfcPolyline ToIfcPolyline(this Polygon polygon, Document doc)
        {
            var points = new List<IfcCartesianPoint>();
            foreach(var v in polygon.Vertices)
            {
                var p = v.ToIfcCartesianPoint();
                doc.AddEntity(p);
                points.Add(p);
            }
            return new IfcPolyline(points);
        }

        /// <summary>
        /// Convert a transform to an IfcLocalPlacement.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static IfcLocalPlacement ToIfcLocalPlacement(this Transform transform, Document doc)
        {
            var placement = transform.ToIfcAxis2Placement3D(doc);
            var localPlacement = new IfcLocalPlacement(new IfcAxis2Placement(placement));
            doc.AddEntity(placement);
            return localPlacement;
        }

        /// <summary>
        /// Convert a transform to an IfcAxis2Placement3D.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Convert a vector3 to an IfcDirection.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static IfcDirection ToIfcDirection(this Vector3 direction)
        {
            return new IfcDirection(new List<double>{direction.X, direction.Y, direction.Z});
        }

        /// <summary>
        /// Convert a vector3 to an IfcCartesianPoint.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private static IfcCartesianPoint ToIfcCartesianPoint(this Vector3 point)
        {
            return new IfcCartesianPoint(new List<IfcLengthMeasure>{point.X, point.Y, point.Z});
        }

        /// <summary>
        /// Convert an IfcSlab to a Floor.
        /// </summary>
        /// <param name="slab">An IfcSlab.</param>
        /// <param name="openings"></param>
        private static Floor ToFloor(this IfcSlab slab, IEnumerable<IfcOpeningElement> openings)
        {
            var transform = new Transform();
            transform.Concatenate(slab.ObjectPlacement.ToTransform());

            // Check if the slab is contained in a building storey
            foreach (var cis in slab.ContainedInStructure)
            {
                transform.Concatenate(cis.RelatingStructure.ObjectPlacement.ToTransform());
            }

            var repItems = slab.Representation.Representations.SelectMany(r => r.Items);
            if (!repItems.Any())
            {
                throw new Exception("The provided IfcSlab does not have any representations.");
            }

            var solid = slab.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();
            if (solid == null)
            {
                return null;
                // throw new Exception("No IfcExtrudedAreaSolid could be found in the provided IfcSlab.");
            }

            var floorType = new FloorType($"{Guid.NewGuid().ToString()}_floor_type", (IfcLengthMeasure)solid.Depth);
            var outline = (Polygon)solid.SweptArea.ToICurve();
            var solidTransform = solid.Position.ToTransform();
            var floor = new Floor(new Profile(outline), solidTransform, solid.ExtrudedDirection.ToVector3(), 
                floorType, 0, BuiltInMaterials.Default, transform);
            floor.Name = slab.Name;
            
            return floor;
        }

        /// <summary>
        /// Convert and IfcSpace to a Space.
        /// </summary>
        /// <param name="space">An IfcSpace.</param>
        /// <returns></returns>
        private static Space ToSpace(this IfcSpace space)
        {
            var transform = new Transform();

            var repItems = space.Representation.Representations.SelectMany(r => r.Items);
            if (!repItems.Any())
            {
                throw new Exception("The provided IfcSlab does not have any representations.");
            }

            var localPlacement = space.ObjectPlacement.ToTransform();
            transform.Concatenate(localPlacement);
            
            var foundSolid = repItems.First();
            var material = new Material("space", new Color(1.0f, 0.0f, 1.0f, 0.5f), 0.0f, 0.0f);
            if (foundSolid.GetType() == typeof(IfcExtrudedAreaSolid))
            {
                var solid = (IfcExtrudedAreaSolid)foundSolid;
                var profileDef = (IfcArbitraryClosedProfileDef)solid.SweptArea;
                transform.Concatenate(solid.Position.ToTransform());
                var pline = (IfcPolyline)profileDef.OuterCurve;
                var outline = pline.ToPolygon(true);
                var result = new Space(new Profile(outline), (IfcLengthMeasure)solid.Depth, 0.0, material, transform);
                result.Name = space.Name;
                return result;
            }
            else if (foundSolid.GetType() == typeof(IfcFacetedBrep))
            {
                var solid = (IfcFacetedBrep)foundSolid;
                var shell = solid.Outer;
                var newSolid = new Solid(material);
                for(var i=0; i< shell.CfsFaces.Count; i++)
                {
                    var f = shell.CfsFaces[i];
                    foreach (var b in f.Bounds)
                    {
                        var loop = (IfcPolyLoop)b.Bound;
                        var poly = loop.Polygon.ToPolygon();
                        newSolid.AddFace(poly);
                    }
                }
                var result = new Space(newSolid, transform);

                result.Name = space.Name;
                return result;
            }

            return null;
        }

        /// <summary>
        /// Convert an IfcWallStandardCase to a Wall.
        /// </summary>
        /// <param name="wall">An IfcWallStandardCase.</param>
        /// <param name="openings">A collection of IfcOpeningElement belonging to this wall.</param>
        /// <param name="wallType">The wall's wall type.</param>
        private static Wall ToWall(this IfcWallStandardCase wall, 
            IEnumerable<IfcOpeningElement> openings, WallType wallType)
        {
            var transform = new Transform();
            transform.Concatenate(wall.ObjectPlacement.ToTransform());

            // An extruded face solid.
            var solid = wall.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();
            
            // A centerline wall with material layers.
            // var axis = (Polyline)wall.RepresentationsOfType<IfcPolyline>().FirstOrDefault().ToICurve(false);
            
            foreach (var cis in wall.ContainedInStructure)
            {
                cis.RelatingStructure.ObjectPlacement.ToTransform().Concatenate(transform);
            }

            // var os = openings.Select(o=>o.ToOpening()).ToArray();

            if(solid != null)
            {
                var c = solid.SweptArea.ToICurve();
                if(c is Polygon)
                {
                    transform.Concatenate(solid.Position.ToTransform());
                    var result = new Wall(new Profile((Polygon)c), wallType, (IfcLengthMeasure)solid.Depth, transform);

                    result.Name = wall.Name;
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// Convert an IfcBeam to a beam.
        /// </summary>
        /// <param name="beam"></param>
        private static Beam ToBeam(this IfcBeam beam)
        {
            var elementTransform = beam.ObjectPlacement.ToTransform();
            
            var solid = beam.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();

            // foreach (var cis in beam.ContainedInStructure)
            // {
            //     cis.RelatingStructure.ObjectPlacement.ToTransform().Concatenate(transform);
            // }

            if(solid != null)
            {
                var solidTransform = solid.Position.ToTransform();

                var c = solid.SweptArea.ToICurve();
                if(c is Polygon)
                {
                    var cl = new Line(Vector3.Origin, 
                        solid.ExtrudedDirection.ToVector3(), (IfcLengthMeasure)solid.Depth);
                    var framingType = new StructuralFramingType(Guid.NewGuid().ToString(), new Profile((Polygon)c), BuiltInMaterials.Steel);
                    var result = new Beam(solidTransform.OfLine(cl), framingType, 0.0, 0.0, elementTransform);
                    result.Name = beam.Name;
                    return result; 
                }
            }
            return null;
        }

        /// <summary>
        /// Convert an IfcColumn to a Column.
        /// </summary>
        /// <param name="column"></param>
        private static Column ToColumn(this IfcColumn column)
        {
            var elementTransform = column.ObjectPlacement.ToTransform();
            
            var solid = column.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();
            foreach (var cis in column.ContainedInStructure)
            {
                cis.RelatingStructure.ObjectPlacement.ToTransform().Concatenate(elementTransform);
            }

            if(solid != null)
            {
                var solidTransform = solid.Position.ToTransform();
                var c = solid.SweptArea.ToICurve();
                var framingType = new StructuralFramingType(Guid.NewGuid().ToString(), new Profile((Polygon)c), BuiltInMaterials.Steel);
                var result = new Column(solidTransform.Origin, (IfcLengthMeasure)solid.Depth, framingType, elementTransform, 0.0, 0.0);
                result.Name = column.Name;
                return result;
            }
            return null;
        }

        /// <summary>
        /// Convert an IfcOpening to an Opening.
        /// </summary>
        /// <param name="opening"></param>
        private static Opening ToOpening(this IfcOpeningElement opening)
        {
            var openingTransform = opening.ObjectPlacement.ToTransform();
            var s = opening.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();
            if(s != null)
            {
                var solidTransform = s.Position.ToTransform();
                solidTransform.Concatenate(openingTransform);
                var profile = (Polygon)s.SweptArea.ToICurve();
                var newOpening = new Opening(profile, (IfcLengthMeasure)s.Depth, solidTransform);
                // var newOpening = new Opening(profile);
                if(opening.Name != null)
                {
                    newOpening.Name = opening.Name;
                }
                return newOpening;
            }
            return null;
        }

        private static Solid Representations(this IfcProduct product)
        {   
            var reps = product.Representation.Representations.SelectMany(r=>r.Items);
            foreach(var r in reps)
            {
                if(r is IfcSurfaceCurveSweptAreaSolid)
                {
                    throw new Exception("IfcSurfaceCurveSweptAreaSolid is not supported yet.");
                }
                if(r is IfcRevolvedAreaSolid)
                {
                    throw new Exception("IfcRevolvedAreaSolid is not supported yet.");
                }
                if(r is IfcSweptDiskSolid)
                {
                    throw new Exception("IfcSweptDiskSolid is not supported yet.");
                }
                else if(r is IfcExtrudedAreaSolid)
                {
                    var eas = (IfcExtrudedAreaSolid)r;
                    var profileDef = (IfcArbitraryClosedProfileDef)eas.SweptArea;
                    var pline = (IfcPolyline)profileDef.OuterCurve;
                    var outline = pline.ToPolygon(true);
                    var solid = Solid.SweepFace(outline, null, (IfcLengthMeasure)eas.Depth);
                }
                else if(r is IfcFacetedBrep)
                {
                    var solid = new Solid();
                    var fbr = (IfcFacetedBrep)r;
                    var shell = fbr.Outer;
                    var faces = new Face[shell.CfsFaces.Count];
                    for(var i=0; i< shell.CfsFaces.Count; i++)
                    {
                        var f = shell.CfsFaces[i];
                        var boundCount = 0;
                        Loop outer = null;
                        Loop[] inner = new Loop[f.Bounds.Count - 1];
                        foreach (var b in f.Bounds)
                        {
                            var loop = (IfcPolyLoop)b.Bound;
                            var newLoop = loop.Polygon.ToLoop(solid);
                            if(boundCount == 0)
                            {
                                outer = newLoop;
                            }
                            else
                            {
                                inner[boundCount-1] = newLoop;
                            }
                            boundCount++;
                        }
                        solid.AddFace(outer, inner);
                    }
                    return solid;
                }
                else if(r is IfcFacetedBrepWithVoids)
                {
                    throw new Exception("IfcFacetedBrepWithVoids is not supported yet.");
                }
            }
            return null;
        }

        private static IEnumerable<T> RepresentationsOfType<T>(this IfcProduct product) where T: IfcGeometricRepresentationItem
        {
            var reps = product.Representation.Representations.SelectMany(r=>r.Items);
            if(reps.Any())
            {
                return reps.OfType<T>();
            }
            return null;
        }

        /// <summary>
        /// Convert an IfcProfileDef to an iCurve.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        private static ICurve ToICurve(this IfcProfileDef profile)
        {
            if(profile is IfcCircleProfileDef)
            {
                var cpd = (IfcCircleProfileDef)profile;
                return Polygon.Circle((IfcLengthMeasure)cpd.Radius);
            }
            else if(profile is IfcParameterizedProfileDef)
            {
                var ipd = (IfcParameterizedProfileDef)profile;
                return ipd.ToICurve();
            }
            else if(profile is IfcArbitraryOpenProfileDef)
            {
                var aopd = (IfcArbitraryOpenProfileDef)profile;
                return aopd.ToICurve();
            }
            else if(profile is IfcArbitraryClosedProfileDef)
            {
                var acpd = (IfcArbitraryClosedProfileDef)profile;
                return acpd.ToICurve();
            }
            else if(profile is IfcCompositeProfileDef)
            {
                throw new Exception("IfcCompositeProfileDef is not supported yet.");
            }
            else if(profile is IfcDerivedProfileDef)
            {
                throw new Exception("IfcDerivedProfileDef is not supported yet.");
            }
            return null;
        }

        /// <summary>
        /// Convert an IfcParameterizedProfileDef to an ICurve
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        private static ICurve ToICurve(this IfcParameterizedProfileDef profile)
        {
            if(profile is IfcRectangleProfileDef)
            {
                var rect = (IfcRectangleProfileDef)profile;
                return Polygon.Rectangle((IfcLengthMeasure)rect.XDim, (IfcLengthMeasure)rect.YDim, rect.Position.Location.ToVector3());
            }
            else if(profile is IfcCircleProfileDef)
            {
                var circle = (IfcCircleProfileDef)profile;
                return Polygon.Circle((IfcLengthMeasure)circle.Radius);
            }
            else
            {
                throw new Exception($"The IfcParameterizedProfileDef type, {profile.GetType().Name}, is not supported.");
            }
        }

        /// <summary>
        /// Convert an IfcArbitraryOpenProfileDef to an ICurve.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        private static ICurve ToICurve(this IfcArbitraryOpenProfileDef profile)
        {
            return profile.Curve.ToICurve(false);
        }

        /// <summary>
        /// Convert an IfcArbitraryClosedProfileDef to an ICurve.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        private static ICurve ToICurve(this IfcArbitraryClosedProfileDef profile)
        {
            return profile.OuterCurve.ToICurve(true);
        }

        /// <summary>
        /// Convert an IfcCurve to in ICurve.
        /// </summary>
        /// <param name="curve">An IfcCurve.</param>
        /// <param name="closed">A flag indicating whether the curve is closed.</param>
        private static ICurve ToICurve(this IfcCurve curve, bool closed)
        {
            if(curve is IfcBoundedCurve)
            {
                if(curve is IfcCompositeCurve)
                {
                    throw new Exception("IfcCompositeCurve is not supported yet.");
                }
                else if(curve is IfcPolyline)
                {
                    var pl = (IfcPolyline)curve;
                    if(closed)
                    {
                        return pl.ToPolygon(true);
                    }
                    else
                    {
                        return pl.ToPolyline();
                    }
                }
                else if(curve is IfcTrimmedCurve)
                {
                    throw new Exception("IfcTrimmedCurve is not supported yet.");
                }
                else if (curve is IfcBSplineCurve)
                {
                    throw new Exception("IfcBSplineCurve is not supported yet.");
                }
            }
            else if(curve is IfcConic)
            {
                throw new Exception("IfcConic is not supported yet.");
            }
            else if(curve is IfcOffsetCurve2D)
            {
                throw new Exception("IfcOffsetCurve2D is not supported yet.");
            }
            else if(curve is IfcOffsetCurve3D)
            {
                throw new Exception("IfcOffsetCurve3D is not supported yet.");
            }
            return null;
        }

        /// <summary>
        /// Convert an IfcCartesianPoint to a Vector3.
        /// </summary>
        /// <param name="cartesianPoint">An IfcCartesianPoint.</param>
        private static Vector3 ToVector3(this IfcCartesianPoint cartesianPoint)
        {
            return cartesianPoint.Coordinates.ToVector3();
        }

        /// <summary>
        /// Convert a collection of IfcLengthMeasure to a Vector3.
        /// </summary>
        /// <param name="measures">A collection of IfcLengthMeasure.</param>
        private static Vector3 ToVector3(this List<IfcLengthMeasure> measures)
        {
            if (measures.Count == 2)
            {
                return new Vector3(measures[0], measures[1]);
            }
            else if (measures.Count == 3)
            {
                return new Vector3(measures[0], measures[1], measures[2]);
            }
            else
            {
                throw new Exception($"{measures.Count} measures could not be converted to a Vector3.");
            }
        }

        /// <summary>
        /// Convert an IfcPolyline to a Polygon.
        /// </summary>
        /// <param name="polyline">An IfcPolyline.</param>
        /// <param name="dropLastPoint">A flag indicating whether the last point should be included.</param>
        private static Polygon ToPolygon(this IfcPolyline polyline, bool dropLastPoint = false)
        {
            var count = dropLastPoint ? polyline.Points.Count - 1 : polyline.Points.Count;
            var verts = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                var v = polyline.Points[i].ToVector3();
                verts[i] = v;
            }
            return new Polygon(verts);
        }

        /// <summary>
        /// Convert an IfcPolyline to a Polyline.
        /// </summary>
        /// <param name="polyline">An IfcPolyline.</param>
        private static Polyline ToPolyline(this IfcPolyline polyline)
        {
            var verts = polyline.Points.Select(p=>p.ToVector3()).ToArray();
            return new Polyline(verts);
        }

        /// <summary>
        /// Check if an IfcPolyline is closed.
        /// </summary>
        /// <param name="pline"></param>
        /// <returns></returns>
        private static bool IsClosed(this IfcPolyline pline)
        {
            var start = pline.Points[0];
            var end = pline.Points[pline.Points.Count-1];
            return start.Equals(end);
        }

        /// <summary>
        /// Check if two IfcCartesianPoints have the same coordinates.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        private static bool Equals(this IfcCartesianPoint point, IfcCartesianPoint other)
        {
            for(var i=0; i<point.Coordinates.Count; i++)
            {
                if(point.Coordinates[i] != other.Coordinates[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Convert an IfcAxis2Placement3D to a Transform.
        /// </summary>
        /// <param name="cs">An IfcAxis2Placement3D.</param>
        /// <returns></returns>
        private static Transform ToTransform(this IfcAxis2Placement3D cs)
        {
            var d = cs.RefDirection != null ? cs.RefDirection.ToVector3() : Vector3.XAxis;
            var z = cs.Axis != null ? cs.Axis.ToVector3() : Vector3.ZAxis;
            var y = z.Cross(d);
            var x = y.Cross(z);
            var o = cs.Location.ToVector3();
            var t = new Transform(new Matrix(x, y, z, o));
            return t;
        }

        /// <summary>
        /// Convert an IfcAxis2Placement2D to a Transform.
        /// </summary>
        /// <param name="cs">An IfcAxis2Placement2D.</param>
        private static Transform ToTransform(this IfcAxis2Placement2D cs)
        {
            var d = cs.RefDirection.ToVector3();
            var z = Vector3.ZAxis;
            var o = cs.Location.ToVector3();
            return new Transform(o, d, z);
        }

        /// <summary>
        /// Convert an IfcDirection to a Vector3.
        /// </summary>
        /// <param name="direction">An IfcDirection.</param>
        private static Vector3 ToVector3(this IfcDirection direction)
        {
            var ratios = direction.DirectionRatios;
            return new Vector3(ratios[0], ratios[1], ratios[2]);
        }

        /// <summary>
        /// Convert an IfcAxis2Placement to a Transform.
        /// </summary>
        /// <param name="placement">An IfcAxis2Placement.</param>
        /// <returns></returns>
        private static Transform ToTransform(this IfcAxis2Placement placement)
        {
            // SELECT IfcAxis2Placement3d, IfcAxis2Placement2d
            if (placement.Choice.GetType() == typeof(IfcAxis2Placement2D))
            {
                var cs = (IfcAxis2Placement2D)placement.Choice;
                return cs.ToTransform();
            }
            else if (placement.Choice.GetType() == typeof(IfcAxis2Placement3D))
            {
                var cs = (IfcAxis2Placement3D)placement.Choice;
                var t = cs.ToTransform();
                return t;
            }
            else
            {
                throw new Exception($"The specified placement of type, {placement.GetType().ToString()}, cannot be converted to a Transform.");
            }
        }

        /// <summary>
        /// Convert an IfcLocalPlacement to a Transform.
        /// </summary>
        /// <param name="placement">An IfcLocalPlacement.</param>
        private static Transform ToTransform(this IfcLocalPlacement placement)
        {
            var t = placement.RelativePlacement.ToTransform();
            if(placement.PlacementRelTo != null)
            {
                var tr = placement.PlacementRelTo.ToTransform();
                t.Concatenate(tr);
            }
            return t;
        }

        /// <summary>
        /// Convert an IfcObjectPlacement to a Transform.
        /// </summary>
        /// <param name="placement">An IfcObjectPlacement.</param>
        private static Transform ToTransform(this IfcObjectPlacement placement)
        {
            if (placement.GetType() == typeof(IfcLocalPlacement))
            {
                var lp = (IfcLocalPlacement)placement;
                var t = lp.ToTransform();
                return t;
            }
            else if (placement.GetType() == typeof(IfcGridPlacement))
            {
                throw new Exception("IfcGridPlacement conversion to Transform not supported.");
            }
            return null;
        }

        /// <summary>
        /// Convert a collection of IfcCartesianPoint to a Polygon.
        /// </summary>
        /// <param name="loop">A collection of IfcCartesianPoint.</param>
        /// <returns>A Polygon.</returns>
        private static Polygon ToPolygon(this List<IfcCartesianPoint> loop)
        {
            var verts = new Vector3[loop.Count];
            for (var i = 0; i < loop.Count; i++)
            {
                verts[i] = loop[i].ToVector3();
            }
            return new Polygon(verts);
        }

        /// <summary>
        /// Convert a collection of IfcCartesianPoint to a Loop.
        /// </summary>
        /// <param name="loop">A collection of IfcCartesianPoint.</param>
        /// <param name="solid"></param>
        /// <returns>A Loop.</returns>
        private static Loop ToLoop(this List<IfcCartesianPoint> loop, Solid solid)
        {
            var hes = new HalfEdge[loop.Count];
            for (var i = 0; i < loop.Count; i++)
            {
                var v = solid.AddVertex(loop[i].ToVector3());
                hes[i] = new HalfEdge(v);
            }
            var newLoop = new Loop(hes);
            return newLoop;
        }

        /// <summary>
        /// Convert an IfcPolyloop to a Polygon.
        /// </summary>
        /// <param name="loop"></param>
        /// <returns>A Polygon.</returns>
        private static Polygon ToPolygon(this IfcPolyLoop loop)
        {
            return loop.Polygon.ToPolygon();
        }
    }
}