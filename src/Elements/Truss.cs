using Elements.Geometry;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// An aggregation of structural framing elements.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/TrussExample.cs?name=example)]
    /// </example>
    public class Truss : Element, IAggregateElements
    {
        private List<Beam> _web = new List<Beam>();
        private List<Beam> _topChord = new List<Beam>();
        private List<Beam> _bottomChord = new List<Beam>();

        /// <summary>
        /// The elements aggregated by this element.
        /// </summary>
        public List<Element> Elements { get; }

        /// <summary>
        /// The start of the truss.
        /// </summary>
        public Vector3 Start { get; }

        /// <summary>
        /// The end of the truss.
        /// </summary>
        public Vector3 End { get; }

        /// <summary>
        /// The depth of the truss.
        /// </summary>
        public double Depth { get; }

        /// <summary>
        /// The number of divisions in the truss.
        /// </summary>
        public int Divisions { get; }

        /// <summary>
        /// The Profile used for members in the top chord of the truss.
        /// </summary>
        public StructuralFramingType TopChordType { get; }

        /// <summary>
        /// The Profile used for members in the bottom chord of the truss.
        /// </summary>
        public StructuralFramingType BottomChordType { get; }

        /// <summary>
        /// The Profile used for members in the web of the truss.
        /// </summary>
        public StructuralFramingType WebType { get; }

        /// <summary>
        /// Construct a truss.
        /// </summary>
        /// <param name="start">The start of the truss.</param>
        /// <param name="end">The end of the truss.</param>
        /// <param name="depth">The depth of the truss.</param>
        /// <param name="divisions">The number of panels in the truss.</param>
        /// <param name="topChordType">The structural framing type to be used for the top chord.</param>
        /// <param name="bottomChordType">The structural framing type to be used for the bottom chord.</param>
        /// <param name="webType">The structural framing type to be used for the web.</param>
        /// <param name="material">The truss' material.</param>
        /// <param name="startSetback">A setback to apply to the start of all members of the truss.</param>
        /// <param name="endSetback">A setback to apply to the end of all members of the truss.</param>
        [JsonConstructor]
        public Truss(Vector3 start, Vector3 end, double depth, int divisions, StructuralFramingType topChordType, 
            StructuralFramingType bottomChordType, StructuralFramingType webType, Material material, double startSetback = 0.0, double endSetback = 0.0)
        {
            if (depth <= 0)
            {
                throw new ArgumentOutOfRangeException($"The provided depth ({depth}) must be greater than 0.0.");
            }

            this.Start = start;
            this.End = end;
            this.Depth = depth;
            this.Divisions = divisions;
            this.TopChordType = topChordType;
            this.BottomChordType = bottomChordType;
            this.WebType = webType;

            var l = new Line(start, end);
            var pts = Vector3.AtNEqualSpacesAlongLine(l, divisions, true);
            for (var i = 0; i < pts.Count; i++)
            {
                if (i != pts.Count - 1)
                {
                    var bt = new Line(pts[i], pts[i + 1]);
                    var bb = new Line(pts[i] - new Vector3(0, 0, depth), pts[i + 1] - new Vector3(0, 0, depth));
                    this._topChord.Add(new Beam(bt, topChordType, startSetback, endSetback));
                    this._bottomChord.Add(new Beam(bb, bottomChordType, startSetback, endSetback));
                    var diag = i > Math.Ceiling((double)divisions / 2) ? new Line(pts[i], pts[i + 1] - new Vector3(0, 0, depth)) : new Line(pts[i + 1], pts[i] - new Vector3(0, 0, depth));
                    this._web.Add(new Beam(diag, webType, startSetback, endSetback));
                }
                var wb = new Line(pts[i], pts[i] - new Vector3(0, 0, depth));
                this._web.Add(new Beam(wb, webType, startSetback, endSetback));
            }

            this.Elements = new List<Element>();
            this.Elements.AddRange(this._topChord);
            this.Elements.AddRange(this._bottomChord);
            this.Elements.AddRange(this._web);
        }
    }
}