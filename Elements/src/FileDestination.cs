using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A destination for an output file.
    /// </summary>
    public class FileDestination
    {
        /// <summary>
        /// Construct a file destination given a key.
        /// </summary>
        /// <param name="key"></param>
        public FileDestination(string key) => Key = key;

        /// <summary>
        /// The key of the file.
        /// </summary>
        [JsonProperty("Key")]
        public string Key { get; set; }

        /// <summary>
        /// The upload Url of the file
        /// </summary>
        [JsonProperty("UploadUrl")]
        public string UploadUrl { get; set; }

        /// <summary>
        /// The expected file extension of the destination.
        /// </summary>
        [JsonProperty("Extension")]
        public string Extension { get; set; }
        private Stream _exportStream { get; set; }

        /// <summary>
        /// Assign a stream to be exported to the destination.
        /// </summary>
        /// <param name="stream">The data stream that will be exported to a file.</param>
        public void SetExportStream(Stream stream)
        {
            _exportStream = stream;
        }

        /// <summary>
        /// Assign text to be exported to the destination.
        /// </summary>
        /// <param name="text">The text to be exported.</param>
        public void SetExportTextContents(string text)
        {
            _exportStream = new System.IO.MemoryStream();
            var writer = new StreamWriter(_exportStream);
            writer.Write(text);
            writer.Flush();
            _exportStream.Position = 0;
        }

        /// <summary>
        /// Retrieve the current stream to be exported to the destination.
        /// </summary>
        public Stream GetExportStream()
        {
            return _exportStream;
        }
    }
}