using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SpriteAtlasForge.Core.Models;
using SpriteAtlasForge.App.ViewModels;
using System;

namespace SpriteAtlasForge.App.Views;

public class ImageCanvas : Control
{
    private Bitmap? _image;
    private Point _offset = new Point(0, 0);
    private double _zoom = 1.0;
    private Point? _lastPanPoint;
    private bool _isPanning;
    private bool _isDraggingGrid;
    private GridGroup? _draggingGroup;
    private Point _dragStartOffset;
    private MainViewModel? _viewModel;

    public void SetViewModel(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        
        // Subscribe to all group property changes for real-time updates
        foreach (var group in viewModel.CurrentProject.Groups)
        {
            SubscribeToGroupChanges(group);
        }
        
        // Subscribe to new groups being added
        viewModel.CurrentProject.Groups.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (GridGroup group in e.NewItems)
                {
                    SubscribeToGroupChanges(group);
                }
            }
            InvalidateVisual();
        };
    }

    private void SubscribeToGroupChanges(GridGroup group)
    {
        group.PropertyChanged += (s, e) => InvalidateVisual();
        group.GridDefinition.PropertyChanged += (s, e) => InvalidateVisual();
        group.Frames.CollectionChanged += (s, e) => InvalidateVisual();
    }

    public void LoadImage(string filePath)
    {
        try
        {
            _image?.Dispose();
            _image = new Bitmap(filePath);
            InvalidateVisual();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load image: {ex.Message}");
        }
    }

    public void SetZoom(double zoom)
    {
        _zoom = Math.Max(0.1, Math.Min(10.0, zoom));
        InvalidateVisual();
    }

    public void ResetView()
    {
        _zoom = 1.0;
        _offset = new Point(0, 0);
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        var point = e.GetCurrentPoint(this);
        var mousePos = e.GetPosition(this);
        
        System.Diagnostics.Debug.WriteLine($"Mouse pressed at: {mousePos}");
        
        // Left click - select group or start dragging grid
        if (point.Properties.IsLeftButtonPressed && _viewModel != null && _image != null)
        {
            var imagePos = ScreenToImageCoordinates(mousePos);
            System.Diagnostics.Debug.WriteLine($"Image coordinates: {imagePos}");
            
            // First, try to find a group to select
            GridGroup? clickedGroup = null;
            foreach (var group in _viewModel.CurrentProject.Groups)
            {
                if (IsPointInGrid(imagePos, group))
                {
                    clickedGroup = group;
                    break;
                }
            }
            
            if (clickedGroup != null)
            {
                System.Diagnostics.Debug.WriteLine($"Clicked on group: {clickedGroup.Name}");
                _viewModel.SelectedGroup = clickedGroup;
                
                // If Ctrl is pressed, start dragging the grid
                if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    _isDraggingGrid = true;
                    _draggingGroup = clickedGroup;
                    _dragStartOffset = new Point(
                        imagePos.X - clickedGroup.GridDefinition.OriginX,
                        imagePos.Y - clickedGroup.GridDefinition.OriginY);
                    Cursor = new Cursor(StandardCursorType.SizeAll);
                    System.Diagnostics.Debug.WriteLine("Started dragging grid");
                }
                
                InvalidateVisual();
                e.Handled = true;
                return;
            }
        }
        
        // Middle button or Shift+Left - pan mode
        if (point.Properties.IsMiddleButtonPressed || 
            (point.Properties.IsLeftButtonPressed && e.KeyModifiers.HasFlag(KeyModifiers.Shift)))
        {
            _isPanning = true;
            _lastPanPoint = mousePos;
            Cursor = new Cursor(StandardCursorType.Hand);
            System.Diagnostics.Debug.WriteLine("Started panning");
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var mousePos = e.GetPosition(this);

        // Handle grid dragging
        if (_isDraggingGrid && _draggingGroup != null && _image != null)
        {
            var imagePos = ScreenToImageCoordinates(mousePos);
            _draggingGroup.GridDefinition.OriginX = (int)(imagePos.X - _dragStartOffset.X);
            _draggingGroup.GridDefinition.OriginY = (int)(imagePos.Y - _dragStartOffset.Y);
            e.Handled = true;
            return;
        }

        // Handle panning
        if (_isPanning && _lastPanPoint.HasValue)
        {
            var currentPoint = mousePos;
            var delta = currentPoint - _lastPanPoint.Value;
            
            _offset = new Point(_offset.X + delta.X, _offset.Y + delta.Y);
            _lastPanPoint = currentPoint;
            
            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        
        if (_isDraggingGrid)
        {
            _isDraggingGrid = false;
            _draggingGroup = null;
            Cursor = Cursor.Default;
            e.Handled = true;
        }
        else if (_isPanning)
        {
            _isPanning = false;
            _lastPanPoint = null;
            Cursor = Cursor.Default;
            e.Handled = true;
        }
    }

    private Point ScreenToImageCoordinates(Point screenPoint)
    {
        if (_image == null)
            return new Point(0, 0);

        var imageWidth = _image.Size.Width * _zoom;
        var imageHeight = _image.Size.Height * _zoom;
        var imageX = (Bounds.Width - imageWidth) / 2 + _offset.X;
        var imageY = (Bounds.Height - imageHeight) / 2 + _offset.Y;

        var relativeX = (screenPoint.X - imageX) / _zoom;
        var relativeY = (screenPoint.Y - imageY) / _zoom;

        return new Point(relativeX, relativeY);
    }

    private bool IsPointInGrid(Point imagePoint, GridGroup group)
    {
        var grid = group.GridDefinition;
        var endX = grid.OriginX + grid.Columns * (grid.CellWidth + grid.Spacing);
        var endY = grid.OriginY + grid.Rows * (grid.CellHeight + grid.Spacing);

        return imagePoint.X >= grid.OriginX && imagePoint.X <= endX &&
               imagePoint.Y >= grid.OriginY && imagePoint.Y <= endY;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            var delta = e.Delta.Y;
            var factor = delta > 0 ? 1.1 : 0.9;
            _zoom *= factor;
            _zoom = Math.Max(0.1, Math.Min(10.0, _zoom));
            
            InvalidateVisual();
            e.Handled = true;
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Draw background
        context.FillRectangle(new SolidColorBrush(Color.FromRgb(30, 30, 30)), Bounds);

        if (_image == null)
            return;

        // Calculate image position (centered)
        var imageWidth = _image.Size.Width * _zoom;
        var imageHeight = _image.Size.Height * _zoom;
        
        var imageX = (Bounds.Width - imageWidth) / 2 + _offset.X;
        var imageY = (Bounds.Height - imageHeight) / 2 + _offset.Y;

        var destRect = new Rect(imageX, imageY, imageWidth, imageHeight);
        var sourceRect = new Rect(0, 0, _image.Size.Width, _image.Size.Height);

        // Draw image
        context.DrawImage(_image, sourceRect, destRect);

        // Draw border around image
        var borderPen = new Pen(Brushes.Gray, 1);
        context.DrawRectangle(null, borderPen, destRect);

        // Draw grids and frames on top of image
        if (_viewModel != null && _viewModel.ShowGrid)
        {
            using (context.PushClip(destRect))
            {
                foreach (var group in _viewModel.CurrentProject.Groups)
                {
                    if (!group.ExportEnabled)
                        continue;

                    DrawGrid(context, group, imageX, imageY);
                    DrawFrames(context, group, imageX, imageY);
                }
            }
        }
    }

    private void DrawGrid(DrawingContext context, GridGroup group, double imageX, double imageY)
    {
        var grid = group.GridDefinition;
        var color = Color.Parse(group.BorderColor);
        var pen = new Pen(new SolidColorBrush(color, 0.5), 1.0 / _zoom);

        // Draw grid cells
        for (int row = 0; row < grid.Rows; row++)
        {
            for (int col = 0; col < grid.Columns; col++)
            {
                var (cellX, cellY) = grid.GetCellPosition(col, row);
                
                var x = imageX + cellX * _zoom;
                var y = imageY + cellY * _zoom;
                var w = grid.CellWidth * _zoom;
                var h = grid.CellHeight * _zoom;

                var rect = new Rect(x, y, w, h);
                context.DrawRectangle(null, pen, rect);
            }
        }

        // Draw origin marker
        var originX = imageX + grid.OriginX * _zoom;
        var originY = imageY + grid.OriginY * _zoom;
        var markerSize = 5 / _zoom;

        var originPen = new Pen(Brushes.Yellow, 2.0);
        context.DrawLine(originPen, 
            new Point(originX - markerSize, originY), 
            new Point(originX + markerSize, originY));
        context.DrawLine(originPen, 
            new Point(originX, originY - markerSize), 
            new Point(originX, originY + markerSize));
    }

    private void DrawFrames(DrawingContext context, GridGroup group, double imageX, double imageY)
    {
        var color = Color.Parse(group.BorderColor);
        var pen = new Pen(new SolidColorBrush(color), 2.0 / _zoom);

        foreach (var frame in group.Frames)
        {
            if (!frame.Enabled)
                continue;

            var x = imageX + frame.X * _zoom;
            var y = imageY + frame.Y * _zoom;
            var w = frame.Width * _zoom;
            var h = frame.Height * _zoom;

            var rect = new Rect(x, y, w, h);
            context.DrawRectangle(null, pen, rect);

            // Draw frame name
            var textBrush = new SolidColorBrush(color);
            var formattedText = new FormattedText(
                frame.Name,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                10,
                textBrush);

            context.DrawText(formattedText, new Point(x + 2, y + 2));
        }
    }
}
