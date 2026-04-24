using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteAtlasForge.Core.Models;

public partial class GridGroup : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = "New Group";

    [ObservableProperty]
    private GridGroupType _type = GridGroupType.Character;

    [ObservableProperty]
    private string _borderColor = "#FF00FF";

    [ObservableProperty]
    private GridDefinition _gridDefinition = new();

    [ObservableProperty]
    private ObservableCollection<SpriteFrame> _frames = new();

    [ObservableProperty]
    private ObservableCollection<AnimationDefinition> _animations = new();

    [ObservableProperty]
    private ObservableCollection<ParallaxLayerDefinition> _parallaxLayers = new();

    [ObservableProperty]
    private bool _exportEnabled = true;

    [ObservableProperty]
    private double _previewFps = 8.0;

    [ObservableProperty]
    private bool _previewLoop = true;

    [ObservableProperty]
    private bool _allowOverlap = false;

    [ObservableProperty]
    private double _scalePreview = 1.0;

    public GridGroup() { }

    public GridGroup(string name, GridGroupType type)
    {
        Name = name;
        Type = type;
        BorderColor = GetDefaultColorForType(type);
    }

    private static string GetDefaultColorForType(GridGroupType type)
    {
        return type switch
        {
            GridGroupType.Character => "#00FF00",
            GridGroupType.Enemy => "#FF0000",
            GridGroupType.Boss => "#FF00FF",
            GridGroupType.Tile => "#FFFF00",
            GridGroupType.Parallax => "#00FFFF",
            GridGroupType.Item => "#FFA500",
            GridGroupType.Effect => "#FF69B4",
            GridGroupType.UI => "#FFFFFF",
            GridGroupType.Animal => "#90EE90",
            GridGroupType.Decoration => "#DDA0DD",
            GridGroupType.Projectile => "#FF6347",
            _ => "#888888"
        };
    }

    public void GenerateFramesFromGrid()
    {
        Frames.Clear();

        for (int row = 0; row < GridDefinition.Rows; row++)
        {
            for (int col = 0; col < GridDefinition.Columns; col++)
            {
                var (x, y) = GridDefinition.GetCellPosition(col, row);
                
                var frame = new SpriteFrame
                {
                    Name = $"{Name}_{row:D3}_{col:D3}",
                    X = x,
                    Y = y,
                    Width = GridDefinition.CellWidth,
                    Height = GridDefinition.CellHeight,
                    Enabled = true
                };

                Frames.Add(frame);
            }
        }
    }
}
