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

        internal static List<byte[]> GetAllBufferByteArrays(this Gltf gltf, string gltfPath)
        {
            var bufferByteArrays = new List<byte[]>();

            for (int i = 0; i < gltf.Buffers.Length; i++)
            {
                var byteArray = Interface.LoadBinaryBuffer(gltf, 0, gltfPath);
                bufferByteArrays.Add(byteArray);
            }
            return bufferByteArrays;
        }

        internal static byte[] CombineBufferAndFixRefs(this Gltf gltf, byte[][] buffers)
        {
            var fullBuffer = new List<byte>();
            for (int i = 0; i < buffers.Length; i++)
            {
                var buff = buffers[i];
                if (i > 0)
                {
                    var referringViews = gltf.BufferViews.Where(bv => bv.Buffer == i);
                    foreach (var buffView in referringViews)
                    {
                        buffView.Buffer = 0;
                        buffView.ByteOffset = buffView.ByteOffset + fullBuffer.Count;
                    }
                }
                fullBuffer.AddRange(buff);
            }
            var onlyBuffer = new Buffer();
            onlyBuffer.ByteLength = fullBuffer.Count;
            gltf.Buffers = new[] { onlyBuffer };
            return fullBuffer.ToArray(fullBuffer.Count);
        }
    }
}