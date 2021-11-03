#pragma warning disable 1591

using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Spatial;
using Newtonsoft.Json;

namespace Elements
{
    public enum JoistSeatDepth
    {
        IN2_5,
        IN5,
        IN7_5,
        IN10_5
    }

    public enum JoistProfileType
    {
        LL1_5X1_5,
        LL2X2,
        LL3X3,
        LL4X4,
        LL5X5,
        LL6X6,
        LL7X7,
        LL8X8
    }

    public enum JoistDepth
    {
        IN10,
        IN12,
        IN14,
        IN16,
        IN18,
        IN20,
        IN22,
        IN24,
        IN26,
        IN28,
        IN30,
        IN32,
        IN34,
        IN36,
        IN38,
        IN40,
        IN42,
        IN44,
        IN46,
        IN48,
        IN50,
        IN52,
        IN54,
        IN56,
        IN58,
        IN60,
        IN62,
        IN64,
        IN66,
        IN68,
        IN70,
        IN72
    }

    /// <summary>
    /// A joist.
    /// </summary>
    public class Joist : StructuralFraming
    {
        private const double THICKNESS = 0.125;

        /// <summary>
        /// The distance to the first panel.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Profile of the top chord of the joist.
        /// </summary>
        public JoistProfileType TopChordProfile { get; set; }

        /// <summary>
        /// Profile of the bottom chord of the joist.
        /// </summary>
        public JoistProfileType BottomChordProfile { get; set; }

        /// <summary>
        /// Profile of the web of the joist.
        /// </summary>
        public JoistProfileType WebProfile { get; set; }

        /// <summary>
        /// The depth of the joist.
        /// </summary>
        public JoistDepth Depth { get; set; }

        /// <summary>
        /// The number of cells in the joist.
        /// </summary>
        public int CellCount { get; set; }

        /// <summary>
        /// The seat depth of the joist.
        /// </summary>
        public JoistSeatDepth SeatDepth { get; set; }

        /// <summary>
        /// The joist support points along the top of the joist.
        /// </summary>
        public List<Vector3> JoistPoints { get; set; } = new List<Vector3>();

        /// <summary>
        /// Construct a new BarJoist.
        /// </summary>
        /// <param name="curve">The centerline of the joist.</param>
        /// <param name="topChordProfile">The top chord profile of the joist.</param>
        /// <param name="bottomChordProfile">The bottom chord profile of the joist.</param>
        /// <param name="webProfile">The web profile of the joist.</param>
        /// <param name="material">The joist's material.</param>
        /// <param name="name">The name of the joist.</param>
        /// <param name="id">The unique identifier of the joist.</param>
        /// <param name="depth">The depth of the joist.</param>
        /// <param name="cellCount">The cell count of the joist.</param>
        /// <param name="seatDepth">The seat depth of the joist.</param>
        /// <param name="y">The distance to the first panel of the joist.</param>
        [JsonConstructor]
        public Joist(Line curve,
                     JoistProfileType topChordProfile,
                     JoistProfileType bottomChordProfile,
                     JoistProfileType webProfile,
                     JoistDepth depth,
                     int cellCount,
                     JoistSeatDepth seatDepth,
                     double y,
                     Material material,
                     string name = null,
                     Guid id = default) : base(curve, null, material, name: name, id: id)
        {
            TopChordProfile = topChordProfile;
            BottomChordProfile = bottomChordProfile;
            WebProfile = webProfile;
            Depth = depth;
            CellCount = cellCount;
            SeatDepth = seatDepth;
            Y = y;

            Representation = ConstructRepresentation();
            Representation.SkipCSGUnion = true;
        }

        private Profile[] Construct2LProfile(JoistProfileType profileType, bool flip = false)
        {
            var w = Units.InchesToMeters(double.Parse(profileType.ToString().Substring(2).Split('X')[0].Replace('_', '.')));

            var L = Polygon.L(w, w, Units.InchesToMeters(THICKNESS));
            double flangeT = Units.InchesToMeters(THICKNESS);

            Transform right;
            Transform left;
            if (flip)
            {
                right = new Transform(Vector3.Origin);
                right.Rotate(Vector3.ZAxis, -90);
                right.Move(flangeT / 2);

                left = new Transform(Vector3.Origin);
                left.Rotate(Vector3.ZAxis, 180);
                left.Move(-flangeT / 2);
            }
            else
            {
                right = new Transform(Vector3.Origin);
                right.Move(flangeT / 2);

                left = new Transform(Vector3.Origin);
                left.Rotate(Vector3.ZAxis, 90);
                left.Move(-flangeT / 2);
            }
            return new Profile[] { L.TransformedPolygon(right), L.TransformedPolygon(left) };
        }

