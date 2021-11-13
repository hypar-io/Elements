namespace Elements
{
    /// <summary>
    /// An override value object
    /// </summary>
    public interface IOverride
    {
        /// <summary>
        /// The override Id
        /// </summary>
        /// <value></value>
        string Id { get; set; }

        /// <summary>
        /// The override's identity
        /// </summary>
        /// <returns></returns>
        object GetIdentity();

        /// <summary>
        /// The name of the override
        /// </summary>
        /// <returns></returns>
        string GetName();
    }
}