using System.Collections.ObjectModel;
using IxMilia.Dxf;

/// <summary>
/// Utilities for converting colors to DXF Colors. Courtesy of Patrick Stalph,
/// via https://github.com/IxMilia/Dxf/issues/97
/// </summary>
public static class DxfColorHelpers
{
    /// <summary>
    /// Find the closest DXF index color to a specified R,G,B value
    /// </summary>
    /// <param name="r">Red channel as a byte</param>
    /// <param name="g">Green channel as a byte</param>
    /// <param name="b">Blue channel as a byte</param>
    /// <returns>A DxfColor.</returns>
    public static DxfColor GetClosestDefaultIndexColor(byte r, byte g, byte b)
    {
        int minDist = int.MaxValue;
        int minIndex = -1;
        ReadOnlyCollection<uint> colors = DxfColor.DefaultColors;
        // index 0 is left out intentionally!
        for (int i = 1; i < colors.Count; i++)
        {
            int sqd = SquaredColorDistance(r, g, b, colors[i]);
            if (sqd == 0) // exact match
            {
                return DxfColor.FromIndex((byte)i);
            }
            if (sqd < minDist)
            {
                minDist = sqd;
                minIndex = i;
            }
        }
        return DxfColor.FromIndex((byte)minIndex);
    }

    private static int SquaredColorDistance(byte r, byte g, byte b, uint otherColor)
    {
        (byte r2, byte g2, byte b2) = ToRgb(otherColor);
        return (r - r2) * (r - r2)
             + (g - g2) * (g - g2)
             + (b - b2) * (b - b2);
    }

    private static (byte, byte, byte) ToRgb(uint color)
    {
        //byte a = (byte)(color >> 24);
        byte r = (byte)(color >> 16);
        byte g = (byte)(color >> 8);
        byte b = (byte)(color >> 0);
        return (r, g, b);
    }

    private static uint FromRgb(byte r, byte g, byte b)
    {
        const byte a = 0xFF; // alpha not used
        return (uint)(a << 24 | r << 16 | g << 8 | b << 0);
    }

    private static string ToHexString(this DxfColor color)
    {
        if (color.IsIndex)
        {
            uint argb = DxfColor.DefaultColors[color.Index];
            return ToHexString(argb);
        }
        return color.ToString();
    }

    private static string ToHexString(uint color)
    {
        return "0x" + color.ToString("X8");
    }
}