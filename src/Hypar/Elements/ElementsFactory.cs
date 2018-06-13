using System;

namespace Hypar.Elements
{
    public static class ElementsFactory
    {
        /// <summary>
        /// Create a default grid.
        /// </summary>
        /// <returns></returns>
        public static Grid CreateGrid()
        {
            var perimeter = Profiles.Rectangular();
            var g = new Grid(perimeter);
            return g;
        }
        
        public static T CreateElementOfType<T>() where T: Element
        {
            var instance = Activator.CreateInstance<T>();
            return instance;
        }
    }
}