using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elements.Flow;

namespace Elements.Fittings
{
    public partial class FittingTree
    {
        public struct PipeNetworkComponentGroup
        {
            public PipeNetworkComponentGroup(List<ComponentBase> components, bool vertical)
            {
                Components = components;
                Vertical = vertical;
            }

            public List<ComponentBase> Components
            {
                get; private set;
            }

            public bool Vertical
            {
                get; internal set;
            }
        }

        /// <summary>
        /// Returns groups of INetworkComponents that are all of vertical or horizontal stretches of the section.
        /// A vertical stretch will include any element at the top or bottom that has a change greater than the allowedHeightVariation
        /// </summary>
        /// <param name="section"></param>
        /// <param name="allowedHeightVariation"></param>
        /// <returns>Component groups, marked as vertical or horizontal, ordered Branchside to Trunkside.</returns>
        /// <exception cref="InvalidDataException"></exception>
        public List<PipeNetworkComponentGroup> GetOrientedGroupsOfPipingFromSection(
            Section section, double allowedHeightVariation = 0.05)
        {
            var components = GetComponentsOfSection(section).Reverse(); // components are ordered Trunkside to Branchside, we want the opposite.
            var stretches = new List<PipeNetworkComponentGroup>();

            // find the starting z of the section
            ComponentBase headTerminal = null;
            var currentZ = GetStartingConnectorHeight(components.First(), out var headless);
            // if there is no branch side connector - this is terminal.
            if (headless)
            {
                headTerminal = components.First();
            }

            var currentHorizontalStretch = new List<ComponentBase>();
            var currentVerticalStretch = new List<ComponentBase>();

            foreach (var c in components)
            {
                // terminal will be put into a group based on the next component.
                if (c == headTerminal)
                {
                    continue;
                }

                var heightOfNextConnector = c?.TrunkSidePort()?.Position.Z;

                // if the height of the next connector has no value or the connector is a zero length coupler,
                // then add it to whatever stretch we are in.
                if (!heightOfNextConnector.HasValue || (c is Coupler && c.GetLength() == 0))
                {
                    if (currentHorizontalStretch.Count > 0)
                    {
                        currentHorizontalStretch.Add(c);
                        continue;
                    }
                    else if (currentVerticalStretch.Count > 0)
                    {
                        currentVerticalStretch.Add(c);
                        continue;
                    }
                }

                // if the height of the next connector is different than the current z, we are in a vertical stretch
                // if the component is a pipe that is vertical we also consider this vertical even if the pipe is
                // shorter than the allowed height variation.
                if (
                 Math.Abs(heightOfNextConnector.Value - currentZ) > allowedHeightVariation
                || (!heightOfNextConnector.Value.ApproximatelyEquals(currentZ)) && currentVerticalStretch.Count > 0
                || (c is StraightSegment segment && segment.IsVertical()))
                {
                    if (currentHorizontalStretch.Count > 0)
                    {
                        stretches.Add(new PipeNetworkComponentGroup(currentHorizontalStretch, false));
                        currentHorizontalStretch = new List<ComponentBase>();
                    }
                    currentZ = heightOfNextConnector ?? currentZ;
                    // put postponed head terminal first and clear it.
                    if (headTerminal != null)
                    {
                        currentVerticalStretch.Add(headTerminal);
                        headTerminal = null;
                    }
                    currentVerticalStretch.Add(c);
                }
                else
                {
                    if (currentVerticalStretch.Count > 0)
                    {
                        stretches.Add(new PipeNetworkComponentGroup(currentVerticalStretch, true));
                        currentVerticalStretch = new List<ComponentBase>();
                    }
                    // put postponed head terminal first and clear it.
                    if (headTerminal != null)
                    {
                        currentHorizontalStretch.Add(headTerminal);
                        headTerminal = null;
                    }
                    currentHorizontalStretch.Add(c);
                }
            }

            if (currentVerticalStretch.Count > 0)
            {
                stretches.Add(new PipeNetworkComponentGroup(currentVerticalStretch, true));
            }
            if (currentHorizontalStretch.Count > 0)
            {
                stretches.Add(new PipeNetworkComponentGroup(currentHorizontalStretch, false));
            }

            if (stretches.Sum(s => s.Components.Count) != components.Count())
            {
                throw new InvalidDataException(
                    "The number of components in the vertical and horizontal stretches do not match the number of components in the section.");
            }

            return stretches;
        }

        /// <summary>
        /// Gets the highest branchside connector of the component or the height of the trunk connector if no branchside connectors are found.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        private static double GetStartingConnectorHeight(ComponentBase component, out bool headless)
        {
            if (component.BranchSideComponents.Count == 0)
            {
                headless = true;
                return component.TrunkSidePort().Position.Z;
            }
            else
            {
                headless = false;
                return component.BranchSideComponents.Select(c => c.TrunkSidePort().Position.Z).Max();
            }
        }
    }
}