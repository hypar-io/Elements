namespace Elements
{
    /// <summary>
    /// The base class for all mapping classes. A mapping specifies additional
    /// data utilized to translate an element to a different application context or platform.
    /// </summary>
    public abstract class MappingBase : Element
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public MappingBase(System.Guid @id, string @name)
            : base(id, name) { }

        /// <summary>
        /// The default empty constructor.
        /// </summary>
        public MappingBase() { }

    }
}