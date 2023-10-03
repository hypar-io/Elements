using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Flow
{
    public partial class Section
    {
        public static Material SectionMaterial = new Material("Section", new Color(0.2, 0.666, 1.0, 1.0));
        public static double DefaultRepresentationDiameter = 0.05;

        public Section(Tree tree, string initialDescriptorValue) : base(new Transform(), SectionMaterial, null, false, Guid.NewGuid(), "")
        {
            Tree = tree;
            SectionKey = initialDescriptorValue;
        }

        public override string ToString()
        {
            return $"Section: {SectionKey}, Flow: {Flow}, Network: {Tree.Purpose} {Tree.GetNetworkReference()}";
        }

        internal bool IsDirectlyUpstream(Section s)
        {
            return s.End == this.Start;
        }

        public override void UpdateRepresentations()
        {
            if (this.Representation == null)
            {
                this.Representation = new Representation(new List<SolidOperation>());
            }
            this.Representation.SolidOperations = new List<SolidOperation>();

            var pipeProfile = new Circle(new Vector3(), DefaultRepresentationDiameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var sameStartEnd = 0;

            foreach (var conn in this.Path.Segments())
            {
                if (conn.End != conn.Start)
                {
                    var centerLine = new Line(conn.End, conn.Start);
                    var pipe = new Sweep(pipeProfile, centerLine, 0, 0, 0, false);
                    this.Representation.SolidOperations.Add(pipe);
                }
                else
                {
                    sameStartEnd++;
                    Console.WriteLine("Start and end were the same");
                }
            }
        }

    }
}