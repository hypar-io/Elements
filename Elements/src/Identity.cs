using System.Collections.Generic;


using OverrideDescriptor = System.Collections.Generic.Dictionary<string, object>;


namespace Elements
{
    /// <summary>
    /// Methods for discovering and tracking stable identifiers for Elements.
    /// </summary>
    public static class Identity
    {
        /// <summary>
        /// Track that an element was affected by a specific override.
        /// </summary>
        /// <param name="element">The element that was overridden.</param>
        /// <param name="overrideName">The name of the override property, from the hypar.json.</param>
        /// <param name="overrideId">The unique ID of the specific override within overrideName that is associated with this element.</param>
        /// <param name="overrideIdentity">The identity object that the override used to describe the object.</param>
        public static void AddOverrideIdentity(Element element, string overrideName, string overrideId, object overrideIdentity)
        {
            const string identitiesFieldName = "associatedIdentities";
            if (!element.AdditionalProperties.ContainsKey(identitiesFieldName))
            {
                element.AdditionalProperties[identitiesFieldName] = new Dictionary<string, List<OverrideDescriptor>>();
            }

            var identities = element.AdditionalProperties[identitiesFieldName] as Dictionary<string, List<OverrideDescriptor>>;
            if (!identities.ContainsKey(overrideName))
            {
                identities[overrideName] = new List<OverrideDescriptor>();
            }

            var overrideIdentities = identities[overrideName];
            overrideIdentities.Add(new OverrideDescriptor { { "id", overrideId }, { "identity", overrideIdentity } });
        }
    }
}