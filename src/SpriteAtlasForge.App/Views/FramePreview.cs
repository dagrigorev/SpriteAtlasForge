using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SpriteAtlasForge.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteAtlasForge.App.Views;

public class FramePreview : Control
{
    private Bitmap? _sourceImage;
    private List<SpriteFrame> _frames = new();
    private int _currentFrameIndex = 0;
    private DispatcherTimer? _animationTimer;
    private double _fps = 8.0;
    private bool _isPlaying = false;

    public void LoadSourceImage(string filePath)
    {
        try
        {
            _sourceImage?.Dispose();
            _sourceImage = new Bitmap(filePath);
            InvalidateVisual();
        }
        catch { }
    }

    public void SetFrames(IEnumerable<SpriteFrame> frames)
    {
        _frames = frames.Where(f => f.Enabled).ToList();
        _currentFrameIndex = 0;
        InvalidateVisual();
    }

    public void SetFPS(double fps)
    {
        _fps = Math.Max(1, Math.Min(60, fps));
        if (_isPlaying && _animationTimer != null)
        {
            _animationTimer.Interval = TimeSpan.FromSeconds(1.0 / _fps);
        }
    }

    public void Play()
    {
        if (_frames.Count == 0)
            return;

        _isPlaying = true;
        
        if (_animationTimer == null)
        {
            _animationTimer = new DispatcherTimer();
            _animationTimer.Tick += OnAnimationTick;
        }
        
        _animationTimer.Interval = TimeSpan.FromSeconds(1.0 / _fps);
        _animationTimer.Start();
    }

    public void Stop()
    {
        _isPlaying = false;
        _animationTimer?.Stop();
        _currentFrameIndex = 0;
        InvalidateVisual();
    }

    public void Pause()
    {
        _isPlaying = false;
        _animationTimer?.Stop();
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        _currentFrameIndex = (_currentFrameIndex + 1) % _frames.Count;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Draw background
        context.FillRectangle(new SolidColorBrush(Color.FromRgb(40, 40, 40)), Bounds);

        if (_frames.Count == 0)
        {
            // Draw placeholder with helpful message
            var message = _sourceImage != null 
                ? "Generate frames to see preview" 
                : "Select a group with frames";
            
            var text = new FormattedText(
                message,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.Gray);
            
            context.DrawText(text, new Point(
                (Bounds.Width - text.Width) / 2,
                (Bounds.Height - text.Height) / 2));
            return;
        }

        if (_sourceImage == null)
        {
            var text = new FormattedText(
                "Image not loaded",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.Red);
            
            context.DrawText(text, new Point(
                (Bounds.Width - text.Width) / 2,
                (Bounds.Height - text.Height) / 2));
            return;
        }

        var frame = _frames[_currentFrameIndex];

        // Calculate scale to fit frame in preview
        var scaleX = Bounds.Width / frame.Width;
        var scaleY = Bounds.Height / frame.Height;
        var scale = Math.Min(scaleX, scaleY) * 0.9; // 90% to leave some padding

        var displayWidth = frame.Width * scale;
        var displayHeight = frame.Height * scale;
        var x = (Bounds.Width - displayWidth) / 2;
        var y = (Bounds.Height - displayHeight) / 2;

        // Draw frame
        var sourceRect = new Rect(frame.X, frame.Y, frame.Width, frame.Height);
        var destRect = new Rect(x, y, displayWidth, displayHeight);

        context.DrawImage(_sourceImage, sourceRect, destRect);

        // Draw border
        var pen = new Pen(Brushes.Cyan, 2);
        context.DrawRectangle(null, pen, destRect);

        // Draw frame info
        var info = $"{_currentFrameIndex + 1}/{_frames.Count} - {frame.Name}";
        var infoText = new FormattedText(
            info,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            10,
            Brushes.White);
        
        context.DrawText(infoText, new Point(4, 4));

        // Draw play indicator
        if (_isPlaying)
        {
            var playText = new FormattedText(
                "▶",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.Lime);
            
            context.DrawText(playText, new Point(Bounds.Width - 20, 4));
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Stop();
        _sourceImage?.Dispose();
    }
}
