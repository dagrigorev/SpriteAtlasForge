using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteAtlasForge.Core.Models;

public partial class SpriteFrame : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _x;

    [ObservableProperty]
    private int _y;

    [ObservableProperty]
    private int _width;

    [ObservableProperty]
    private int _height;

    [ObservableProperty]
    private bool _enabled = true;

    [ObservableProperty]
    private string _animationName = string.Empty;

    [ObservableProperty]
    private PivotDefinition? _pivot;

    [ObservableProperty]
    private List<HitboxDefinition> _hitboxes = new();

    [ObservableProperty]
    private List<string> _tags = new();

    [ObservableProperty]
    private TileKind? _tileKind;

    [ObservableProperty]
    private double _duration = 0.125; // seconds per frame

    [ObservableProperty]
    private string? _backgroundColor; // Hex color for background removal (e.g., "#FF00FF")

    [ObservableProperty]
    private bool _removeBackground; // Enable background removal for this frame

    [ObservableProperty]
    private int _trimLeft; // Pixels to trim from left

    [ObservableProperty]
    private int _trimRight; // Pixels to trim from right

    [ObservableProperty]
    private int _trimTop; // Pixels to trim from top

    [ObservableProperty]
    private int _trimBottom; // Pixels to trim from bottom

    public SpriteFrame() { }

    public SpriteFrame(string name, int x, int y, int width, int height)
    {
        Name = name;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool IsWithinBounds(int imageWidth, int imageHeight)
    {
        return X >= 0 && Y >= 0 && 
               X + Width <= imageWidth && 
               Y + Height <= imageHeight;
    }

    public bool IsEmpty()
    {
        return Width <= 0 || Height <= 0;
    }

    public bool IntersectsWith(SpriteFrame other)
    {
        return !(X + Width <= other.X || 
                 other.X + other.Width <= X ||
                 Y + Height <= other.Y || 
                 other.Y + other.Height <= Y);
    }

    /// <summary>
    /// Get effective bounds after trimming
    /// </summary>
    public (int x, int y, int width, int height) GetTrimmedBounds()
    {
        return (
            X + TrimLeft,
            Y + TrimTop,
            Width - TrimLeft - TrimRight,
            Height - TrimTop - TrimBottom
        );
    }

    /// <summary>
    /// Apply auto-trim based on transparent pixels
    /// </summary>
    public void ApplyAutoTrim(int left, int right, int top, int bottom)
    {
        TrimLeft = left;
        TrimRight = right;
        TrimTop = top;
        TrimBottom = bottom;
    }

    /// <summary>
    /// Reset trim values
    /// </summary>
    public void ResetTrim()
    {
        TrimLeft = 0;
        TrimRight = 0;
        TrimTop = 0;
        TrimBottom = 0;
    }
}
