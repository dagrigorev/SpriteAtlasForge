using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteAtlasForge.App.Models;

/// <summary>
/// Represents a single log entry with timestamp, level, and message
/// </summary>
public partial class LogEntry : ObservableObject
{
    /// <summary>
    /// When the log entry was created
    /// </summary>
    [ObservableProperty]
    private DateTime _timestamp;

    /// <summary>
    /// Log level (Info, Warning, Error, Debug)
    /// </summary>
    [ObservableProperty]
    private LogLevel _level;

    /// <summary>
    /// The log message
    /// </summary>
    [ObservableProperty]
    private string _message = string.Empty;

    /// <summary>
    /// Optional category/source of the log
    /// </summary>
    [ObservableProperty]
    private string _category = string.Empty;

    public LogEntry()
    {
        Timestamp = DateTime.Now;
    }

    public LogEntry(LogLevel level, string message, string category = "")
    {
        Timestamp = DateTime.Now;
        Level = level;
        Message = message;
        Category = category;
    }

    /// <summary>
    /// Gets formatted timestamp for display
    /// </summary>
    public string FormattedTime => Timestamp.ToString("HH:mm:ss.fff");

    /// <summary>
    /// Gets icon based on log level
    /// </summary>
    public string Icon => Level switch
    {
        LogLevel.Info => "ℹ️",
        LogLevel.Warning => "⚠️",
        LogLevel.Error => "❌",
        LogLevel.Debug => "🔍",
        LogLevel.Success => "✅",
        _ => "•"
    };

    /// <summary>
    /// Gets color for the log level
    /// </summary>
    public string LevelColor => Level switch
    {
        LogLevel.Info => "#3794FF",      // Blue
        LogLevel.Warning => "#FFB900",   // Orange
        LogLevel.Error => "#E81123",     // Red
        LogLevel.Debug => "#8E8E93",     // Gray
        LogLevel.Success => "#10893E",   // Green
        _ => "#FFFFFF"
    };
}

/// <summary>
/// Log severity levels
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Success,
    Warning,
    Error
}
