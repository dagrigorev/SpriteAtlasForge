using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteAtlasForge.Core.Services;

/// <summary>
/// Background detection and removal using OpenCV
/// </summary>
public class BackgroundRemovalService
{
    private const int ColorTolerance = 30; // RGB tolerance for background color
    private const double BackgroundThreshold = 0.7; // 70% of border must be same color

    /// <summary>
    /// Detect dominant background color by analyzing border pixels
    /// </summary>
    public string? DetectBackgroundColor(byte[] imageData, int width, int height, int x, int y, int frameWidth, int frameHeight)
    {
        if (x + frameWidth > width || y + frameHeight > height)
            return null;

        var borderColors = new List<(byte r, byte g, byte b)>();

        // Sample top and bottom borders
        for (int px = x; px < x + frameWidth; px++)
        {
            // Top border
            int topIndex = (y * width + px) * 4;
            if (topIndex + 3 < imageData.Length && imageData[topIndex + 3] > 10)
            {
                borderColors.Add((imageData[topIndex], imageData[topIndex + 1], imageData[topIndex + 2]));
            }

            // Bottom border
            int bottomIndex = ((y + frameHeight - 1) * width + px) * 4;
            if (bottomIndex + 3 < imageData.Length && imageData[bottomIndex + 3] > 10)
            {
                borderColors.Add((imageData[bottomIndex], imageData[bottomIndex + 1], imageData[bottomIndex + 2]));
            }
        }

        // Sample left and right borders
        for (int py = y; py < y + frameHeight; py++)
        {
            // Left border
            int leftIndex = (py * width + x) * 4;
            if (leftIndex + 3 < imageData.Length && imageData[leftIndex + 3] > 10)
            {
                borderColors.Add((imageData[leftIndex], imageData[leftIndex + 1], imageData[leftIndex + 2]));
            }

            // Right border
            int rightIndex = (py * width + (x + frameWidth - 1)) * 4;
            if (rightIndex + 3 < imageData.Length && imageData[rightIndex + 3] > 10)
            {
                borderColors.Add((imageData[rightIndex], imageData[rightIndex + 1], imageData[rightIndex + 2]));
            }
        }

        if (borderColors.Count < 10)
            return null;

        // Find most common color
        var colorGroups = borderColors
            .GroupBy(c => (c.r / 10, c.g / 10, c.b / 10)) // Group similar colors
            .OrderByDescending(g => g.Count())
            .ToList();

        var dominantGroup = colorGroups.First();
        
        // Check if it's dominant enough (>70% of border)
        if (dominantGroup.Count() < borderColors.Count * BackgroundThreshold)
            return null;

        // Calculate average color in dominant group
        var avgR = (byte)dominantGroup.Average(c => c.r);
        var avgG = (byte)dominantGroup.Average(c => c.g);
        var avgB = (byte)dominantGroup.Average(c => c.b);

        return $"#{avgR:X2}{avgG:X2}{avgB:X2}";
    }

