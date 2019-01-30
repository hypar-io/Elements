using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Serialization
{
    /// <summary>
    /// Extension methods for converting IFC types to Element types.
    /// </summary>
    public static class IFCExtensions
    {
        /// <summary>
        /// Convert an IfcSlab to a Floor.
        /// </summary>
        /// <param name="slab">An IfcSlab.</param>
        public static Floor ToFloor(this IfcSlab slab)
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
            var floor = new Floor(new Profile(outline), solidTransform, solid.ExtrudedDirection.ToVector3(), floorType, 0, BuiltInMaterials.Default, transform);
            floor.Name = slab.Name;
            
            return floor;
        }

        /// <summary>
        /// Convert and IfcSpace to a Space.
        /// </summary>
        /// <param name="space">An IfcSpace.</param>
        /// <returns></returns>
        public static Space ToSpace(this IfcSpace space)
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
            if (foundSolid.GetType() == typeof(IFC.IfcExtrudedAreaSolid))
            {
                var solid = (IFC.IfcExtrudedAreaSolid)foundSolid;
                var profileDef = (IFC.IfcArbitraryClosedProfileDef)solid.SweptArea;
                transform.Concatenate(solid.Position.ToTransform());
                var pline = (IFC.IfcPolyline)profileDef.OuterCurve;
                var outline = pline.ToPolygon(true);
                var result = new Space(new Profile(outline), (IfcLengthMeasure)solid.Depth, 0.0, material, transform);
                result.Name = space.Name;
                return result;
            }
            else if (foundSolid.GetType() == typeof(IFC.IfcFacetedBrep))
            {
                var solid = (IFC.IfcFacetedBrep)foundSolid;
                var shell = solid.Outer;
                var newSolid = new Solid();
                for(var i=0; i< shell.CfsFaces.Count; i++)
                {
                    var f = shell.CfsFaces[i];
                    foreach (var b in f.Bounds)
                    {
                        var loop = (IFC.IfcPolyLoop)b.Bound;
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
        /// <param name="wall"></param>
        public static Wall ToWall(this IfcWallStandardCase wall)
        {
            var transform = new Transform();
            transform.Concatenate(wall.ObjectPlacement.ToTransform());
            var solid = wall.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();

            foreach (var cis in wall.ContainedInStructure)
            {
                cis.RelatingStructure.ObjectPlacement.ToTransform().Concatenate(transform);
            }

            var material = new Material("wall", new Color(0.5f, 0.5f, 0.5f, 0.5f), 0.1f, 0.1f);

            if(solid != null)
            {
                var c = solid.SweptArea.ToICurve();
                if(c is Polygon)
                {
                    transform.Concatenate(solid.Position.ToTransform());
                    var result = new Wall(new Profile((Polygon)c), (IfcLengthMeasure)solid.Depth, material, transform);
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
        public static Beam ToBeam(this IfcBeam beam)
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
                    var cl = new Line(Vector3.Origin, solid.ExtrudedDirection.ToVector3(), (IfcLengthMeasure)solid.Depth);
                    var result = new Beam(solidTransform.OfLine(cl), new Profile((Polygon)c), BuiltInMaterials.Steel, 0.0, 0.0, elementTransform);
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
        public static Column ToColumn(this IfcColumn column)
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
                var result = new Column(solidTransform.Origin, (IfcLengthMeasure)solid.Depth, new Profile((Polygon)c), BuiltInMaterials.Steel, elementTransform, 0.0, 0.0);
                result.Name = column.Name;
                return result;
            }
            return null;
        }

        /// <summary>
        /// Convert an IfcOpening to an Opening.
        /// </summary>
        /// <param name="opening"></param>
        public static Opening ToOpening(this IfcOpeningElement opening)
        {
            var openingTransform = opening.ObjectPlacement.ToTransform();
            var s = opening.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();
            if(s != null)
            {
                var solidTransform = s.Position.ToTransform();
                solidTransform.Concatenate(openingTransform);
                var profile = (Polygon)s.SweptArea.ToICurve();
                // var newOpening = new Opening(profile, (IfcLengthMeasure)s.Depth, solidTransform);
                var newOpening = new Opening(profile);
                newOpening.Name = opening.Name;
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
                    var profileDef = (IFC.IfcArbitraryClosedProfileDef)eas.SweptArea;
                    var pline = (IFC.IfcPolyline)profileDef.OuterCurve;
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
                            var loop = (IFC.IfcPolyLoop)b.Bound;
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
        public static ICurve ToICurve(this IfcProfileDef profile)
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
        public static ICurve ToICurve(this IfcParameterizedProfileDef profile)
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
        public static ICurve ToICurve(this IfcArbitraryOpenProfileDef profile)
        {
            return profile.Curve.ToICurve();
        }

        /// <summary>
        /// Convert an IfcArbitraryClosedProfileDef to an ICurve.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static ICurve ToICurve(this IfcArbitraryClosedProfileDef profile)
        {
            return profile.OuterCurve.ToICurve();
        }

        /// <summary>
        /// Convert an IfcCurve to in ICurve.
        /// </summary>
        /// <param name="curve"></param>
        public static ICurve ToICurve(this IfcCurve curve)
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
                    return pl.ToPolygon(true);
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
        public static Vector3 ToVector3(this IfcCartesianPoint cartesianPoint)
        {
            return cartesianPoint.Coordinates.ToVector3();
        }

        /// <summary>
        /// Convert a collection of IfcLengthMeasure to a Vector3.
        /// </summary>
        /// <param name="measures">A collection of IfcLengthMeasure.</param>
        public static Vector3 ToVector3(this List<IfcLengthMeasure> measures)
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
        public static Polygon ToPolygon(this IfcPolyline polyline, bool dropLastPoint = false)
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
        /// Check if an IfcPolyline is closed.
        /// </summary>
        /// <param name="pline"></param>
        /// <returns></returns>
        public static bool IsClosed(this IfcPolyline pline)
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
        public static bool Equals(this IfcCartesianPoint point, IfcCartesianPoint other)
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
        public static Transform ToTransform(this IfcAxis2Placement3D cs)
        {
            var d = cs.RefDirection.ToVector3();
            var z = cs.Axis.ToVector3();
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
        public static Transform ToTransform(this IfcAxis2Placement2D cs)
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
        public static Vector3 ToVector3(this IfcDirection direction)
        {
            var ratios = direction.DirectionRatios;
            return new Vector3(ratios[0], ratios[1], ratios[2]);
        }

        /// <summary>
        /// Convert an IfcAxis2Placement to a Transform.
        /// </summary>
        /// <param name="placement">An IfcAxis2Placement.</param>
        /// <returns></returns>
        public static Transform ToTransform(this IfcAxis2Placement placement)
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
        /// <returns></returns>
        public static Transform ToTransform(this IfcLocalPlacement placement)
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
        public static Transform ToTransform(this IfcObjectPlacement placement)
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
        public static Polygon ToPolygon(this List<IfcCartesianPoint> loop)
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
        public static Loop ToLoop(this List<IfcCartesianPoint> loop, Solid solid)
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
        public static Polygon ToPolygon(this IfcPolyLoop loop)
        {
            return loop.Polygon.ToPolygon();
        }
    }
}