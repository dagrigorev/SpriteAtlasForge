using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using SpriteAtlasForge.Core.Models;

namespace SpriteAtlasForge.Rendering.Canvas;

public class CanvasRenderer
{
    private Bitmap? _sourceImage;
    private SKBitmap? _skBitmap;

    public Bitmap? SourceImage => _sourceImage;
    public int ImageWidth => _skBitmap?.Width ?? 0;
    public int ImageHeight => _skBitmap?.Height ?? 0;

    public async Task<bool> LoadImageAsync(string filePath)
    {
        try
        {
            await using var stream = File.OpenRead(filePath);
            _skBitmap = SKBitmap.Decode(stream);
            
            // Convert to Avalonia Bitmap
            using var image = SKImage.FromBitmap(_skBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            
            var memoryStream = new MemoryStream();
            data.SaveTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            
            _sourceImage = new Bitmap(memoryStream);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void UnloadImage()
    {
        _sourceImage?.Dispose();
        _sourceImage = null;
        _skBitmap?.Dispose();
        _skBitmap = null;
    }

    public void DrawImage(DrawingContext context, Rect bounds, double zoom, Point offset)
    {
        if (_sourceImage == null)
            return;

        var transform = Matrix.CreateTranslation(offset.X, offset.Y) * 
                       Matrix.CreateScale(zoom, zoom);

        using (context.PushTransform(transform))
        {
            var imageRect = new Rect(0, 0, _sourceImage.Size.Width, _sourceImage.Size.Height);
            context.DrawImage(_sourceImage, imageRect);
        }
    }

    public void DrawGrid(DrawingContext context, GridDefinition grid, string color, 
                        double zoom, Point offset, bool showCells = true)
    {
        var transform = Matrix.CreateTranslation(offset.X, offset.Y) * 
                       Matrix.CreateScale(zoom, zoom);

        using (context.PushTransform(transform))
        {
            var pen = new Pen(Brush.Parse(color), 1.0 / zoom);

            if (showCells)
            {
                // Draw grid cells
                for (int row = 0; row < grid.Rows; row++)
                {
                    for (int col = 0; col < grid.Columns; col++)
                    {
                        var (x, y) = grid.GetCellPosition(col, row);
                        var rect = new Rect(x, y, grid.CellWidth, grid.CellHeight);
                        context.DrawRectangle(null, pen, rect);
                    }
                }
            }

            // Draw origin marker
            var originPen = new Pen(Brushes.Yellow, 2.0 / zoom);
            context.DrawLine(originPen, 
                new Point(grid.OriginX - 10, grid.OriginY), 
                new Point(grid.OriginX + 10, grid.OriginY));
            context.DrawLine(originPen, 
                new Point(grid.OriginX, grid.OriginY - 10), 
                new Point(grid.OriginX, grid.OriginY + 10));
        }
    }

    public void DrawFrame(DrawingContext context, SpriteFrame frame, string color, 
                         double zoom, Point offset, bool isSelected = false)
    {
        var transform = Matrix.CreateTranslation(offset.X, offset.Y) * 
                       Matrix.CreateScale(zoom, zoom);

        using (context.PushTransform(transform))
        {
            var thickness = isSelected ? 2.0 / zoom : 1.0 / zoom;
            var pen = new Pen(Brush.Parse(color), thickness);
            
            if (!frame.Enabled)
            {
                pen = new Pen(Brushes.Gray, thickness) { DashStyle = DashStyle.Dash };
            }

            var rect = new Rect(frame.X, frame.Y, frame.Width, frame.Height);
            context.DrawRectangle(null, pen, rect);

            // Draw pivot if exists
            if (frame.Pivot != null)
            {
                var (pivotX, pivotY) = frame.Pivot.ToPixels(frame.Width, frame.Height);
                var pivotWorldX = frame.X + pivotX;
                var pivotWorldY = frame.Y + pivotY;
                
                var pivotPen = new Pen(Brushes.Cyan, 2.0 / zoom);
                context.DrawEllipse(null, pivotPen, 
                    new Point(pivotWorldX, pivotWorldY), 
                    3.0 / zoom, 3.0 / zoom);
            }

            // Draw hitboxes
            foreach (var hitbox in frame.Hitboxes)
            {
                var hitboxBrush = hitbox.Type == "hitbox" ? Brushes.Red : Brushes.Orange;
                var hitboxPen = new Pen(hitboxBrush, 1.0 / zoom) { DashStyle = DashStyle.Dot };
                var hitboxRect = new Rect(
                    frame.X + hitbox.X, 
                    frame.Y + hitbox.Y, 
                    hitbox.Width, 
                    hitbox.Height);
                context.DrawRectangle(null, hitboxPen, hitboxRect);
            }
        }
    }

    public void DrawSelectionBox(DrawingContext context, Rect box, double zoom, Point offset)
    {
        var transform = Matrix.CreateTranslation(offset.X, offset.Y) * 
                       Matrix.CreateScale(zoom, zoom);

        using (context.PushTransform(transform))
        {
            var pen = new Pen(Brushes.White, 1.0 / zoom) { DashStyle = DashStyle.Dash };
            var brush = new SolidColorBrush(Colors.White, 0.1);
            context.DrawRectangle(brush, pen, box);
        }
    }
}
