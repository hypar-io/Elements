using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Elements.Flow;

[assembly: InternalsVisibleTo("Elements.MEP.Tests")]
namespace Elements.Fittings
{
    public partial class FittingTree
    {
        public string ToDotConnectors(bool includeAssemblies = false)
        {
            var allComponents = ExpandAssemblies(this.AllComponents);
            if (includeAssemblies)
            {
                allComponents = allComponents.Concat(this.AllComponents.Where(c => c is Assembly));
            }
            var allText = "digraph graphname {";

            var allPoints = allComponents.SelectMany(c => c.GetPorts().Select(p => p.Position.ToString())).Distinct().ToDictionary(c => c);

            foreach (var p in allPoints)
            {
                var newText = $"\"{p.Key}\" [label=\"XYZ\"]";
                allText = String.Join("\n", allText, newText);
            }

            foreach (var c in allComponents)
            {
                switch (c)
                {
                    case StraightSegment pipeSegment:
                        var pipeText = $"\"{pipeSegment.Start.Position.ToString()}\" -> \"{pipeSegment.End.Position.ToString()}\" [label=\"{c.Name}\"] ";
                        allText = String.Join("\n", allText, pipeText);
                        break;
                    case Elbow elbow:
                        var elbowText = $"\"{elbow.Start.Position.ToString()}\" -> \"{elbow.End.Position.ToString()}\" [label=\"{c.Name}\"] ";
                        allText = String.Join("\n", allText, elbowText);
                        break;
                    case Reducer reducer:
                        var reducerText = $"\"{reducer.Start.Position.ToString()}\" -> \"{reducer.End.Position.ToString()}\" [label=\"{c.Name}\"] ";
                        allText = String.Join("\n", allText, reducerText);
                        break;
                    case Wye wye:
                        var wyeText = $"\"{wye.MainBranch.Position.ToString()}\" -> \"{wye.Trunk.Position.ToString()}\" [label=\"{c.Name}\"] ";
                        allText = String.Join("\n", allText, wyeText);
                        wyeText = $"\"{wye.SideBranch.Position.ToString()}\" -> \"{wye.Trunk.Position.ToString()}\" [label=\"{c.Name}\"] ";
                        allText = String.Join("\n", allText, wyeText);
                        break;
                    default:
                        c.TryGetGuid(out var thisGuid);
                        var connectors = c.GetPorts().ToArray();
                        for (int i = 0; i < connectors.Count(); i++)
                        {
                            for (int j = i + 1; j < connectors.Count() - i; j++)
                            {
                                var iCon = connectors[i];
                                var jCon = connectors[j];
                                var newText = $"\"{iCon.Position.ToString()}\" -> \"{jCon.Position.ToString()}\" [label=\"{c.Name}\"] [dir=both]";
                                allText = String.Join("\n", allText, newText);
                            }
                        }
                        break;
                }
            }
            allText = String.Join("\n", allText, "}");
            return allText;
        }
        public string ToDot(bool includeAssemblies = false)
        {
            var alreadyLabeled = new HashSet<ComponentBase>();
            var allText = "digraph graphname {";

            var allComponents = ExpandAssemblies(this.AllComponents);
            if (includeAssemblies)
            {
                allComponents = allComponents.Concat(this.AllComponents.Where(c => c is Assembly));
            }

            foreach (var c in allComponents)
            {
                c.TryGetGuid(out var thisGuid);
                allText = string.Join("\n", allText, $"\"{thisGuid.ToString()}\" [label=\"{c.Name}\"]");
            }

            foreach (var c in allComponents)
            {
                c.TryGetGuid(out var thisGuid);
                if (c.TrunkSideComponent.TryGetGuid(out var nextId))
                {
                    allText = String.Join("\n", allText, $"\"{thisGuid.ToString()}\" -> \"{nextId.ToString()}\" [label=\"Ts\"]");
                }
                foreach (var branch in c.BranchSideComponents)
                {
                    if (branch.TryGetGuid(out var prevId))
                    {
                        allText = String.Join("\n", allText, $"\"{thisGuid.ToString()}\" -> \"{prevId.ToString()}\" [label=\"Bs\"]");
                    }
                }
            }
            allText = String.Join("\n", allText, "}");

            return allText;
        }
    }
}