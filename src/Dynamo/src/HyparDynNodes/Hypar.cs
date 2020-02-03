using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using Elements;
using Elements.Serialization.glTF;
using Elements.Geometry;


using Revit.Elements;
using RevitServices.Persistence;


namespace HyparDyn
{
    public class Hypar  
    {
        private Hypar() {}

        /// <summary>
        /// gets the walls
        /// </summary>
        /// <param name="wall">The walls to be exported</param>
        /// <returns name="Hypar.Wall">The Hypar Wall element </param>
        public static Elements.Wall ConvertRevitWall(Revit.Elements.Wall incomingWall) {
            var r_Wall = (Autodesk.Revit.DB.Wall)incomingWall.InternalElement;

            // wrapped exception catching to deliver more meaningful message in Dynamo
            try {
                return RevitHyparTools.Create.WallFromRevitWall(r_Wall);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public static Elements.Floor[] ConvertRevitFloor(Revit.Elements.Floor incomingFloor) {
            var r_Floor = (Autodesk.Revit.DB.Floor)incomingFloor.InternalElement;
            
            // wrapped exception catching to deliver more meaningful message in Dynamo
            try {
                return RevitHyparTools.Create.FloorsFromRevitFloor(DocumentManager.Instance.CurrentDBDocument, r_Floor);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public static string ConvertRevitColumn(Revit.Elements.Element column) {
            return "";
        }

        public static void WriteGlb(string filePath, Model model) {
            model.ToGlTF(filePath);
        }

        public static Model ModelFromElements(object[] elements) {
            var mdl = new Model();
            var elems = elements.Cast<Elements.Element>().Where(e => e != null);
            
            mdl.AddElements(elems);

            return mdl;
        }
    }
}