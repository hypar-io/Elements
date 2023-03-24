using System;
using System.Collections.Generic;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// Contains SVG serialization event data
    /// </summary>
    public class ElementSerializationEventArgs : EventArgs
    {
        /// <summary>
        /// The set of available options of the element creation sequence
        /// </summary>
        public enum CreationSequences
        {
            /// <summary>
            /// The element should be added to the document immidiatly
            /// </summary>
            Immediately,
            /// <summary>
            /// The element should be added to the document before grid lines
            /// </summary>
            BeforeGridLines,
            /// <summary>
            /// The element should be added to the document after grid lines
            /// </summary>
            AfterGridLines
        }

        /// <summary>
        /// Initializes a new instance of the ElementSerializationEventArgs class
        /// </summary>
        /// <param name="drawingPlan">The instance of the SvgDrawingPlan class
        /// It can be used to create SvgElements
        /// (e.g. polygon.ToSvgPolygon(e.DrawingPlan, e.DrawingPlan.FrontContext) or e.DrawingPlan.CreateText(..))</param>
        /// <param name="element"></param>
        /// <param name="scale"></param>
        public ElementSerializationEventArgs(SvgSection drawingPlan, Element element, double scale)
        {
            DrawingPlan = drawingPlan;
            Element = element;
            Scale = scale;
        }

        /// <summary>
        /// The instance of the drawing plan where the event occured
        /// </summary>
        public SvgSection DrawingPlan { get; }

        /// <summary>
        /// Gets or sets if element was processed by user and should be excluded from the further calculations
        /// </summary>
        public bool IsProcessed { get; set; }

        /// <summary>
        /// The set of elements that will be added to the drawing plan if IsProcessed == true
        /// Please add there all new element that you created from the Element
        public List<DrawingAction> Actions { get; } = new List<DrawingAction>();


        /// <summary>
        /// The element which is processed before being added to the drawing plan
        /// </summary>
        public Element Element { get; }

        /// <summary>
        /// Items are processed in the following order:
        /// 1. Elements that are marked as Immidiatly
        /// 2. All elements that where not processed within this event
        /// 3. Elements that are marked as BeforeGridLines
        /// 4. Grid lines and texts
        /// 5. Elements that are marked as AfterGridLines
        /// </summary>
        public CreationSequences CreationSequence { get; set; }

        /// <summary>
        /// The drawing scale
        /// </summary>
        public double Scale { get; }
    }
}