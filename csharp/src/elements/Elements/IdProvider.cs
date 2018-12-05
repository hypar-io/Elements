namespace Hypar.Elements
{
    public class IdProvider
    {
        private static int _currentId = 0;
        private static IdProvider _instance = null;

        private IdProvider(){}

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

        public int GetNextId()
        {
            var result = _currentId;
            _currentId++;
            return result;
        }

    }
}