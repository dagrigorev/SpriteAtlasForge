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
}
