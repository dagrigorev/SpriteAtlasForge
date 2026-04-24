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

    // Selection mode
    private bool _isSelectionMode;
    private Point? _selectionStart;
    private Point? _selectionEnd;
    private Rect? _selectedRegion;

    // Frame editing
    private SpriteFrame? _selectedFrame;
    private bool _isDraggingFrame;
    private bool _isResizingFrame;
    private Point _frameDragStart;
    private ResizeHandle _activeResizeHandle = ResizeHandle.None;
    
    // Temporary values during resize (to avoid PropertyChanged spam)
    private int _tempFrameX;
    private int _tempFrameY;
    private int _tempFrameWidth;
    private int _tempFrameHeight;
    
    // Initial values at resize start (for delta calculation)
    private int _initialFrameX;
    private int _initialFrameY;
    private int _initialFrameWidth;
    private int _initialFrameHeight;

    private enum ResizeHandle
    {
        None,
        TopLeft, Top, TopRight,
        Left, Right,
        BottomLeft, Bottom, BottomRight
    }

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

    public void EnableSelectionMode()
    {
        _isSelectionMode = true;
        Cursor = new Cursor(StandardCursorType.Cross);
    }

    public void DisableSelectionMode()
    {
        _isSelectionMode = false;
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    public void ClearSelection()
    {
        _selectedRegion = null;
        _selectionStart = null;
        _selectionEnd = null;
        if (_viewModel != null)
            _viewModel.SelectedRegion = null;
        InvalidateVisual();
    }

    public Rect? GetSelectedRegion()
    {
        return _selectedRegion;
    }

    /// <summary>
    /// Get resize handle at point (if any)
    /// </summary>
    private ResizeHandle GetResizeHandleAtPoint(Point imagePoint, SpriteFrame frame)
    {
        const double handleSize = 8; // pixels in image space

        // Top-left
        if (IsPointNearCorner(imagePoint, new Point(frame.X, frame.Y), handleSize))
            return ResizeHandle.TopLeft;

        // Top-right
        if (IsPointNearCorner(imagePoint, new Point(frame.X + frame.Width, frame.Y), handleSize))
            return ResizeHandle.TopRight;

        // Bottom-left
        if (IsPointNearCorner(imagePoint, new Point(frame.X, frame.Y + frame.Height), handleSize))
            return ResizeHandle.BottomLeft;

        // Bottom-right
        if (IsPointNearCorner(imagePoint, new Point(frame.X + frame.Width, frame.Y + frame.Height), handleSize))
            return ResizeHandle.BottomRight;

        // Top edge
        if (Math.Abs(imagePoint.Y - frame.Y) <= handleSize &&
            imagePoint.X >= frame.X && imagePoint.X <= frame.X + frame.Width)
            return ResizeHandle.Top;

        // Bottom edge
        if (Math.Abs(imagePoint.Y - (frame.Y + frame.Height)) <= handleSize &&
            imagePoint.X >= frame.X && imagePoint.X <= frame.X + frame.Width)
            return ResizeHandle.Bottom;

        // Left edge
        if (Math.Abs(imagePoint.X - frame.X) <= handleSize &&
            imagePoint.Y >= frame.Y && imagePoint.Y <= frame.Y + frame.Height)
            return ResizeHandle.Left;

        // Right edge
        if (Math.Abs(imagePoint.X - (frame.X + frame.Width)) <= handleSize &&
            imagePoint.Y >= frame.Y && imagePoint.Y <= frame.Y + frame.Height)
            return ResizeHandle.Right;

        return ResizeHandle.None;
    }

    private bool IsPointNearCorner(Point point, Point corner, double tolerance)
    {
        return Math.Abs(point.X - corner.X) <= tolerance &&
               Math.Abs(point.Y - corner.Y) <= tolerance;
    }

    private bool IsPointInFrame(Point imagePoint, SpriteFrame frame)
    {
        return imagePoint.X >= frame.X && imagePoint.X <= frame.X + frame.Width &&
               imagePoint.Y >= frame.Y && imagePoint.Y <= frame.Y + frame.Height;
    }

    private StandardCursorType GetCursorForHandle(ResizeHandle handle)
    {
        return handle switch
        {
            ResizeHandle.TopLeft => StandardCursorType.TopLeftCorner,
            ResizeHandle.TopRight => StandardCursorType.TopRightCorner,
            ResizeHandle.BottomLeft => StandardCursorType.BottomLeftCorner,
            ResizeHandle.BottomRight => StandardCursorType.BottomRightCorner,
            ResizeHandle.Top => StandardCursorType.TopSide,
            ResizeHandle.Bottom => StandardCursorType.BottomSide,
            ResizeHandle.Left => StandardCursorType.LeftSide,
            ResizeHandle.Right => StandardCursorType.RightSide,
            _ => StandardCursorType.Arrow
        };
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        var point = e.GetCurrentPoint(this);
        var mousePos = e.GetPosition(this);
        
        // Selection mode
        if (_isSelectionMode && point.Properties.IsLeftButtonPressed && _image != null)
        {
            var imagePos = ScreenToImageCoordinates(mousePos);
            _selectionStart = imagePos;
            _selectionEnd = imagePos;
            InvalidateVisual();
            return;
        }
        
        System.Diagnostics.Debug.WriteLine($"Mouse pressed at: {mousePos}");
        
        // Left click - select group or start dragging grid
        if (point.Properties.IsLeftButtonPressed && _viewModel != null && _image != null)
        {
            var imagePos = ScreenToImageCoordinates(mousePos);
            System.Diagnostics.Debug.WriteLine($"Image coordinates: {imagePos}");
            
            // PRIORITY 1: Check if clicking on selected frame's resize handle
            if (_viewModel.SelectedFrame != null)
            {
                var handle = GetResizeHandleAtPoint(imagePos, _viewModel.SelectedFrame);
                if (handle != ResizeHandle.None)
                {
                    _isResizingFrame = true;
                    _activeResizeHandle = handle;
                    _frameDragStart = imagePos;
                    
                    // Store INITIAL values (for delta calculation)
                    _initialFrameX = _viewModel.SelectedFrame.X;
                    _initialFrameY = _viewModel.SelectedFrame.Y;
                    _initialFrameWidth = _viewModel.SelectedFrame.Width;
                    _initialFrameHeight = _viewModel.SelectedFrame.Height;
                    
                    // Initialize temp values
                    _tempFrameX = _initialFrameX;
                    _tempFrameY = _initialFrameY;
                    _tempFrameWidth = _initialFrameWidth;
                    _tempFrameHeight = _initialFrameHeight;
                    
                    Cursor = new Cursor(GetCursorForHandle(handle));
                    e.Handled = true;
                    return;
                }

                // Check if clicking inside selected frame (to drag it)
                if (IsPointInFrame(imagePos, _viewModel.SelectedFrame))
                {
                    _isDraggingFrame = true;
                    _frameDragStart = new Point(
                        imagePos.X - _viewModel.SelectedFrame.X,
                        imagePos.Y - _viewModel.SelectedFrame.Y);
                    Cursor = new Cursor(StandardCursorType.Hand);
                    e.Handled = true;
                    return;
                }
            }

            // PRIORITY 2: Check if clicking on any frame in selected group
            if (_viewModel.SelectedGroup != null)
            {
                SpriteFrame? clickedFrame = null;
                foreach (var frame in _viewModel.SelectedGroup.Frames)
                {
                    if (IsPointInFrame(imagePos, frame))
                    {
                        clickedFrame = frame;
                        break;
                    }
                }

                if (clickedFrame != null)
                {
                    _viewModel.SelectedFrame = clickedFrame;
                    InvalidateVisual();
                    e.Handled = true;
                    return;
                }
            }

            // PRIORITY 3: Try to find a group to select
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

        // Handle selection drawing
        if (_isSelectionMode && _selectionStart.HasValue && _image != null)
        {
            var imagePos = ScreenToImageCoordinates(mousePos);
            _selectionEnd = imagePos;
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        // Handle frame resizing
        if (_isResizingFrame && _viewModel?.SelectedFrame != null && _image != null)
        {
            var imagePos = ScreenToImageCoordinates(mousePos);
            var deltaX = (int)(imagePos.X - _frameDragStart.X);
            var deltaY = (int)(imagePos.Y - _frameDragStart.Y);

            // Apply delta to INITIAL values (not current temp values!)
            int newX = _initialFrameX;
            int newY = _initialFrameY;
            int newWidth = _initialFrameWidth;
            int newHeight = _initialFrameHeight;

            switch (_activeResizeHandle)
            {
                case ResizeHandle.TopLeft:
                    newX = _initialFrameX + deltaX;
                    newY = _initialFrameY + deltaY;
                    newWidth = _initialFrameWidth - deltaX;
                    newHeight = _initialFrameHeight - deltaY;
                    break;
                case ResizeHandle.TopRight:
                    newY = _initialFrameY + deltaY;
                    newWidth = _initialFrameWidth + deltaX;
                    newHeight = _initialFrameHeight - deltaY;
                    break;
                case ResizeHandle.BottomLeft:
                    newX = _initialFrameX + deltaX;
                    newWidth = _initialFrameWidth - deltaX;
                    newHeight = _initialFrameHeight + deltaY;
                    break;
                case ResizeHandle.BottomRight:
                    newWidth = _initialFrameWidth + deltaX;
                    newHeight = _initialFrameHeight + deltaY;
                    break;
                case ResizeHandle.Top:
                    newY = _initialFrameY + deltaY;
                    newHeight = _initialFrameHeight - deltaY;
                    break;
                case ResizeHandle.Bottom:
                    newHeight = _initialFrameHeight + deltaY;
                    break;
                case ResizeHandle.Left:
                    newX = _initialFrameX + deltaX;
                    newWidth = _initialFrameWidth - deltaX;
                    break;
                case ResizeHandle.Right:
                    newWidth = _initialFrameWidth + deltaX;
                    break;
            }

            // Constrain minimum size
            newWidth = Math.Max(4, newWidth);
            newHeight = Math.Max(4, newHeight);

            // Update temp values
            _tempFrameX = newX;
            _tempFrameY = newY;
            _tempFrameWidth = newWidth;
            _tempFrameHeight = newHeight;

            InvalidateVisual();
            e.Handled = true;
            return;
        }

        // Handle frame dragging
        if (_isDraggingFrame && _viewModel?.SelectedFrame != null && _image != null)
        {
            var imagePos = ScreenToImageCoordinates(mousePos);
            var frame = _viewModel.SelectedFrame;
            frame.X = (int)(imagePos.X - _frameDragStart.X);
            frame.Y = (int)(imagePos.Y - _frameDragStart.Y);
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        // Update cursor when hovering over selected frame
        if (_viewModel?.SelectedFrame != null && _image != null && !_isDraggingGrid && !_isPanning)
        {
            var imagePos = ScreenToImageCoordinates(mousePos);
            var handle = GetResizeHandleAtPoint(imagePos, _viewModel.SelectedFrame);
            
            if (handle != ResizeHandle.None)
            {
                Cursor = new Cursor(GetCursorForHandle(handle));
            }
            else if (IsPointInFrame(imagePos, _viewModel.SelectedFrame))
            {
                Cursor = new Cursor(StandardCursorType.Hand);
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Arrow);
            }
        }

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
        
        // Finalize selection
        if (_isSelectionMode && _selectionStart.HasValue && _selectionEnd.HasValue)
        {
            var x1 = Math.Min(_selectionStart.Value.X, _selectionEnd.Value.X);
            var y1 = Math.Min(_selectionStart.Value.Y, _selectionEnd.Value.Y);
            var x2 = Math.Max(_selectionStart.Value.X, _selectionEnd.Value.X);
            var y2 = Math.Max(_selectionStart.Value.Y, _selectionEnd.Value.Y);
            
            var width = x2 - x1;
            var height = y2 - y1;
            
            if (width > 10 && height > 10) // Minimum selection size
            {
                _selectedRegion = new Rect(x1, y1, width, height);
                if (_viewModel != null)
                    _viewModel.SelectedRegion = _selectedRegion;
            }
            
            _selectionStart = null;
            _selectionEnd = null;
            _isSelectionMode = false;
            Cursor = new Cursor(StandardCursorType.Arrow);
            InvalidateVisual();
            e.Handled = true;
            return;
        }
        
        // End frame resizing
        if (_isResizingFrame)
        {
            // Apply final values to frame (single PropertyChanged batch)
            if (_viewModel?.SelectedFrame != null)
            {
                _viewModel.SelectedFrame.X = _tempFrameX;
                _viewModel.SelectedFrame.Y = _tempFrameY;
                _viewModel.SelectedFrame.Width = _tempFrameWidth;
                _viewModel.SelectedFrame.Height = _tempFrameHeight;
            }
            
            _isResizingFrame = false;
            _activeResizeHandle = ResizeHandle.None;
            Cursor = new Cursor(StandardCursorType.Arrow);
            e.Handled = true;
            return;
        }

        // End frame dragging
        if (_isDraggingFrame)
        {
            _isDraggingFrame = false;
            Cursor = new Cursor(StandardCursorType.Arrow);
            e.Handled = true;
            return;
        }
        
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

        // Draw selection rectangle
        if (_selectionStart.HasValue && _selectionEnd.HasValue)
        {
            var x1 = Math.Min(_selectionStart.Value.X, _selectionEnd.Value.X);
            var y1 = Math.Min(_selectionStart.Value.Y, _selectionEnd.Value.Y);
            var x2 = Math.Max(_selectionStart.Value.X, _selectionEnd.Value.X);
            var y2 = Math.Max(_selectionStart.Value.Y, _selectionEnd.Value.Y);

            var screenX = imageX + x1 * _zoom;
            var screenY = imageY + y1 * _zoom;
            var screenW = (x2 - x1) * _zoom;
            var screenH = (y2 - y1) * _zoom;

            var selectionRect = new Rect(screenX, screenY, screenW, screenH);
            var selectionBrush = new SolidColorBrush(Color.FromArgb(60, 0, 150, 255));
            var selectionPen = new Pen(new SolidColorBrush(Color.FromRgb(0, 150, 255)), 2);

            context.FillRectangle(selectionBrush, selectionRect);
            context.DrawRectangle(null, selectionPen, selectionRect);
        }

        // Draw finalized selection
        if (_selectedRegion.HasValue)
        {
            var screenX = imageX + _selectedRegion.Value.X * _zoom;
            var screenY = imageY + _selectedRegion.Value.Y * _zoom;
            var screenW = _selectedRegion.Value.Width * _zoom;
            var screenH = _selectedRegion.Value.Height * _zoom;

            var selectionRect = new Rect(screenX, screenY, screenW, screenH);
            var selectionBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 0));
            var selectionPen = new Pen(new SolidColorBrush(Color.FromRgb(255, 255, 0)), 2);

            context.FillRectangle(selectionBrush, selectionRect);
            context.DrawRectangle(null, selectionPen, selectionRect);
        }

        // Draw selected frame with resize handles
        if (_viewModel?.SelectedFrame != null)
        {
            var frame = _viewModel.SelectedFrame;
            
            // Use temporary values during resize for smooth performance
            int frameX = _isResizingFrame ? _tempFrameX : frame.X;
            int frameY = _isResizingFrame ? _tempFrameY : frame.Y;
            int frameWidth = _isResizingFrame ? _tempFrameWidth : frame.Width;
            int frameHeight = _isResizingFrame ? _tempFrameHeight : frame.Height;
            
            var screenX = imageX + frameX * _zoom;
            var screenY = imageY + frameY * _zoom;
            var screenW = frameWidth * _zoom;
            var screenH = frameHeight * _zoom;

            var frameRect = new Rect(screenX, screenY, screenW, screenH);
            
            // Highlight border (thick magenta)
            var highlightPen = new Pen(new SolidColorBrush(Color.FromRgb(255, 0, 255)), 3);
            context.DrawRectangle(null, highlightPen, frameRect);

            // Draw resize handles (8 small squares)
            var handleSize = 8.0; // screen pixels
            var handleBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            var handlePen = new Pen(new SolidColorBrush(Color.FromRgb(0, 0, 0)), 1);

            // Top-left
            DrawResizeHandle(context, screenX, screenY, handleSize, handleBrush, handlePen);
            // Top
            DrawResizeHandle(context, screenX + screenW / 2, screenY, handleSize, handleBrush, handlePen);
            // Top-right
            DrawResizeHandle(context, screenX + screenW, screenY, handleSize, handleBrush, handlePen);
            // Left
            DrawResizeHandle(context, screenX, screenY + screenH / 2, handleSize, handleBrush, handlePen);
            // Right
            DrawResizeHandle(context, screenX + screenW, screenY + screenH / 2, handleSize, handleBrush, handlePen);
            // Bottom-left
            DrawResizeHandle(context, screenX, screenY + screenH, handleSize, handleBrush, handlePen);
            // Bottom
            DrawResizeHandle(context, screenX + screenW / 2, screenY + screenH, handleSize, handleBrush, handlePen);
            // Bottom-right
            DrawResizeHandle(context, screenX + screenW, screenY + screenH, handleSize, handleBrush, handlePen);
        }
    }

    private void DrawResizeHandle(DrawingContext context, double x, double y, double size, IBrush brush, IPen pen)
    {
        var handleRect = new Rect(x - size / 2, y - size / 2, size, size);
        context.FillRectangle(brush, handleRect);
        context.DrawRectangle(null, pen, handleRect);
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
