using System.IO;

namespace Elements.MEP.Tests
{

    public class TestUtils
    {
        public static string GetTestPath(string directoryName = null)
        {
            var path = "../../../TestResults/";
            if (directoryName != null)
            {
                path = Path.Combine(path, directoryName);
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}