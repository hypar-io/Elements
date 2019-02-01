using Elements;

namespace Hypar.Elements.Interfaces
{
    /// <summary>
    /// Represents collection of Openings.
    /// </summary>
    public interface IOpeningsProvider
    {
        /// <summary>
        /// A collection of Openings which are transformed in the coordinate system of their host Element.
        /// </summary>
        Opening[] Openings{get;}
    }
}