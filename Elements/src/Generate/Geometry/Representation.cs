using System.Collections;
using System.Collections.Generic;
using Elements.Geometry.Interfaces;

namespace Elements.Geometry
{
    public partial class Representation : IList<IRenderable>
    {
        private List<IRenderable> _renderables = new List<IRenderable>();

        public IRenderable this[int index] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public int Count => _renderables.Count;

        public bool IsReadOnly => false;

        public void Add(IRenderable item)
        {
            _renderables.Add(item);
        }

        public void Clear()
        {
            _renderables.Clear();
        }

        public bool Contains(IRenderable item)
        {
            return _renderables.Contains(item);
        }

        public void CopyTo(IRenderable[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<IRenderable> GetEnumerator()
        {
            return _renderables.GetEnumerator();
        }

        public int IndexOf(IRenderable item)
        {
            return _renderables.IndexOf(item);
        }

        public void Insert(int index, IRenderable item)
        {
            _renderables.Insert(index, item);
        }

        public bool Remove(IRenderable item)
        {
            _renderables.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _renderables.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _renderables.GetEnumerator();
        }
    }
}