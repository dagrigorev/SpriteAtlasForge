using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteAtlasForge.Core.Models;

public partial class SourceImage : ObservableObject
{
    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private int _width;

    [ObservableProperty]
    private int _height;

    [ObservableProperty]
    private string _format = string.Empty;

    [ObservableProperty]
    private long _fileSizeBytes;

    public SourceImage() { }

    public SourceImage(string filePath, int width, int height, string format, long fileSizeBytes)
    {
        FilePath = filePath;
        Width = width;
        Height = height;
        Format = format;
        FileSizeBytes = fileSizeBytes;
    }

    public string GetFileName() => Path.GetFileName(FilePath);

    public string GetFileSizeFormatted()
    {
        const int KB = 1024;
        const int MB = KB * 1024;

        if (FileSizeBytes >= MB)
            return $"{FileSizeBytes / (double)MB:F2} MB";
        if (FileSizeBytes >= KB)
            return $"{FileSizeBytes / (double)KB:F2} KB";
        
        return $"{FileSizeBytes} bytes";
    }
}
