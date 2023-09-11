using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements
{
    public partial class DrainableRoofSection
    {
        public static bool Render;

        public MergeEnum MergeType { get; set; }

        public enum MergeEnum
        {
            None,

            VirtualSplit
        }

        public override void UpdateRepresentations()
        {
            if (Render)
            {
                Representation = new Representation(new List<SolidOperation>());
                Representation.SolidOperations.Add(new Lamina(Boundary, false));
            }
            else
            {
                Representation = null;
            }
        }

        public List<Line> GetLowLines()
        {
            if (Parent == null)
            {
                return InteriorLowLines.Union(PerimeterLowLines).ToList();
            }

            return Parent.GetLowLines().SelectMany(l => l.Trim(Boundary, out _)).ToList();
        }

        // TODO Add Children to DRS instead of using childSectionLookup everywhere:
        // Make "Children" available on the DrainableRoofSection,
        // make sure it doesn't serialize, and have a method something like "FillInChildren" 
        // that you can run once at the beginning of a function, and then never have to think about again.
        public static Dictionary<DrainableRoofSection, List<DrainableRoofSection>> GetChildSectionLookup(IEnumerable<DrainableRoofSection> sections)
        {
            var childSectionLookup = sections.ToDictionary(rs => rs, rs => new List<DrainableRoofSection>());
            foreach (var section in sections.Where(s => s.Parent != null))
            {
                childSectionLookup[section.Parent].Add(section);
            }

            return childSectionLookup;
        }

        public IEnumerable<DrainableRoofSection> GetAncestors()
        {
            var list = new List<DrainableRoofSection>();
            DrainableRoofSection node = this;
            while ((node = node.Parent) != null)
            {
                list.Add(node);
            }
            return list;
        }

        public IEnumerable<DrainableRoofSection> GetLeafChildrenFlattened(Dictionary<DrainableRoofSection, List<DrainableRoofSection>> childSectionLookup)
        {
            var leafNodes = new List<DrainableRoofSection>();
            if (!childSectionLookup.TryGetValue(this, out var children))
            {
                children = childSectionLookup.FirstOrDefault(p => p.Key.Id.Equals(Id)).Value ?? new List<DrainableRoofSection>();
            }

            if (!children.Any())
            {
                leafNodes.Add(this);
            }
            else
            {
                foreach (var child in children)
                {
                    leafNodes.AddRange(child.GetLeafChildrenFlattened(childSectionLookup));
                }
            }
            return leafNodes;
        }
    }
}