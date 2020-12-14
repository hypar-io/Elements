namespace Elements.Serialization.glTF
{
    /// <summary>
    /// The renderable data for one object, provided as a set of buffers and bounds.
    /// </summary>
    public class GlMeshRenderData
    {
        byte[] VertexBuffer { get; set; }
        byte[] IndexBuffer { get; set; }
        byte[] NormalBuffer { get; set; }
        byte[] ColorBuffer { get; set; }
        byte[] UVBuffer { get; set; }
        double[] VMax { get; set; }
        double[] VMin { get; set; }
        double[] NMin { get; set; }
        double[] NMax { get; set; }
        float[] CMin { get; set; }
        float[] CMax { get; set; }
        ushort IMin { get; set; }
        ushort IMax { get; set; }
        double[] UVMin { get; set; }
        double[] UVMax { get; set; }

        /// <summary>
        /// Construct a GL render data.
        /// </summary>
        /// <param name="vertexBuffer"></param>
        /// <param name="indexBuffer"></param>
        /// <param name="colorBuffer"></param>
        /// <param name="uvBuffer"></param>
        /// <param name="vMax"></param>
        /// <param name="vMin"></param>
        /// <param name="nMin"></param>
        /// <param name="nMax"></param>
        /// <param name="cMin"></param>
        /// <param name="cMax"></param>
        /// <param name="iMin"></param>
        /// <param name="iMax"></param>
        /// <param name="uvMin"></param>
        /// <param name="uvMax"></param>
        public GlMeshRenderData(byte[] vertexBuffer,
                            byte[] indexBuffer,
                            byte[] colorBuffer,
                            byte[] uvBuffer,
                            double[] vMax,
                            double[] vMin,
                            double[] nMin,
                            double[] nMax,
                            float[] cMin,
                            float[] cMax,
                            ushort iMin,
                            ushort iMax,
                            double[] uvMin,
                            double[] uvMax)
        {
            this.VertexBuffer = vertexBuffer;
            this.IndexBuffer = indexBuffer;
            this.ColorBuffer = colorBuffer;
            this.UVBuffer = uvBuffer;
            this.VMax = vMax;
            this.VMin = vMin;
            this.NMax = nMax;
            this.NMin = nMin;
            this.CMin = cMin;
            this.CMax = cMax;
            this.IMin = iMin;
            this.IMax = IMax;
            this.UVMin = uvMin;
            this.UVMax = uvMax;
        }
    }
}