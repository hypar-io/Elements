namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// The renderable data for one object, provided as a set of buffers and bounds.
    /// </summary>
    public class RenderData
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
    }

    /// <summary>
    /// An object which creates renderable content.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Construct a render data package.
        /// </summary>
        /// <returns></returns>
        RenderData Render();
    }
}