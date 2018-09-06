using System;

namespace AECSpaces
{
    /// <summary>
    /// ExceptionBoundarInvalid is thrown when a list of points cannot form a valid polygon.
    /// </summary>
    /// 
    public class ExceptionBoundaryInvalid : Exception
    {
        public ExceptionBoundaryInvalid(string message) : base(message) { }
    }//Exception

    /// <summary>
    /// ExceptionPointCount is thrown when a list of points is too short to form a valid polygon.
    /// </summary>
    /// 
    public class ExceptionPointCount: Exception
    {
        public ExceptionPointCount(string message): base(message) { }
    }//Exception
}//namespace

