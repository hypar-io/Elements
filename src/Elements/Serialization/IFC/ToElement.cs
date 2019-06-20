using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;
using IFC;

namespace Elements.Serialization.IFC
{
    public static partial class IFCExtensions
    {
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
                var result = new Space(newSolid, transform);

                result.Name = space.Name;
                return result;
            }

            return null;
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

            var floorType = new FloorType($"{Guid.NewGuid().ToString()}_floor_type", new List<MaterialLayer>{new MaterialLayer(new Material("slab",Colors.Green), (IfcLengthMeasure)solid.Depth)});
            var outline = (Polygon)solid.SweptArea.ToICurve();
            var solidTransform = solid.Position.ToTransform();
            var floor = new Floor(new Profile(outline), solidTransform, solid.ExtrudedDirection.ToVector3(), 
                floorType, 0, transform);
            floor.Name = slab.Name;
            
            return floor;
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
                var c = solid.SweptArea.ToICurve();
                if(c is Polygon)
                {
                    transform.Concatenate(solid.Position.ToTransform());
                    var result = new Wall((Polygon)c, wallType, (IfcLengthMeasure)solid.Depth, transform);

                    result.Name = wall.Name;
                    return result;
                }
            }
            return null;
        }
    }
}