    /// <summary>
    /// Calculate auto-trim bounds by finding actual sprite content
    /// </summary>
    public (int left, int right, int top, int bottom) CalculateAutoTrim(
        byte[] imageData, int width, int height, 
        int x, int y, int frameWidth, int frameHeight,
        string? backgroundColor = null)
    {
        byte? bgR = null, bgG = null, bgB = null;
        
        if (backgroundColor != null && backgroundColor.StartsWith("#") && backgroundColor.Length == 7)
        {
            bgR = Convert.ToByte(backgroundColor.Substring(1, 2), 16);
            bgG = Convert.ToByte(backgroundColor.Substring(3, 2), 16);
            bgB = Convert.ToByte(backgroundColor.Substring(5, 2), 16);
        }

        int minX = frameWidth;
        int maxX = 0;
        int minY = frameHeight;
        int maxY = 0;

        bool foundContent = false;

        // Scan frame for opaque/non-background pixels
        for (int py = 0; py < frameHeight; py++)
        {
            for (int px = 0; px < frameWidth; px++)
            {
                int imgX = x + px;
                int imgY = y + py;

                if (imgX >= width || imgY >= height)
                    continue;

                int index = (imgY * width + imgX) * 4;
                if (index + 3 >= imageData.Length)
                    continue;

                byte r = imageData[index];
                byte g = imageData[index + 1];
                byte b = imageData[index + 2];
                byte a = imageData[index + 3];

                bool isContent = false;

                if (bgR.HasValue)
                {
                    // Check if pixel is NOT background color
                    int rDiff = Math.Abs(r - bgR.Value);
                    int gDiff = Math.Abs(g - bgG.Value);
                    int bDiff = Math.Abs(b - bgB.Value);

                    isContent = a > 10 && (rDiff > ColorTolerance || gDiff > ColorTolerance || bDiff > ColorTolerance);
                }
                else
                {
                    // Use alpha only
                    isContent = a > 10;
                }

                if (isContent)
                {
                    minX = Math.Min(minX, px);
                    maxX = Math.Max(maxX, px);
                    minY = Math.Min(minY, py);
                    maxY = Math.Max(maxY, py);
                    foundContent = true;
                }
            }
        }

        if (!foundContent)
            return (0, 0, 0, 0);

        int trimLeft = minX;
        int trimRight = frameWidth - maxX - 1;
        int trimTop = minY;
        int trimBottom = frameHeight - maxY - 1;

        return (trimLeft, trimRight, trimTop, trimBottom);
    }

    /// <summary>
    /// Remove background by making matching pixels transparent
    /// </summary>
    public byte[] RemoveBackground(
        byte[] imageData, int width, int height,
        int x, int y, int frameWidth, int frameHeight,
        string backgroundColor)
    {
        if (!backgroundColor.StartsWith("#") || backgroundColor.Length != 7)
            return imageData;

        byte bgR = Convert.ToByte(backgroundColor.Substring(1, 2), 16);
        byte bgG = Convert.ToByte(backgroundColor.Substring(3, 2), 16);
        byte bgB = Convert.ToByte(backgroundColor.Substring(5, 2), 16);

        var result = (byte[])imageData.Clone();

        for (int py = 0; py < frameHeight; py++)
        {
            for (int px = 0; px < frameWidth; px++)
            {
                int imgX = x + px;
                int imgY = y + py;

                if (imgX >= width || imgY >= height)
                    continue;

                int index = (imgY * width + imgX) * 4;
                if (index + 3 >= result.Length)
                    continue;

                byte r = result[index];
                byte g = result[index + 1];
                byte b = result[index + 2];

                // Check if pixel matches background color
                int rDiff = Math.Abs(r - bgR);
                int gDiff = Math.Abs(g - bgG);
                int bDiff = Math.Abs(b - bgB);

                if (rDiff <= ColorTolerance && gDiff <= ColorTolerance && bDiff <= ColorTolerance)
                {
                    // Make transparent
                    result[index + 3] = 0;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Detect and apply auto-trim for all frames in a group
    /// </summary>
    public void AutoTrimFrames(
        byte[] imageData, int width, int height,
        IList<Models.SpriteFrame> frames,
        bool detectBackground = true)
    {
        foreach (var frame in frames)
        {
            // Detect background color if needed
            string? bgColor = null;
            if (detectBackground)
            {
                bgColor = DetectBackgroundColor(imageData, width, height, 
                    frame.X, frame.Y, frame.Width, frame.Height);
                
                if (bgColor != null)
                {
                    frame.BackgroundColor = bgColor;
                }
            }
            else
            {
                bgColor = frame.BackgroundColor;
            }

            // Calculate trim
            var (left, right, top, bottom) = CalculateAutoTrim(
                imageData, width, height,
                frame.X, frame.Y, frame.Width, frame.Height,
                bgColor);

            frame.ApplyAutoTrim(left, right, top, bottom);

            System.Diagnostics.Debug.WriteLine(
                $"[AutoTrim] Frame {frame.Name}: " +
                $"BG={bgColor ?? "none"}, " +
                $"Trim=L{left}R{right}T{top}B{bottom}");
        }
    }
}
