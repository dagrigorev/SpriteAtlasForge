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
    private readonly AutoDetectionService _autoDetection;
    private readonly ImageDataExtractor _imageExtractor;
    private readonly BackgroundRemovalService _backgroundRemoval;

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

    [ObservableProperty]
    private ObservableCollection<GroupTreeNode> _groupTree = new();

    [ObservableProperty]
    private Avalonia.Rect? _selectedRegion;

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
        _autoDetection = new AutoDetectionService();
        _imageExtractor = new ImageDataExtractor();
        _backgroundRemoval = new BackgroundRemovalService();

        CurrentProject.PropertyChanged += (s, e) => MarkProjectAsModified();
        CurrentProject.Groups.CollectionChanged += (s, e) => RebuildGroupTree();
        
        RebuildGroupTree();
    }

    private void RebuildGroupTree()
    {
        GroupTree.Clear();

        // Group by type
        var groupsByType = CurrentProject.Groups
            .GroupBy(g => g.Type)
            .OrderBy(g => g.Key);

        foreach (var typeGroup in groupsByType)
        {
            var categoryNode = GroupTreeNode.CreateCategory(typeGroup.Key);
            
            foreach (var group in typeGroup.OrderBy(g => g.Name))
            {
                var groupNode = GroupTreeNode.CreateGroupNode(group);
                categoryNode.Children.Add(groupNode);
            }
            
            GroupTree.Add(categoryNode);
        }
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

    [RelayCommand]
    private async Task AutoDetect()
    {
        if (CurrentProject.SourceImage == null)
        {
            StatusText = "No image loaded - open an image first";
            return;
        }

        try
        {
            string regionInfo = SelectedRegion.HasValue 
                ? $" (Region: {(int)SelectedRegion.Value.Width}x{(int)SelectedRegion.Value.Height})"
                : "";
            StatusText = $"Analyzing image with computer vision{regionInfo}... Please wait...";
            
            // Extract pixel data
            var (imageData, width, height) = _imageExtractor.ExtractPixelData(CurrentProject.SourceImage.FilePath);
            
            // Convert Avalonia.Rect? to OpenCV-compatible ROI
            OpenCvSharp.Rect? roi = null;
            if (SelectedRegion.HasValue)
            {
                roi = new OpenCvSharp.Rect(
                    (int)SelectedRegion.Value.X,
                    (int)SelectedRegion.Value.Y,
                    (int)SelectedRegion.Value.Width,
                    (int)SelectedRegion.Value.Height);
            }
            
            // Run computer vision detection
            var result = await Task.Run(() => _autoDetection.DetectSprites(imageData, width, height, roi));
            
            if (result.Clusters.Count == 0)
            {
                StatusText = "No sprite patterns detected - try adjusting your sprite sheet";
                return;
            }
            
            // Clear existing groups
            CurrentProject.Groups.Clear();
            
            // Convert detected clusters to GridGroups (with auto background detection)
            foreach (var cluster in result.Clusters)
            {
                var group = _autoDetection.CreateGridGroup(cluster, imageData, width, height);
                CurrentProject.Groups.Add(group);
            }
            
            // Select first group
            SelectedGroup = CurrentProject.Groups.FirstOrDefault();
            
            MarkProjectAsModified();
            StatusText = $"✓ {result.Summary}";
        }
        catch (Exception ex)
        {
            StatusText = $"Auto-detect error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AutoTrimSelectedGroup()
    {
        if (SelectedGroup == null || CurrentProject.SourceImage == null)
        {
            StatusText = "Select a group first";
            return;
        }

        try
        {
            StatusText = "Applying auto-trim...";

            var (imageData, width, height) = _imageExtractor.ExtractPixelData(CurrentProject.SourceImage.FilePath);
            
            await Task.Run(() => _backgroundRemoval.AutoTrimFrames(
                imageData, width, height, 
                SelectedGroup.Frames, 
                detectBackground: true));

            MarkProjectAsModified();
            StatusText = $"✓ Auto-trim applied to {SelectedGroup.Frames.Count} frames";
        }
        catch (Exception ex)
        {
            StatusText = $"Auto-trim error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ResetTrimSelectedGroup()
    {
        if (SelectedGroup == null)
        {
            StatusText = "Select a group first";
            return;
        }

        foreach (var frame in SelectedGroup.Frames)
        {
            frame.ResetTrim();
        }

        MarkProjectAsModified();
        StatusText = $"✓ Trim reset for {SelectedGroup.Frames.Count} frames";
    }

    [RelayCommand]
    private void ToggleBackgroundRemoval()
    {
        if (SelectedGroup == null)
        {
            StatusText = "Select a group first";
            return;
        }

        bool newState = !SelectedGroup.Frames.FirstOrDefault()?.RemoveBackground ?? true;
        
        foreach (var frame in SelectedGroup.Frames)
        {
            frame.RemoveBackground = newState;
        }

        MarkProjectAsModified();
        StatusText = newState 
            ? $"✓ Background removal enabled" 
            : $"✓ Background removal disabled";
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
