using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SpriteAtlasForge.App.ViewModels;
using SpriteAtlasForge.Core.Models;

namespace SpriteAtlasForge.App.Views;

public partial class MainWindow : Window
{
    private ImageCanvas? _imageCanvas;
    private FramePreview? _framePreview;

    public MainWindow()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
        
        // Create and add canvas
        _imageCanvas = new ImageCanvas();
        var canvasContainer = this.FindControl<Panel>("CanvasContainer");
        if (canvasContainer != null)
        {
            canvasContainer.Children.Add(_imageCanvas);
        }

        // Create and add frame preview
        _framePreview = new FramePreview();
        var previewContainer = this.FindControl<Panel>("PreviewContainer");
        if (previewContainer != null)
        {
            previewContainer.Children.Add(_framePreview);
        }

        // Subscribe to DataContext changes
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (ViewModel != null && _imageCanvas != null)
        {
            _imageCanvas.SetViewModel(ViewModel);
            
            // Subscribe to property changes to redraw canvas
            ViewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(ViewModel.ShowGrid))
                {
                    _imageCanvas?.InvalidateVisual();
                }
                else if (args.PropertyName == nameof(ViewModel.SelectedGroup))
                {
                    _imageCanvas?.InvalidateVisual();
                    UpdateFramePreview();
                }
                else if (args.PropertyName == nameof(ViewModel.CurrentProject))
                {
                    UpdateFramePreview();
                }
            };

            // Subscribe to Groups collection changes
            ViewModel.CurrentProject.Groups.CollectionChanged += (s, args) =>
            {
                _imageCanvas?.InvalidateVisual();
                
                // Subscribe to new groups
                if (args.NewItems != null)
                {
                    foreach (GridGroup group in args.NewItems)
                    {
                        SubscribeToGroup(group);
                    }
                }
            };
            
            // Subscribe to existing groups
            foreach (var group in ViewModel.CurrentProject.Groups)
            {
                SubscribeToGroup(group);
            }
        }
    }

    private void SubscribeToGroup(GridGroup group)
    {
        // Subscribe to frames collection changes
        group.Frames.CollectionChanged += (s, e) =>
        {
            if (group == ViewModel?.SelectedGroup)
            {
                UpdateFramePreview();
            }
        };
        
        // Subscribe to property changes
        group.PropertyChanged += (s, e) =>
        {
            if (group == ViewModel?.SelectedGroup && e.PropertyName == nameof(group.PreviewFps))
            {
                _framePreview?.SetFPS(group.PreviewFps);
            }
        };
    }

    private void UpdateFramePreview()
    {
        if (_framePreview == null)
            return;

        if (ViewModel?.SelectedGroup == null)
        {
            _framePreview.SetFrames(Enumerable.Empty<SpriteFrame>());
            return;
        }

        var sourceImagePath = ViewModel.CurrentProject.SourceImage?.FilePath;
        if (!string.IsNullOrEmpty(sourceImagePath) && System.IO.File.Exists(sourceImagePath))
        {
            _framePreview.LoadSourceImage(sourceImagePath);
        }

        // Show frames if they exist, otherwise show empty preview
        _framePreview.SetFrames(ViewModel.SelectedGroup.Frames);
        _framePreview.SetFPS(ViewModel.SelectedGroup.PreviewFps);
        _framePreview.Stop(); // Reset to first frame
    }

    private void OnPlayPreview(object? sender, RoutedEventArgs e)
    {
        _framePreview?.Play();
    }

    private void OnPausePreview(object? sender, RoutedEventArgs e)
    {
        _framePreview?.Pause();
    }

    private void OnStopPreview(object? sender, RoutedEventArgs e)
    {
        _framePreview?.Stop();
    }

    private void OnPreviewFpsChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_framePreview != null && e.NewValue.HasValue)
        {
            _framePreview.SetFPS((double)e.NewValue.Value);
        }
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void OnOpenImageClick(object? sender, RoutedEventArgs e)
    {
        _ = OpenImageDialog();
    }

    private void OnOpenProjectClick(object? sender, RoutedEventArgs e)
    {
        _ = OpenProjectDialog();
    }

    private void OnSaveProjectClick(object? sender, RoutedEventArgs e)
    {
        _ = SaveProjectDialog();
    }

    private void OnExportAtlasClick(object? sender, RoutedEventArgs e)
    {
        _ = ExportAtlasDialog();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (ViewModel == null)
            return;

        // Ctrl+O - Open Image
        if (e.Key == Key.O && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            _ = OpenImageDialog();
            e.Handled = true;
        }
        // Ctrl+S - Save Project
        else if (e.Key == Key.S && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            _ = SaveProjectDialog();
            e.Handled = true;
        }
        // Ctrl+E - Export Atlas
        else if (e.Key == Key.E && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            _ = ExportAtlasDialog();
            e.Handled = true;
        }
        // G - Toggle Grid
        else if (e.Key == Key.G && !e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ViewModel.ToggleGridCommand.Execute(null);
            e.Handled = true;
        }
        // F - Fit to Screen
        else if (e.Key == Key.F && !e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ViewModel.FitToScreenCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+ + Zoom In
        else if ((e.Key == Key.OemPlus || e.Key == Key.Add) && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ViewModel.ZoomInCommand.Execute(null);
            _imageCanvas?.SetZoom(ViewModel.CanvasZoom);
            e.Handled = true;
        }
        // Ctrl+ - Zoom Out
        else if ((e.Key == Key.OemMinus || e.Key == Key.Subtract) && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ViewModel.ZoomOutCommand.Execute(null);
            _imageCanvas?.SetZoom(ViewModel.CanvasZoom);
            e.Handled = true;
        }
        // Delete - Remove selected group/frame
        else if (e.Key == Key.Delete)
        {
            if (ViewModel.SelectedGroup != null)
            {
                ViewModel.RemoveGroupCommand.Execute(ViewModel.SelectedGroup);
            }
            e.Handled = true;
        }
    }

    private async Task OpenImageDialog()
    {
        if (ViewModel == null)
            return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Image Files")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.webp" }
                }
            }
        });

        if (files.Count > 0)
        {
            var filePath = files[0].Path.LocalPath;
            
            // Load image in canvas
            _imageCanvas?.LoadImage(filePath);
            
            // Load image in preview
            _framePreview?.LoadSourceImage(filePath);
            
            // Update ViewModel
            await ViewModel.OpenImageCommand.ExecuteAsync(filePath);
        }
    }

    private async Task OpenProjectDialog()
    {
        if (ViewModel == null)
            return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Project",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("SpriteAtlasForge Project")
                {
                    Patterns = new[] { "*.safproj" }
                }
            }
        });

        if (files.Count > 0)
        {
            var filePath = files[0].Path.LocalPath;
            await ViewModel.OpenProjectCommand.ExecuteAsync(filePath);
        }
    }

    private async Task SaveProjectDialog()
    {
        if (ViewModel == null)
            return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Project",
            DefaultExtension = "safproj",
            SuggestedFileName = ViewModel.CurrentProject.ProjectName,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("SpriteAtlasForge Project")
                {
                    Patterns = new[] { "*.safproj" }
                }
            }
        });

        if (file != null)
        {
            var filePath = file.Path.LocalPath;
            await ViewModel.SaveProjectCommand.ExecuteAsync(filePath);
        }
    }

    private async Task ExportAtlasDialog()
    {
        if (ViewModel == null)
            return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Atlas JSON",
            DefaultExtension = "json",
            SuggestedFileName = "atlas.json",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("JSON File")
                {
                    Patterns = new[] { "*.json" }
                }
            }
        });

        if (file != null)
        {
            var filePath = file.Path.LocalPath;
            await ViewModel.ExportAtlasCommand.ExecuteAsync(filePath);
        }
    }
}
