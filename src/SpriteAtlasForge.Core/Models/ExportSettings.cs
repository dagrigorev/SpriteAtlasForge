using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteAtlasForge.Core.Models;

public partial class ExportSettings : ObservableObject
{
    [ObservableProperty]
    private string _outputDirectory = string.Empty;

    [ObservableProperty]
    private string _atlasFileName = "atlas.json";

    [ObservableProperty]
    private bool _prettyPrint = true;

    [ObservableProperty]
    private bool _includeDisabledFrames = false;

    [ObservableProperty]
    private bool _validateBeforeExport = true;

    [ObservableProperty]
    private bool _generateTextureReference = true;

    [ObservableProperty]
    private string _textureFileName = string.Empty;

    public ExportSettings() { }
}
