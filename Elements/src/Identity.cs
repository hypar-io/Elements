using System.Collections.Generic;


using OverrideDescriptor = System.Collections.Generic.Dictionary<string, object>;


namespace Elements
{
    /// <summary>
    /// Methods for discovering and tracking stable identifiers for Elements.
    /// </summary>
    public static class Identity
    {
        private static string identitiesFieldName = "associatedIdentities";
        private static string valuesFieldName = "associatedValues";

        /// <summary>
        /// Track that an element was affected by a specific override.
        /// </summary>
        /// <param name="element">The element that was overridden.</param>
        /// <param name="overrideName">The name of the override property, from the hypar.json.</param>
        /// <param name="overrideId">The unique ID of the specific override within overrideName that is associated with this element.</param>
        /// <param name="overrideIdentity">The identity object that the override used to describe the object.</param>
        public static void AddOverrideIdentity(this Element element, string overrideName, string overrideId, object overrideIdentity)
        {
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

        /// <summary>
        /// Add the final values used from an override to an element or proxy.
        /// This is only required if the override schema does not reflect the element schema,
        /// or if the properties within the override were not added directly to additionalProperties.
        /// In the earlier two cases, the data on the element within the schema or in additionalProperties
        /// will be used as the current values for override.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="overrideName"></param>
        /// <param name="overrideValue"></param>
        public static void AddOverrideValue(this Element element, string overrideName, object overrideValue)
        {
            if (!element.AdditionalProperties.ContainsKey(valuesFieldName))
            {
                element.AdditionalProperties[valuesFieldName] = new Dictionary<string, object>();
            }
            var identities = element.AdditionalProperties[valuesFieldName] as Dictionary<string, object>;
            if (!identities.ContainsKey(overrideName))
            {
                identities[overrideName] = overrideValue;
            }
        }

        /// <summary>
        /// Get the override descriptors attached to this element. This means AddOverrideIdentity() was called on this element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="overrideName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static List<OverrideDescriptor> GetOverrideDescriptors<T>(this Element element, string overrideName)
        {
            if (!element.AdditionalProperties.ContainsKey(identitiesFieldName))
            {
                return null;
            }
            var identities = element.AdditionalProperties[identitiesFieldName] as Dictionary<string, List<OverrideDescriptor>>;
            if (!identities.ContainsKey(overrideName))
            {
                return null;
            }
            return identities[overrideName];
        }

        /// <summary>
        /// Get the IDs of all overrides of this type that were already attached using AddOverrideIdentity to this element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="overrideName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> GetOverrideIds<T>(this Element element, string overrideName)
        {
            var ids = new List<string>();

            var descriptors = element.GetOverrideDescriptors<T>(overrideName);

            if (descriptors != null && descriptors.Count > 0)
            {
                foreach (var descriptor in descriptors)
                {
                    if (descriptor.TryGetValue("id", out var id))
                    {
                        ids.Add(id.ToString());
                    }
                }
            }

            return ids;
        }
    }
}