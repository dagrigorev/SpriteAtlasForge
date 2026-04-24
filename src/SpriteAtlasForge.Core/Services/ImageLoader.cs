using SpriteAtlasForge.Core.Models;

namespace SpriteAtlasForge.Core.Services;

public class ImageLoader
{
    private static readonly string[] SupportedExtensions = { ".png", ".jpg", ".jpeg", ".webp" };

    public bool IsSupportedFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return SupportedExtensions.Contains(extension);
    }

    public async Task<SourceImage> LoadImageInfoAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Image file not found: {filePath}");

        if (!IsSupportedFormat(filePath))
            throw new NotSupportedException($"Unsupported image format: {Path.GetExtension(filePath)}");

        try
        {
            var fileInfo = new FileInfo(filePath);
            var format = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant();

            // We'll get actual dimensions when rendering
            // For now, return basic info
            return new SourceImage(
                filePath,
                0, // Width will be set when image is loaded for rendering
                0, // Height will be set when image is loaded for rendering
                format,
                fileInfo.Length
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load image info: {ex.Message}", ex);
        }
    }

    public string[] GetSupportedExtensions() => SupportedExtensions.ToArray();
}
