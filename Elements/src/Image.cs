using System;
using System.IO;
using System.Net;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Elements
{
    /// <summary>
    /// An image.
    /// </summary>
    internal class Image
    {
        /// <summary>
        /// Create an image.
        /// </summary>
        /// <param name="uri">The URI of the image.</param>
        /// <returns>An image or null if an image cannot be created from the provided URI.</returns>
        internal static Image<Rgba32> CreateFromUri(Uri uri)
        {
            if (!RemoteFileExists(uri))
            {
                return null;
            }

            var webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(uri);

            // We don't wrap this in a using statement because
            // we hold onto the image in other places.
            var texImage = SixLabors.ImageSharp.Image.Load(imageBytes);

            // Flip the texture image vertically
            // to align with OpenGL convention.
            // 0,1  1,1
            // 0,0  1,0
            texImage.Mutate(x => x.Flip(FlipMode.Vertical));

            return texImage;
        }

        /// <summary>
        /// Create an image.
        /// </summary>
        /// <param name="data">A byte array containing the image data.</param>
        /// <returns>An image.</returns>
        internal static Image<Rgba32> CreateFromBytes(byte[] data)
        {
            var texImage = SixLabors.ImageSharp.Image.Load(data);
            return texImage;
        }

        internal static bool RemoteFileExists(Uri uri)
        {
            if (uri.IsFile)
            {
                var localPath = uri.LocalPath;
                return File.Exists(localPath);
            }

            // https://stackoverflow.com/questions/924679/c-sharp-how-can-i-check-if-a-url-exists-is-valid
            try
            {
                HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                var ok = response.StatusCode == HttpStatusCode.OK;
                response.Close();
                return ok;
            }
            catch (Exception ex)
            {
                //Any exception will returns false.
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}