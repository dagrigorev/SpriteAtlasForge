using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteAtlasForge.Core.Models;

public partial class ParallaxLayerDefinition : ObservableObject
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
    private double _scrollFactor = 1.0;

    [ObservableProperty]
    private bool _repeatX = false;

    [ObservableProperty]
    private bool _repeatY = false;

    [ObservableProperty]
    private int _renderOrder = 0;

    [ObservableProperty]
    private double _opacity = 1.0;

    [ObservableProperty]
    private string _tint = "#FFFFFF";

    public ParallaxLayerDefinition() { }

    public ParallaxLayerDefinition(string name, int x, int y, int width, int height)
    {
        Name = name;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
