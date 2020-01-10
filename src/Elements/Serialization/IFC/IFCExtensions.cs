using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using IFC;

namespace Elements.Serialization.IFC
{
    /// <summary>
    /// Extension methods for converting IFC entities to elements.
    /// </summary>
    internal static class IFCExtensions
    {
        internal static Beam ToBeam(this IfcBeam beam)
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

                var c = solid.SweptArea.ToCurve();
                if(c is Polygon)
                {
                    var cl = new Line(Vector3.Origin, 
                        solid.ExtrudedDirection.ToVector3(), (IfcLengthMeasure)solid.Depth);
                    var result = new Beam(solidTransform.OfLine(cl),
                                          new Profile((Polygon)c),
                                          BuiltInMaterials.Steel,
                                          0.0,
                                          0.0,
                                          0.0,
                                          elementTransform,
                                          IfcGuid.FromIfcGUID(beam.GlobalId),
                                          beam.Name);
                    return result; 
                }
            }
            return null;
        }

        internal static Column ToColumn(this IfcColumn column)
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
                var c = solid.SweptArea.ToCurve();
                var result = new Column(solidTransform.Origin, (IfcLengthMeasure)solid.Depth, new Profile((Polygon)c), BuiltInMaterials.Steel, elementTransform, 0.0, 0.0, 0.0, IfcGuid.FromIfcGUID(column.GlobalId), column.Name);
                return result;
            }
            return null;
        }

        internal static Space ToSpace(this IfcSpace space)
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
                var result = new Space(new Profile(outline), (IfcLengthMeasure)solid.Depth, material, transform, null, IfcGuid.FromIfcGUID(space.GlobalId), space.Name);
                return result;
            }
            else if (foundSolid.GetType() == typeof(IfcFacetedBrep))
            {
                var solid = (IfcFacetedBrep)foundSolid;
                var shell = solid.Outer;
                var newSolid = new Solid();
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
                var result = new Space(newSolid, transform, null, Guid.NewGuid(), space.Name);

                return result;
            }

            return null;
        }

        internal static Floor ToFloor(this IfcSlab slab, IEnumerable<IfcOpeningElement> openings)
        {
            var transform = new Transform();
            transform.Concatenate(slab.ObjectPlacement.ToTransform());
            // Console.WriteLine($"IfcSlab transform:\n{transform}\n");

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
            }

            var outline = (Polygon)solid.SweptArea.ToCurve();
            var solidTransform = solid.Position.ToTransform();

            solidTransform.Concatenate(transform);
            var floor = new Floor(new Profile(outline), (IfcLengthMeasure)solid.Depth, 
                solidTransform, BuiltInMaterials.Concrete, null, IfcGuid.FromIfcGUID(slab.GlobalId));

            floor.Openings.AddRange(openings.Select(o=>o.ToOpening()));

            return floor;
        }
    
        internal static Wall ToWall(this IfcWallStandardCase wall, 
            IEnumerable<IfcOpeningElement> openings)
        {
            var transform = new Transform();
            transform.Concatenate(wall.ObjectPlacement.ToTransform());

            // An extruded face solid.
            var solid = wall.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();
            if(solid == null)
            {
                // It's possible that the rep is a boolean.
                var boolean = wall.RepresentationsOfType<IfcBooleanClippingResult>().FirstOrDefault();
                if(boolean != null)
                {
                    solid = boolean.FirstOperand.Choice as IfcExtrudedAreaSolid;
                    if(solid == null)
                    {
                        solid = boolean.SecondOperand.Choice as IfcExtrudedAreaSolid; 
                    }
                }

                // if(solid == null)
                // {
                //     throw new Exception("No usable solid was found when converting an IfcWallStandardCase to a Wall.");
                // }
            }
            
            // A centerline wall with material layers.
            // var axis = (Polyline)wall.RepresentationsOfType<IfcPolyline>().FirstOrDefault().ToICurve(false);
            
            foreach (var cis in wall.ContainedInStructure)
            {
                cis.RelatingStructure.ObjectPlacement.ToTransform().Concatenate(transform);
            }

            // var os = openings.Select(o=>o.ToOpening()).ToArray();

            if(solid != null)
            {
                var c = solid.SweptArea.ToCurve();
                if(c is Polygon)
                {
                    transform.Concatenate(solid.Position.ToTransform());
                    var result = new Wall((Polygon)c,
                                          (IfcLengthMeasure)solid.Depth,
                                          null,
                                          transform,
                                          null,
                                          IfcGuid.FromIfcGUID(wall.GlobalId),
                                          wall.Name);
                    return result;
                }
            }
            return null;
        }
    
        internal static Opening ToOpening(this IfcOpeningElement opening)
        {
            var openingTransform = opening.ObjectPlacement.ToTransform();
            var s = opening.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();
            if(s != null)
            {
                var solidTransform = s.Position.ToTransform();
                solidTransform.Concatenate(openingTransform);
                var profile = (Polygon)s.SweptArea.ToCurve();
                // Console.WriteLine($"Opening profile:\n{profile.ToString()}\n");
                
                var newOpening = new Opening(profile,
                                             (IfcLengthMeasure)s.Depth,
                                             solidTransform,
                                             null,
                                             IfcGuid.FromIfcGUID(opening.GlobalId));
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

        // private static IfcOpeningElement ToIfcOpeningElement(this Opening opening, IfcRepresentationContext context, Document doc, IfcObjectPlacement parent)
        // {
        //     // var sweptArea = opening.Profile.Perimeter.ToIfcArbitraryClosedProfileDef(doc);
        //     // We use the Z extrude direction because the direction is 
        //     // relative to the local placement, which is a transform at the
        //     // beam's end with the Z axis pointing along the direction.
            
        //     // var extrudeDirection = opening.ExtrudeDirection.ToIfcDirection();
        //     // var position = new Transform().ToIfcAxis2Placement3D(doc);
        //     // var solid = new IfcExtrudedAreaSolid(sweptArea, position, 
        //     //     extrudeDirection, new IfcPositiveLengthMeasure(opening.ExtrudeDepth));

        //     var extrude= (Extrude)opening.Geometry.SolidOperations[0];
        //     var solid = extrude.ToIfcExtrudedAreaSolid(new Transform(), doc);
        //     var localPlacement = new Transform().ToIfcLocalPlacement(doc, parent);

        //     var shape = new IfcShapeRepresentation(context, "Body", "SweptSolid", new List<IfcRepresentationItem>{solid});
        //     var productRep = new IfcProductDefinitionShape(new List<IfcRepresentation>{shape});

        //     var ifcOpening = new IfcOpeningElement(IfcGuid.ToIfcGuid(opening.Id), null, null, null, null, localPlacement, productRep, null);
            
        //     // doc.AddEntity(sweptArea);
        //     // doc.AddEntity(extrudeDirection);
        //     // doc.AddEntity(position);
        //     // doc.AddEntity(repItem);

        //     doc.AddEntity(solid);
        //     doc.AddEntity(localPlacement);
        //     doc.AddEntity(shape);
        //     doc.AddEntity(productRep);

        //     return ifcOpening;
        // }

        private static ICurve ToCurve(this IfcProfileDef profile)
        {
            if(profile is IfcCircleProfileDef)
            {
                var cpd = (IfcCircleProfileDef)profile;
                return Polygon.Circle((IfcLengthMeasure)cpd.Radius);
            }
            else if(profile is IfcParameterizedProfileDef)
            {
                var ipd = (IfcParameterizedProfileDef)profile;
                return ipd.ToCurve();
            }
            else if(profile is IfcArbitraryOpenProfileDef)
            {
                var aopd = (IfcArbitraryOpenProfileDef)profile;
                return aopd.ToCurve();
            }
            else if(profile is IfcArbitraryClosedProfileDef)
            {
                var acpd = (IfcArbitraryClosedProfileDef)profile;
                return acpd.ToCurve();
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

        private static ICurve ToCurve(this IfcParameterizedProfileDef profile)
        {
            if(profile is IfcRectangleProfileDef)
            {
                var rect = (IfcRectangleProfileDef)profile;
                var p = Polygon.Rectangle((IfcLengthMeasure)rect.XDim, (IfcLengthMeasure)rect.YDim);
                var t = new Transform(rect.Position.Location.ToVector3());
                return t.OfPolygon(p);
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

        private static ICurve ToCurve(this IfcArbitraryOpenProfileDef profile)
        {
            return profile.Curve.ToCurve(false);
        }

        private static ICurve ToCurve(this IfcArbitraryClosedProfileDef profile)
        {
            return profile.OuterCurve.ToCurve(true);
        }

        private static ICurve ToCurve(this IfcCurve curve, bool closed)
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

        private static Vector3 ToVector3(this IfcCartesianPoint cartesianPoint)
        {
            return cartesianPoint.Coordinates.ToVector3();
        }

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

        private static Polyline ToPolyline(this IfcPolyline polyline)
        {
            var verts = polyline.Points.Select(p=>p.ToVector3()).ToArray();
            return new Polyline(verts);
        }

        private static bool IsClosed(this IfcPolyline pline)
        {
            var start = pline.Points[0];
            var end = pline.Points[pline.Points.Count-1];
            return start.Equals(end);
        }

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
        private static Transform ToTransform(this IfcAxis2Placement3D cs)
        {
            var x = cs.RefDirection != null ? cs.RefDirection.ToVector3() : Vector3.XAxis;
            var z = cs.Axis != null ? cs.Axis.ToVector3() : Vector3.ZAxis;
            var y = z.Cross(x);
            var o = cs.Location.ToVector3();
            var t = new Transform(o, x, y, z);
            return t;
        }

        private static Transform ToTransform(this IfcAxis2Placement2D cs)
        {
            var d = cs.RefDirection.ToVector3();
            var z = Vector3.ZAxis;
            var o = cs.Location.ToVector3();
            return new Transform(o, d, z);
        }

        private static Vector3 ToVector3(this IfcDirection direction)
        {
            var ratios = direction.DirectionRatios;
            return new Vector3(ratios[0], ratios[1], ratios[2]);
        }

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

        private static Polygon ToPolygon(this List<IfcCartesianPoint> loop)
        {
            var verts = new Vector3[loop.Count];
            for (var i = 0; i < loop.Count; i++)
            {
                verts[i] = loop[i].ToVector3();
            }
            return new Polygon(verts);
        }

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

        private static Polygon ToPolygon(this IfcPolyLoop loop)
        {
            return loop.Polygon.ToPolygon();
        }
    }
}