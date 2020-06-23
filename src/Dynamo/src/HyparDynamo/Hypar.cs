using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Serialization.glTF;
using Hypar.Revit;
using RevitServices.Persistence;
using Elements;
using Elements.Generate;
using Elements.Geometry;

using ADSK = Autodesk.Revit.DB;
using Autodesk.Revit.DB;

namespace HyparDynamo.Hypar
{
    public static class SpaceBoundary
    {
        /// <summary>
        /// Convert a Revit Area into a SpaceBoundary element. This currently only
        /// stores the perimeter polygon, and no geometric representation, so you will 
        /// not see it in 3D views in Hypar.
        /// </summary>
        /// <param name="revitArea">The area that is meant to be converted to a Hypar SpaceBoundary.</param>
        /// <returns name="SpaceBoundar">The Hypar space boundary elements.</returns>
        public static Elements.SpaceBoundary[] FromArea(Revit.Elements.Element revitArea)
        {
            var areaElement = (Autodesk.Revit.DB.Area)revitArea.InternalElement;
            var doc = DocumentManager.Instance.CurrentDBDocument;

            return Create.SpaceBoundaryFromRevitArea(areaElement, doc);
        }
    }

    public static class ModelPoints
    {
        /// <summary>
        /// Convert a list of points from Dynamo into a ModelPoints element, with the option 
        /// to add a tag that will be stored in the elements Name field.
        /// </summary>
        /// <param name="points">The points that will be stored.</param>
        /// <param name="tag">The tag to assign to the model points.</param>
        /// <returns name="ModelPoints">The ModelPoint objects for Hypar.</returns>
        public static Elements.ModelPoints FromPoints(List<Autodesk.DesignScript.Geometry.Point> points, string tag = "")
        {
            return Create.ModelPointsFromPoints(points.Select(p => new XYZ(p.X, p.Y, p.Z)), tag);
        }
    }

    public static class Wall
    {
        /// <summary>
        /// Convert a Revit wall to Elements.WallByProfile(s) for use in Hypar models.
        /// Sometimes a single wall in Revit needs to be converted to multiple Hypar walls.
        /// </summary>
        /// <param name="revitWall">The walls to be exported.</param>
        /// <returns name="WallByProfiles">The Hypar walls.</returns>
        public static Elements.WallByProfile[] FromRevitWall(this Revit.Elements.Wall revitWall)
        {
            var revitWallElement = (Autodesk.Revit.DB.Wall)revitWall.InternalElement;

            // wrapped exception catching to deliver more meaningful message in Dynamo
            try
            {
                return Create.WallsFromRevitWall(revitWallElement, DocumentManager.Instance.CurrentDBDocument);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public static class Floor
    {
        /// <summary>
        /// Convert a Revit floor to Elements.Floor(s) for use in Hypar models.
        /// Sometimes a single floor in Revit needs to be converted to multiple Hypar floors.
        /// </summary>
        /// <param name="revitFloor">The floor to be exported.</param>
        /// <returns name="Floors">The Hypar floors.</returns>
        public static Elements.Floor[] FromRevitFloor(this Revit.Elements.Floor revitFloor)
        {
            var revitFloorElement = (Autodesk.Revit.DB.Floor)revitFloor.InternalElement;

            // wrapped exception catching to deliver more meaningful message in Dynamo
            try
            {
                return Create.FloorsFromRevitFloor(DocumentManager.Instance.CurrentDBDocument, revitFloorElement);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public static class Column
    {
        /// <summary>
        /// Convert a column in Revit into an Elements.Column for use in Hypar models.
        /// </summary>
        /// <param name="revitColumn">The column to be exported.</param>
        /// <returns name="Column">The Hypar column element.</returns>
        public static Elements.Column FromRevitColumn(this Revit.Elements.Element revitColumn)
        {
            var revitColumnElement = (Autodesk.Revit.DB.FamilyInstance)revitColumn.InternalElement;
            try
            {
                return Create.ColumnFromRevitColumn(revitColumnElement);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public static class Model
    {
        /// <summary>
        /// Write a Hypar model to its JSON representation .
        /// </summary>
        /// <param name="filePath">The path to write the JSON file.</param>
        /// <param name="model">The Hypar Model to write.</param>
        public static void WriteJson(string filePath, Elements.Model model)
        {
            try
            {
                var json = model.ToJson();
                System.IO.File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Write a Hypar model to glb.  GLB is a file format that is use for displaying 
        /// 3D models on the web. This is the format used in the hypar.io website,
        /// and you can also view these files using https://gltf-viewer.donmccurdy.com/ .
        /// </summary>
        /// <param name="filePath">The path to write the JSON file.</param>
        /// <param name="model">The Hypar Model to write.</param>
        public static void WriteGlb(string filePath, Elements.Model model)
        {
            try
            {
                model.ToGlTF(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Create a Hypar Model from a list of Elements.
        /// </summary>
        /// <param name="elements">The list of Elements that should be added to the model.</param>
        /// <returns name="Model">The Hypar Model to write.</returns>
        public static Elements.Model FromElements(IList<object> elements)
        {
            var model = new Elements.Model();
            var elems = elements.Cast<Elements.Element>().Where(e => e != null);

            model.AddElements(elems);

            return model;
        }
    }
}