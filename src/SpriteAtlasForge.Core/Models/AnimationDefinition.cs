using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteAtlasForge.Core.Models;

public partial class AnimationDefinition : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private AnimationType _type = AnimationType.Idle;

    [ObservableProperty]
    private double _fps = 8.0;

    [ObservableProperty]
    private bool _loop = true;

    [ObservableProperty]
    private List<SpriteFrame> _frames = new();

    public AnimationDefinition() { }

    public AnimationDefinition(string name, AnimationType type, double fps = 8.0, bool loop = true)
    {
        Name = name;
        Type = type;
        Fps = fps;
        Loop = loop;
    }

    public bool HasFrames() => Frames.Count > 0;

    public double GetTotalDuration()
    {
        return Frames.Sum(f => f.Duration);
    }
}