        private Representation ConstructRepresentation()
        {
            if (CellCount == 0)
            {
                return null;
            }

            JoistPoints.Clear();

            var ll = Construct2LProfile(TopChordProfile, true);

            var topSweepR = new Sweep(ll[0],
                                     Curve,
                                     StartSetback,
                                     EndSetback,
                                     Rotation,
                                     false);
            var topSweepL = new Sweep(ll[1],
                                     Curve,
                                     StartSetback,
                                     EndSetback,
                                     Rotation,
                                     false);

            var startT = Curve.TransformAt(0);
            Line line = (Line)Curve;
            var d = Units.InchesToMeters(double.Parse(Depth.ToString().Substring(2)));

            var topStart = line.Start - startT.ZAxis * Y;
            var topEnd = line.End + startT.ZAxis * Y;

            var bottomStart = line.Start - startT.YAxis * d - startT.ZAxis * Y;
            var bottomEnd = line.End - startT.YAxis * d + startT.ZAxis * Y;

            ll = Construct2LProfile(BottomChordProfile);

            var bottomChord = new Line(bottomStart, bottomEnd);
            var bottomSweepR = new Sweep(ll[0],
                                        bottomChord,
                                        0,
                                        0,
                                        Rotation,
                                        false);
            var bottomSweepL = new Sweep(ll[1],
                                        bottomChord,
                                        0,
                                        0,
                                        Rotation,
                                        false);

            // Use a line that is shorter than the curve length.
            var topGrid = new Grid1d(new Line(topStart, topEnd));
            topGrid.DivideByCount(CellCount);

            var bottomGrid = new Grid1d(bottomChord);
            bottomGrid.DivideByCount(CellCount);

            var topPts = topGrid.GetCellSeparators();
            var bottomPts = bottomGrid.GetCellSeparators();

            var solidOperations = new List<SolidOperation>() { topSweepL, topSweepR, bottomSweepL, bottomSweepR };

            Vector3 prevTop = default;
            Vector3 prevBottom = default;

            var wll = Construct2LProfile(WebProfile);

            for (var i = 0; i < topPts.Count; i++)
            {
                var topPt = topPts[i];
                var bottomPt = bottomPts[i];
                if (i % 2 == 0)
                {
                    prevTop = topPt;

                    // Vertical web
                    if (i != 0 && i != topPts.Count - 1)
                    {
                        var v1 = new Sweep(wll[0],
                                           new Line(topPt, bottomPt),
                                           0,
                                           0,
                                           0,
                                           false);
                        solidOperations.Add(v1);
                        var v2 = new Sweep(wll[1],
                                           new Line(topPt, bottomPt),
                                           0,
                                           0,
                                           0,
                                           false);
                        solidOperations.Add(v2);

                        JoistPoints.Add(topPt);
                    }

                    // Forward leaning web
                    if (i > 0)
                    {
                        var fl1 = new Sweep(wll[0],
                                                  new Line(prevBottom, topPt),
                                                  0,
                                                  0,
                                                  0,
                                                  false);
                        solidOperations.Add(fl1);
                        var fl2 = new Sweep(wll[1],
                                                  new Line(prevBottom, topPt),
                                                  0,
                                                  0,
                                                  0,
                                                  false);
                        solidOperations.Add(fl2);
                    }
                }
                else
                {
                    // Backward leaning web
                    var bl1 = new Sweep(wll[0],
                                             new Line(bottomPt, prevTop),
                                             0,
                                             0,
                                             0,
                                             false);
                    solidOperations.Add(bl1);
                    var bl2 = new Sweep(wll[1],
                                             new Line(bottomPt, prevTop),
                                             0,
                                             0,
                                             0,
                                             false);
                    solidOperations.Add(bl2);

                    prevBottom = bottomPt;
                }
            }

            var rep = new Representation(solidOperations);
            return rep;
        }

        /// <summary>
        /// Update the bar joist's representation.
        /// </summary>
        public override void UpdateRepresentations()
        {
            if (Representation.SolidOperations.Count == 0)
            {
                Representation = ConstructRepresentation();
            }
        }
    }
}