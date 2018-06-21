namespace Hypar.Elements
{
    /// <summary>
    /// Common exception messages.
    /// </summary>
    public static class Messages
    {   
        /// <summary>
        /// 
        /// </summary>
        public const string EMPTY_POLYLINE_EXCEPTION = "You've supplied an empty Polyline as a profile. Profiles must contain at least three non-coincident points.";
        
        /// <summary>
        /// 
        /// </summary>
        public const string TOP_BELOW_BOTTOM_EXCEPTION = "You've supplied a top elevation which is below the bottom elevation.";
        
        /// <summary>
        /// 
        /// </summary>
        public const string BOTTOM_ABOVE_TOP_EXCEPTION = "You've supplied a bottom elevation which is above the top elevation.";
        
        /// <summary>
        /// 
        /// </summary>
        public const string PROFILES_UNEQUAL_VERTEX_EXCEPTION = "You've provided profiles with unequal number of vertices. Profiles must have the same number of vertices.";
        
        /// <summary>
        /// 
        /// </summary>
        public const string ZERO_THICKNESS_EXCEPTION = "You've provided a zero thickness. Thickness must be greater than zero.";
   }

}