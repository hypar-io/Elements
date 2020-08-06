using System;

namespace Elements
{
    /// <summary>
    /// An attribute which defines an element as a user-defined element type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UserElement : Attribute{}
}