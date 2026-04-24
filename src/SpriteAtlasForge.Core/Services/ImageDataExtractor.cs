using SkiaSharp;
using System;
using System.IO;

namespace SpriteAtlasForge.Core.Services;

/// <summary>
/// Extracts raw pixel data from images for computer vision analysis
/// </summary>
public class ImageDataExtractor
{
    /// <summary>
    /// Extract RGBA pixel data from image file
    /// Returns byte array in RGBA format (4 bytes per pixel)
    /// </summary>
    public (byte[] data, int width, int height) ExtractPixelData(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Image file not found: {filePath}");

        using var stream = File.OpenRead(filePath);
        using var original = SKBitmap.Decode(stream);

        if (original == null)
            throw new InvalidOperationException($"Failed to decode image: {filePath}");

        int width = original.Width;
        int height = original.Height;
        
        // Ensure we have RGBA8888 format
        var bitmap = original;
        if (original.ColorType != SKColorType.Rgba8888)
        {
            var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            bitmap = new SKBitmap(imageInfo);
            original.CopyTo(bitmap, SKColorType.Rgba8888);
        }

        // Allocate buffer
        var pixelData = new byte[width * height * 4];
        
        // Copy pixel data
        var pixelsPtr = bitmap.GetPixels();
        System.Runtime.InteropServices.Marshal.Copy(pixelsPtr, pixelData, 0, pixelData.Length);

        // Dispose temporary bitmap if created
        if (bitmap != original)
        {
            bitmap.Dispose();
        }

        return (pixelData, width, height);
    }

    /// <summary>
    /// Get color of specific pixel
    /// </summary>
    public SKColor GetPixel(byte[] data, int width, int x, int y)
    {
        int index = (y * width + x) * 4;
        return new SKColor(
            data[index],     // R
            data[index + 1], // G
            data[index + 2], // B
            data[index + 3]  // A
        );
    }

    /// <summary>
    /// Check if pixel is transparent (alpha below threshold)
    /// </summary>
    public bool IsTransparent(byte[] data, int width, int x, int y, byte threshold = 10)
    {
        int index = (y * width + x) * 4;
        return data[index + 3] <= threshold;
    }

    /// <summary>
    /// Get alpha value of pixel
    /// </summary>
    public byte GetAlpha(byte[] data, int width, int x, int y)
    {
        int index = (y * width + x) * 4;
        return data[index + 3];
    }
}
