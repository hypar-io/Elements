namespace Elements
{
    /// <summary>
    /// A singleton which provides integer Ids.
    /// </summary>
    public class IdProvider
    {
        private static int _currentId = 0;
        private static IdProvider _instance = null;

        private IdProvider(){}

        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static IdProvider Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new IdProvider();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Get the next valid integer Id.
        /// </summary>
        public int GetNextId()
        {
            var result = _currentId;
            _currentId++;
            return result;
        }

    }
}