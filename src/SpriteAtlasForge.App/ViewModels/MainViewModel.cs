using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpriteAtlasForge.Core.Export;
using SpriteAtlasForge.Core.Models;
using SpriteAtlasForge.Core.Services;
using SpriteAtlasForge.Core.Validation;
using SpriteAtlasForge.App.Models;
using System.Text.Json;

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

    // Logging system
    [ObservableProperty]
    private ObservableCollection<LogEntry> _logEntries = new();

    [ObservableProperty]
    private bool _showDebugLogs = true;

    [ObservableProperty]
    private bool _showInfoLogs = true;

    [ObservableProperty]
    private bool _showWarningLogs = true;

    [ObservableProperty]
    private bool _showErrorLogs = true;

    [ObservableProperty]
    private bool _autoScrollLogs = true;

    private const int MaxLogEntries = 1000; // Limit log entries to prevent memory issues

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
        
        // Initial log entry
        LogInfo("SpriteAtlasForge initialized", "System");
        LogDebug($".NET Version: {Environment.Version}", "System");
    }

    /// <summary>
    /// Rebuilds the TreeView structure from current project groups
    /// Ensures UI reflects actual data state
    /// </summary>
    private void RebuildGroupTree()
    {
        System.Diagnostics.Debug.WriteLine($"[RebuildGroupTree] Rebuilding with {CurrentProject.Groups.Count} groups");
        
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
                
                System.Diagnostics.Debug.WriteLine($"[RebuildGroupTree] Added: {group.Name} ({group.Frames.Count} frames)");
            }
            
            GroupTree.Add(categoryNode);
        }
        
        System.Diagnostics.Debug.WriteLine($"[RebuildGroupTree] Complete");
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
    /// <summary>
    /// Saves the current project to .safproj file
    /// </summary>
    /// <param name="filePath">Full path where to save</param>
    private async Task SaveProject(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        try
        {
            System.Diagnostics.Debug.WriteLine($"[SaveProject] Saving to: {filePath}");
            
            await _projectSerializer.SaveProjectAsync(CurrentProject, filePath);
            
            HasUnsavedChanges = false;
            CurrentProject.FilePath = filePath;
            StatusText = $"✓ Project saved: {Path.GetFileName(filePath)}";
            
            System.Diagnostics.Debug.WriteLine($"[SaveProject] Success");
        }
        catch (Exception ex)
        {
            StatusText = $"❌ Error saving project: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[SaveProject] Error: {ex}");
        }
    }

    /// <summary>
    /// Opens an existing project from .safproj file
    /// Fully reloads and synchronizes all UI elements
    /// </summary>
    /// <param name="filePath">Full path to .safproj file</param>
    [RelayCommand]
    private async Task OpenProject(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        try
        {
            LogInfo($"Loading project: {Path.GetFileName(filePath)}", "OpenProject");
            
            // STEP 1: Load project data from JSON
            var loadedProject = await _projectSerializer.LoadProjectAsync(filePath);
            
            if (loadedProject == null)
            {
                LogError("Failed to load project: Invalid file", "OpenProject");
                StatusText = "❌ Failed to load project: Invalid file";
                return;
            }

            LogSuccess($"Loaded {loadedProject.Groups.Count} groups", "OpenProject");

            // STEP 2: Reload source image
            if (loadedProject.SourceImage != null)
            {
                var imagePath = loadedProject.SourceImage.FilePath;
                
                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (File.Exists(imagePath))
                    {
                        LogInfo($"Loading image: {Path.GetFileName(imagePath)}", "OpenProject");
                        await OpenImageCommand.ExecuteAsync(imagePath);
                        
                        // IMPORTANT: Restore the loaded project 
                        // (OpenImage creates a new project, we want the loaded one)
                        CurrentProject = loadedProject;
                    }
                    else
                    {
                        LogWarning($"Source image not found: {Path.GetFileName(imagePath)}", "OpenProject");
                        StatusText = $"⚠️ Source image not found: {Path.GetFileName(imagePath)}";
                        CurrentProject = loadedProject;
                    }
                }
                else
                {
                    CurrentProject = loadedProject;
                }
            }
            else
            {
                CurrentProject = loadedProject;
            }

            // STEP 3: Clear all selections
            SelectedGroup = null;
            SelectedFrame = null;
            SelectedRegion = null;

            // STEP 4: Rebuild UI Tree
            LogDebug("Rebuilding UI tree", "OpenProject");
            RebuildGroupTree();

            // STEP 5: Subscribe to new groups
            CurrentProject.Groups.CollectionChanged += (s, e) =>
            {
                RebuildGroupTree();
            };

            foreach (var group in CurrentProject.Groups)
            {
                group.PropertyChanged += (s, e) =>
                {
                    RebuildGroupTree();
                };
            }

            // STEP 6: Notify all UI bindings
            OnPropertyChanged(nameof(CurrentProject));
            OnPropertyChanged(nameof(HasSourceImage));
            
            // STEP 7: Mark as clean (no unsaved changes)
            HasUnsavedChanges = false;
            CurrentProject.FilePath = filePath;

            // STEP 8: Update status
            StatusText = $"✓ Project loaded: {Path.GetFileName(filePath)} ({CurrentProject.Groups.Count} groups)";
            LogSuccess($"Project loaded successfully: {Path.GetFileName(filePath)}", "OpenProject");
        }
        catch (FileNotFoundException ex)
        {
            var fileName = Path.GetFileName(ex.FileName ?? filePath);
            LogError($"File not found: {fileName}", "OpenProject", ex);
            StatusText = $"❌ File not found: {fileName}";
        }
        catch (JsonException ex)
        {
            LogError("Invalid project file format", "OpenProject", ex);
            StatusText = $"❌ Invalid project file format";
        }
        catch (Exception ex)
        {
            LogError($"Error loading project: {ex.Message}", "OpenProject", ex);
            StatusText = $"❌ Error loading project: {ex.Message}";
        }
    }

    /// <summary>
    /// Exports the atlas to JSON format
    /// </summary>
    /// <param name="outputPath">Full path where to export</param>
    [RelayCommand]
    private async Task ExportAtlas(string? outputPath)
    {
        if (string.IsNullOrEmpty(outputPath))
            return;

        try
        {
            System.Diagnostics.Debug.WriteLine($"[ExportAtlas] Starting export to: {outputPath}");
            
            // Validate first
            var validationResult = _validator.Validate(CurrentProject);
            
            ValidationMessages.Clear();
            foreach (var msg in validationResult.Messages)
            {
                ValidationMessages.Add(msg);
            }

            if (!validationResult.IsValid)
            {
                StatusText = $"❌ Validation failed: {validationResult.ErrorCount} error(s)";
                System.Diagnostics.Debug.WriteLine($"[ExportAtlas] Validation failed");
                return;
            }

            // Count total frames
            var totalFrames = CurrentProject.Groups.Sum(g => g.Frames.Count);
            System.Diagnostics.Debug.WriteLine($"[ExportAtlas] Exporting {totalFrames} frames from {CurrentProject.Groups.Count} groups");

            await _exporter.ExportToJsonAsync(CurrentProject, outputPath);
            
            StatusText = $"✓ Atlas exported: {Path.GetFileName(outputPath)}";
            System.Diagnostics.Debug.WriteLine($"[ExportAtlas] Success!");
        }
        catch (Exception ex)
        {
            StatusText = $"❌ Error exporting atlas: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[ExportAtlas] Error: {ex}");
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

    /// <summary>
    /// Validates the current project and displays validation messages
    /// </summary>
    [RelayCommand]
    private void ValidateProject()
    {
        LogInfo("Starting project validation...", "Validation");
        
        var result = _validator.Validate(CurrentProject);
        
        ValidationMessages.Clear();
        foreach (var msg in result.Messages)
        {
            ValidationMessages.Add(msg);
            
            // Also log to log panel
            switch (msg.Severity)
            {
                case ValidationSeverity.Error:
                    LogError($"{msg.Message} ({msg.Source})", "Validation");
                    break;
                case ValidationSeverity.Warning:
                    LogWarning($"{msg.Message} ({msg.Source})", "Validation");
                    break;
                case ValidationSeverity.Info:
                    LogInfo($"{msg.Message} ({msg.Source})", "Validation");
                    break;
            }
        }

        if (result.IsValid)
        {
            StatusText = "✓ Validation passed - no issues found";
            LogSuccess($"Validation passed - no issues found", "Validation");
        }
        else
        {
            StatusText = $"⚠️ Validation: {result.ErrorCount} error(s), {result.WarningCount} warning(s)";
            LogWarning($"Validation found {result.ErrorCount} error(s), {result.WarningCount} warning(s)", "Validation");
        }
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

    #region Logging System

    /// <summary>
    /// Adds a debug log entry
    /// </summary>
    public void LogDebug(string message, string category = "")
    {
        AddLog(LogLevel.Debug, message, category);
        System.Diagnostics.Debug.WriteLine($"[{category}] {message}");
    }

    /// <summary>
    /// Adds an info log entry
    /// </summary>
    public void LogInfo(string message, string category = "")
    {
        AddLog(LogLevel.Info, message, category);
        System.Diagnostics.Debug.WriteLine($"[{category}] {message}");
    }

    /// <summary>
    /// Adds a success log entry
    /// </summary>
    public void LogSuccess(string message, string category = "")
    {
        AddLog(LogLevel.Success, message, category);
        System.Diagnostics.Debug.WriteLine($"[{category}] ✓ {message}");
    }

    /// <summary>
    /// Adds a warning log entry
    /// </summary>
    public void LogWarning(string message, string category = "")
    {
        AddLog(LogLevel.Warning, message, category);
        System.Diagnostics.Debug.WriteLine($"[{category}] ⚠️ {message}");
    }

    /// <summary>
    /// Adds an error log entry
    /// </summary>
    public void LogError(string message, string category = "", Exception? exception = null)
    {
        var fullMessage = exception != null 
            ? $"{message}: {exception.Message}" 
            : message;
        
        AddLog(LogLevel.Error, fullMessage, category);
        System.Diagnostics.Debug.WriteLine($"[{category}] ❌ {fullMessage}");
        
        if (exception != null)
        {
            System.Diagnostics.Debug.WriteLine($"[{category}] Exception: {exception}");
        }
    }

    /// <summary>
    /// Adds a log entry to the collection
    /// </summary>
    private void AddLog(LogLevel level, string message, string category)
    {
        var entry = new LogEntry(level, message, category);
        
        // Add to collection (on UI thread)
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            LogEntries.Add(entry);
            
            // Trim old entries if exceeding max
            while (LogEntries.Count > MaxLogEntries)
            {
                LogEntries.RemoveAt(0);
            }
        });
    }

    /// <summary>
    /// Clears all log entries
    /// </summary>
    [RelayCommand]
    private void ClearLogs()
    {
        LogEntries.Clear();
        LogInfo("Logs cleared", "System");
    }

    /// <summary>
    /// Gets filtered log entries based on level toggles
    /// </summary>
    public IEnumerable<LogEntry> FilteredLogs
    {
        get
        {
            return LogEntries.Where(log =>
                (log.Level == LogLevel.Debug && ShowDebugLogs) ||
                (log.Level == LogLevel.Info && ShowInfoLogs) ||
                (log.Level == LogLevel.Success && ShowInfoLogs) ||
                (log.Level == LogLevel.Warning && ShowWarningLogs) ||
                (log.Level == LogLevel.Error && ShowErrorLogs)
            );
        }
    }

    #endregion
}
