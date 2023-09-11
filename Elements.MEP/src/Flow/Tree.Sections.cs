using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Flow
{
    public partial class Tree
    {
        private Section[] _sections;
        private Dictionary<Connection, Section> _connectionSectionLookup;

        /// <summary>
        /// Get all of the tree FlowSections.
        /// </summary>
        /// <param name="forceUpdate"></param>
        /// <param name="sortClosestToFurthest">Optionally sort the sections in order of the sections distance from the trunk.</param>
        /// <returns></returns>
        public Section[] GetSections(bool forceUpdate = false, bool sortClosestToFurthest = false)
        {
            if (_sections == null || forceUpdate)
            {
                UpdateSections();
            }

            if (sortClosestToFurthest)
            {
                var endSection = GetSectionFromKey("0");
                var distances = _sections.Select(s => (CumulativeDistance: AllSectionsBetweenSections(s, endSection).Sum(subSec => subSec.Path.Length()), Section: s));
                var sorted = distances.OrderBy(s => s.CumulativeDistance).Select(s => s.Section).ToArray();
                return sorted;
            }

            return _sections;
        }

        /// <summary>
        /// Get all of the tree loop sections.
        /// </summary>
        /// <returns></returns>
        public List<Section> GetLoopSections(bool forceUpdate = false)
        {
            return GetSections(forceUpdate).Where(s => GetConnectionsForSection(s).Any(c => c.IsLoop == true)).ToList();
        }

        public double GetFlowOfSectionReference(string sectionReference)
        {
            var section = GetSectionFromKey(sectionReference);
            return section.Flow;
        }

        public double GetFlowOfConnection(Connection connection, bool forceUpdate = false)
        {
            return GetSectionFromConnection(connection, forceUpdate).Flow;
        }

        public Section GetSectionFromConnection(Connection connection, bool forceUpdate = false)
        {
            if (connection == null)
            {
                return null;
            }
            if (forceUpdate || _sections == null || _connectionSectionLookup == null)
            {
                UpdateSections();
            }
            return _connectionSectionLookup[connection];
        }

        private Dictionary<string, Section> _keySectionLookup;
        public Section GetSectionFromKey(string sectionKey)
        {
            if (_sections == null || _keySectionLookup == null)
            {
                GetSections();
            }
            var splitKey = sectionKey.Split(':');
            if (splitKey.Count() > 2)
            {
                throw new System.InvalidOperationException("The section key has too many ':' characters, only 1 is allowed");
            }
            var keyToLookup = splitKey.Last();
            if (_keySectionLookup.ContainsKey(keyToLookup))
            {
                return _keySectionLookup[keyToLookup];
            }
            else
            {
                return null;
            }
        }

        private Section[] AllSectionsBetweenSections(Section startSection, Section endSection)
        {
            var sections = new List<Section>();
            var currentSection = startSection;
            while (currentSection != endSection && currentSection != null)
            {
                sections.Add(currentSection);
                currentSection = GetTrunkSideSection(currentSection);
            }
            return sections.ToArray();
        }

        private Section GetTrunkSideSection(Section currentSection)
        {
            var lastComma = currentSection.SectionKey.LastIndexOf(',');
            if (lastComma == -1)
            {
                return null;
            }
            var shortenedKey = currentSection.SectionKey.Substring(0, lastComma);
            return GetSectionFromKey(shortenedKey);
        }

        /// <summary>
        /// Updates the FlowSections of the tree.  It traverses the connections
        /// of the tree and creates a Section for each component.
        /// </summary>
        /// <returns></returns>
        public void UpdateSections()
        {
            var rootNode = this.Outlet;

            var allSections = new List<Section>();

            _connectionSectionLookup = new Dictionary<Connection, Section>();
            var currentSection = new Section(this, "0");
            currentSection.End = this.Outlet;
            var newSections = RecursiveGetSections(this.Outlet, currentSection);

            _sections = newSections.Select(section => SetPathOnSection(section)).ToArray();
            _keySectionLookup = _sections.ToDictionary(s => s.SectionKey);
            SetFlowConnectionFlows();
        }

        private void SetFlowConnectionFlows()
        {
            foreach (var s in _sections)
            {
                var i = 0;
                foreach (var c in GetConnectionsForSection(s).Reverse())
                {
                    c.Flow = s.Flow;
                    c.ComponentLocator = new ConnectionLocator(s, c.Path());
                    i++;
                }
            }
        }

        /// <summary>
        /// Retrieves all of the FlowConnections for a given section ordered from Start to End.
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Connection[] GetConnectionsForSection(Section section)
        {
            var allConnections = new List<Connection>();
            Node currentNode = section.Start;
            while (currentNode != section.End)
            {
                var thisConnections = GetOutgoingConnections(currentNode);
                Connection thisConnection;
                if (thisConnections.Count == 1)
                {
                    thisConnection = thisConnections.First();
                }
                else
                {
                    thisConnection = thisConnections.First(c => _connectionSectionLookup[c].SectionKey.Equals(section.SectionKey));
                }
                allConnections.Add(thisConnection);
                currentNode = thisConnection.End;
            }
            return allConnections.ToArray();
        }

        public Connection[] GetConnectionsForSectionKey(string sectionKey)
        {
            var section = GetSectionFromKey(sectionKey);
            return GetConnectionsForSection(section);
        }

        private Section SetPathOnSection(Section section)
        {
            var allPath = new List<Vector3> { section.Start.Position };
            Node currentNode = section.Start;
            while (currentNode != section.End)
            {
                var connections = GetOutgoingConnections(currentNode);
                var connection = connections.FirstOrDefault(c => _connectionSectionLookup[c].SectionKey.Equals(section.SectionKey));
                if (connection == null)
                {
                    break;
                }
                allPath.Add(connection.End.Position);
                currentNode = connection.End;
            }
            section.Path = new Polyline(allPath);

            if (section.HintPath == null)
                section.HintPath = new Polyline(allPath);

            return section;
        }

        private List<Section> RecursiveGetSections(Node lastNode, Section current)
        {
            var incoming = GetIncomingConnections(lastNode);
            if (incoming.Count == 0)
            {
                current.Start = lastNode;
                if (lastNode is Leaf leaf)
                {
                    current.Flow = leaf.Flow;
                }
                return new List<Section> { current };
            }
            else if (incoming.Count == 1 && GetOutgoingConnections(lastNode).Count <= 1)
            {
                if (_connectionSectionLookup.ContainsKey(incoming[0]))
                {
                    return new List<Section> { };
                }
                _connectionSectionLookup[incoming[0]] = current;
                var sections = RecursiveGetSections(incoming[0].Start, current);
                return sections;
            }
            else
            {
                current.Start = lastNode;
                var allDownstream = new List<Section>();
                var outgoing = GetOutgoingConnection(lastNode);
                var sorted = incoming.OrderBy(c => c.IsLoop == true).ThenBy(c => c, new ConnectionComparer(outgoing, true)).ToArray();
                for (int i = 0; i < sorted.Count(); i++)
                {
                    var income = sorted[i];
                    var newSection = new Section(this, current.SectionKey + $",{i}");
                    newSection.End = income.End;
                    if (_connectionSectionLookup.ContainsKey(income))
                    {
                        continue;
                    }
                    _connectionSectionLookup[income] = newSection;
                    var downstreamSections = RecursiveGetSections(income.Start, newSection);
                    allDownstream.AddRange(downstreamSections);

                    current.Flow += downstreamSections.Where(s => current.IsDirectlyUpstream(s)).Sum(s => s.Flow);
                }
                allDownstream.Add(current);
                return allDownstream;
            }
        }
    }
}