using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpriteAtlasForge.Core.Export;
using SpriteAtlasForge.Core.Models;
using SpriteAtlasForge.Core.Services;
using SpriteAtlasForge.Core.Validation;

namespace SpriteAtlasForge.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ProjectSerializer _projectSerializer;
    private readonly ImageLoader _imageLoader;
    private readonly ProjectValidator _validator;
    private readonly AtlasExporter _exporter;

    [ObservableProperty]
    private AtlasProject _currentProject = new();

    [ObservableProperty]
    private GridGroup? _selectedGroup;

    [ObservableProperty]
    private SpriteFrame? _selectedFrame;

    [ObservableProperty]
    private ObservableCollection<ValidationMessage> _validationMessages = new();

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private double _canvasZoom = 1.0;

    [ObservableProperty]
    private bool _showGrid = true;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    public string WindowTitle => HasUnsavedChanges 
        ? $"SpriteAtlasForge - {CurrentProject.ProjectName}*" 
        : $"SpriteAtlasForge - {CurrentProject.ProjectName}";

    public Array GridGroupTypes => Enum.GetValues(typeof(GridGroupType));
    public Array AnimationTypes => Enum.GetValues(typeof(AnimationType));
    public Array TileKinds => Enum.GetValues(typeof(TileKind));

    public bool HasSourceImage => CurrentProject.SourceImage != null;

    public MainViewModel()
    {
        _projectSerializer = new ProjectSerializer();
        _imageLoader = new ImageLoader();
        _validator = new ProjectValidator();
        _exporter = new AtlasExporter();

        CurrentProject.PropertyChanged += (s, e) => MarkProjectAsModified();
    }

    [RelayCommand]
    private async Task NewProject()
    {
        if (HasUnsavedChanges)
        {
            // In real app, show confirmation dialog
            // For now, just proceed
        }

        CurrentProject = new AtlasProject
        {
            ProjectName = "New Project",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        SelectedGroup = null;
        SelectedFrame = null;
        ValidationMessages.Clear();
        HasUnsavedChanges = false;
        StatusText = "New project created";
    }

    [RelayCommand]
    private async Task OpenImage(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        try
        {
            var sourceImage = await _imageLoader.LoadImageInfoAsync(filePath);
            CurrentProject.SourceImage = sourceImage;
            OnPropertyChanged(nameof(HasSourceImage));
            StatusText = $"Loaded image: {sourceImage.GetFileName()}";
            MarkProjectAsModified();
        }
        catch (Exception ex)
        {
            StatusText = $"Error loading image: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveProject(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        try
        {
            await _projectSerializer.SaveProjectAsync(CurrentProject, filePath);
            HasUnsavedChanges = false;
            StatusText = $"Project saved: {Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            StatusText = $"Error saving project: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task OpenProject(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        try
        {
            CurrentProject = await _projectSerializer.LoadProjectAsync(filePath);
            SelectedGroup = null;
            SelectedFrame = null;
            HasUnsavedChanges = false;
            StatusText = $"Project loaded: {Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            StatusText = $"Error loading project: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportAtlas(string? outputPath)
    {
        if (string.IsNullOrEmpty(outputPath))
            return;

        try
        {
            // Validate first
            var validationResult = _validator.Validate(CurrentProject);
            
            ValidationMessages.Clear();
            foreach (var msg in validationResult.Messages)
            {
                ValidationMessages.Add(msg);
            }

            if (!validationResult.IsValid)
            {
                StatusText = $"Validation failed: {validationResult.ErrorCount} error(s)";
                return;
            }

            await _exporter.ExportToJsonAsync(CurrentProject, outputPath);
            StatusText = $"Atlas exported: {Path.GetFileName(outputPath)}";
        }
        catch (Exception ex)
        {
            StatusText = $"Error exporting atlas: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddGroup()
    {
        var newGroup = new GridGroup($"Group {CurrentProject.Groups.Count + 1}", GridGroupType.Character);
        CurrentProject.Groups.Add(newGroup);
        SelectedGroup = newGroup;
        MarkProjectAsModified();
        StatusText = $"Added new group: {newGroup.Name}";
    }

    [RelayCommand]
    private void RemoveGroup(GridGroup? group)
    {
        if (group == null)
            return;

        CurrentProject.Groups.Remove(group);
        
        if (SelectedGroup == group)
            SelectedGroup = null;

        MarkProjectAsModified();
        StatusText = $"Removed group: {group.Name}";
    }

    [RelayCommand]
    private void GenerateFrames()
    {
        if (SelectedGroup == null)
            return;

        SelectedGroup.GenerateFramesFromGrid();
        MarkProjectAsModified();
        StatusText = $"Generated {SelectedGroup.Frames.Count} frames for {SelectedGroup.Name}";
    }

    [RelayCommand]
    private void ToggleGrid()
    {
        ShowGrid = !ShowGrid;
    }

    [RelayCommand]
    private void ZoomIn()
    {
        CanvasZoom = Math.Min(CanvasZoom * 1.2, 10.0);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        CanvasZoom = Math.Max(CanvasZoom / 1.2, 0.1);
    }

    [RelayCommand]
    private void FitToScreen()
    {
        CanvasZoom = 1.0;
        StatusText = "Reset zoom to 100%";
    }

    [RelayCommand]
    private void ValidateProject()
    {
        var result = _validator.Validate(CurrentProject);
        
        ValidationMessages.Clear();
        foreach (var msg in result.Messages)
        {
            ValidationMessages.Add(msg);
        }

        StatusText = result.IsValid 
            ? "Validation passed" 
            : $"Validation: {result.ErrorCount} error(s), {result.WarningCount} warning(s)";
    }

    private void MarkProjectAsModified()
    {
        CurrentProject.MarkAsModified();
        HasUnsavedChanges = true;
        OnPropertyChanged(nameof(WindowTitle));
    }

    partial void OnSelectedGroupChanged(GridGroup? value)
    {
        SelectedFrame = null;
        StatusText = value != null ? $"Selected: {value.Name}" : "No group selected";
    }

    partial void OnCanvasZoomChanged(double value)
    {
        StatusText = $"Zoom: {value:P0}";
    }
}
