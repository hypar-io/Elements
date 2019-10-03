#pragma warning disable 1591

using System.Collections;
using System.Collections.Generic;

namespace Elements.Properties
{
    public partial class PropertySet : IDictionary<string, Property>
    {
        private Dictionary<string,Property> _properties = new Dictionary<string, Property>();

        public Property this[string key] { get => _properties[key]; set => _properties[key] = value; }

        public ICollection<string> Keys => _properties.Keys;

        public ICollection<Property> Values => _properties.Values;

        public int Count => _properties.Count;

        public bool IsReadOnly => false;

        public void Add(string key, Property value)
        {
            _properties.Add(key, value);
        }

        public void Add(KeyValuePair<string, Property> item)
        {
            _properties.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _properties.Clear();
        }

        public bool Contains(KeyValuePair<string, Property> item)
        {
            return _properties.ContainsValue(item.Value);
        }

        public bool ContainsKey(string key)
        {
            return _properties.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, Property>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, Property>> GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _properties.Remove(key);
        }

        public bool Remove(KeyValuePair<string, Property> item)
        {
            return _properties.Remove(item.Key);
        }

        public bool TryGetValue(string key, out Property value)
        {
            return _properties.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _properties.GetEnumerator();
        }
    }
}