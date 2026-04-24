using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteAtlasForge.Core.Models;

public partial class AtlasProject : ObservableObject
{
    [ObservableProperty]
    private string _version = "1.0";

    [ObservableProperty]
    private string _projectName = "Untitled Project";

    [ObservableProperty]
    private SourceImage? _sourceImage;

    [ObservableProperty]
    private ObservableCollection<GridGroup> _groups = new();

    [ObservableProperty]
    private ExportSettings _exportSettings = new();

    [ObservableProperty]
    private DateTime _createdAt = DateTime.UtcNow;

    [ObservableProperty]
    private DateTime _modifiedAt = DateTime.UtcNow;

    [ObservableProperty]
    [property: JsonIgnore]
    private string? _filePath;

    public AtlasProject() { }

    public void MarkAsModified()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    public bool HasSourceImage() => SourceImage != null;

    public bool HasGroups() => Groups.Count > 0;
}
