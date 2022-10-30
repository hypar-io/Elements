using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Elements.Collections.Generics;
using glTFLoader;
using glTFLoader.Schema;

[assembly: InternalsVisibleTo("Hypar.Elements.Tests")]

namespace Elements.Serialization.glTF
{
    /// <summary>
    /// Extensions for glTF serialization.
    /// </summary>
    public static class GltfBufferExtensions
    {
        internal static void SaveBuffersAndAddUris(this Gltf gltf, string gltfPath, List<byte[]> buffers)
        {
            if (gltf.Buffers.Length != buffers.Count)
            {
                throw new InvalidDataException("There are not the same number of buffer byte arrays as there are buffers referenced in the gltf.  Make sure all buffers have corresponding byte arrays.");
            }
            for (int i = 0; i < gltf.Buffers.Length; i++)
            {
                var buffer = buffers[i];
                var binSaveName = Path.GetFileNameWithoutExtension(gltfPath) + $"_{i}.bin";
                gltf.Buffers[i].Uri = binSaveName;

                var binSaveDir = Path.GetDirectoryName(gltfPath);
                var binSavePath = Path.Combine(binSaveDir, binSaveName);
                if (File.Exists(binSavePath))
                {
                    File.Delete(binSavePath);
                }
                using (var fs = new FileStream(binSavePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(buffer, 0, buffer.Length);
                }
            }
        }

        internal static List<byte[]> GetAllBufferByteArrays(this Gltf gltf, Stream gltfStream)
        {
            var bufferByteArrays = new List<byte[]>();

            for (int i = 0; i < gltf.Buffers.Length; i++)
            {
                gltfStream.Position = 0;
                var byteArray = Interface.LoadBinaryBuffer(gltfStream);
                bufferByteArrays.Add(byteArray);
            }
            return bufferByteArrays;
        }

        internal static byte[] CombineBufferAndFixRefs(this Gltf gltf, List<byte[]> buffers)
        {
            if (buffers.All(b => b.Length == 0))
            {
                return new byte[0];
            }

            var fullBuffer = new byte[buffers.Sum(b => b.Length)];
            var index = 0;
            for (int i = 0; i < buffers.Count; i++)
            {
                var buff = buffers[i];
                if (i > 0)
                {
                    var referringViews = gltf.BufferViews.Where(bv => bv.Buffer == i);
                    foreach (var buffView in referringViews)
                    {
                        buffView.Buffer = 0;
                        buffView.ByteOffset += index;
                    }
                }

                System.Buffer.BlockCopy(buff, 0, fullBuffer, index, buff.Length);
                index += buff.Length;
            }
            var onlyBuffer = new Buffer
            {
                ByteLength = fullBuffer.Length
            };
            gltf.Buffers = new[] { onlyBuffer };

            return fullBuffer;
        }
    }
}