# 📋 Log Panel - Real-Time Monitoring System

## Новая панель логов под canvas!

```
┌──────────────────────────────────────┐
│         Canvas (sprite sheet)        │
│                                      │
└──────────────────────────────────────┘
┌──────────────────────────────────────┐
│ 📋 Logs   [Filters] [AutoScroll] [Clear]│
├──────────────────────────────────────┤
│ 12:34:56.123 ℹ️  OpenProject  Loading...│
│ 12:34:56.456 ✅ OpenProject  Success! │
│ 12:34:57.789 ⚠️  Export  No frames   │
│ 12:34:58.012 ❌ Export  Failed!      │
└──────────────────────────────────────┘
```

---

## Features

### 1. 📊 Real-Time Logging
- All operations logged instantly
- Color-coded by severity
- Timestamps with milliseconds
- Category/source identification

### 2. 🎨 Log Levels
```
🔍 Debug   - #8E8E93 (Gray)   - Development info
ℹ️ Info    - #3794FF (Blue)   - General information
✅ Success - #10893E (Green)  - Successful operations
⚠️ Warning - #FFB900 (Orange) - Warnings
❌ Error   - #E81123 (Red)    - Errors & exceptions
```

### 3. 🔍 Filtering
- Toggle Debug/Info/Warning/Error
- Real-time filter updates
- Unchecked levels hidden instantly

### 4. 📜 Auto-Scroll
- Automatically scroll to latest entry
- Toggle on/off
- Stays at position when disabled

### 5. 🗑️ Clear Logs
- One-click to clear all logs
- Confirms with log entry
- Max 1000 entries (auto-trim)

---

## UI Layout

### Panel Structure
```xml
Grid (3 rows):
├─ Row 0: Toolbar (zoom, grid toggle)
├─ Row 1: Canvas (main sprite editor)
└─ Row 2: Log Panel (220px height)
    ├─ Header (filters, controls)
    └─ Log list (scrollable)
```

### Log Entry Format
```
[Timestamp] [Icon] [Category] [Message]
12:34:56.789  ✅   OpenProject  Project loaded successfully
```

### Header Controls
```
┌────────────────────────────────────────────┐
│ 📋 Logs  [🔍Debug] [ℹ️Info] [⚠️Warn] [❌Error] │
│          [📜 AutoScroll] [🗑️ Clear]         │
└────────────────────────────────────────────┘
```

---

## Usage in Code

### Logging Methods

```csharp
// Debug - development info
LogDebug("Canvas redraw triggered", "Canvas");

// Info - general information
LogInfo("Loading sprite sheet...", "ImageLoader");

// Success - successful operations
LogSuccess("Project saved successfully", "SaveProject");

// Warning - non-critical issues
LogWarning("Image file not found, using cached version", "ImageCache");

// Error - failures & exceptions
LogError("Failed to export atlas", "Export", exception);
```

### Example Usage

```csharp
private async Task OpenProject(string filePath)
{
    try
    {
        LogInfo($"Loading project: {Path.GetFileName(filePath)}", "OpenProject");
        
        var project = await _serializer.LoadAsync(filePath);
        
        LogSuccess($"Loaded {project.Groups.Count} groups", "OpenProject");
        
        if (project.SourceImage == null)
        {
            LogWarning("Project has no source image", "OpenProject");
        }
    }
    catch (Exception ex)
    {
        LogError($"Failed to load project", "OpenProject", ex);
    }
}
```

---

## Log Categories

### Core Operations
```
OpenProject  - Project loading
SaveProject  - Project saving
Export       - Atlas export
ImageLoader  - Image loading
Canvas       - Canvas operations
```

### Auto Detection
```
AutoDetect   - Automatic sprite detection
GridDetect   - Grid detection
Validation   - Project validation
```

### System
```
System       - Application startup/shutdown
UI           - UI updates
Error        - Unhandled errors
```

---

## Visual Examples

### Successful Operation
```
12:34:56.123 ℹ️  OpenProject  Loading project: myproject.safproj
12:34:56.456 ℹ️  OpenProject  Loaded 5 groups
12:34:56.789 ℹ️  OpenProject  Loading image: sprite.png
12:34:57.012 ✅ OpenProject  Project loaded successfully: myproject.safproj
```

### Warning Example
```
12:35:00.123 ℹ️  Export  Starting export to: atlas.json
12:35:00.456 ⚠️  Export  Frame 24 has no pivot defined
12:35:00.789 ⚠️  Export  3 frames skipped (disabled)
12:35:01.012 ✅ Export  Export completed: 48 frames
```

### Error Example
```
12:36:00.123 ℹ️  OpenProject  Loading project: test.safproj
12:36:00.456 ❌ OpenProject  File not found: test.safproj
12:36:00.789 ❌ OpenProject  Exception: System.IO.FileNotFoundException
```

---

## Benefits

### For Users
✅ **Real-time visibility** - see what's happening
✅ **Debugging** - identify issues immediately
✅ **Transparency** - understand operations
✅ **Error tracking** - find problems quickly

### For Developers
✅ **Diagnostics** - debug without debugger
✅ **User reports** - users can screenshot logs
✅ **Performance** - see operation timing
✅ **Testing** - verify operation flow

---

## Technical Details

### LogEntry Model
```csharp
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; }
    public string Category { get; set; }
    
    // Display properties
    public string FormattedTime => Timestamp.ToString("HH:mm:ss.fff");
    public string Icon => GetIcon(Level);
    public string LevelColor => GetColor(Level);
}
```

### Log Levels Enum
```csharp
public enum LogLevel
{
    Debug,    // 🔍 Development info
    Info,     // ℹ️ General info
    Success,  // ✅ Successful operations
    Warning,  // ⚠️ Warnings
    Error     // ❌ Errors
}
```

### Performance
- Max 1000 entries (auto-trim oldest)
- UI thread dispatch for thread safety
- Minimal memory footprint
- Fast filtering (no LINQ in UI binding)

---

## Configuration

### Adjust Panel Height
```xml
<!-- MainWindow.axaml -->
<Grid RowDefinitions="Auto,*,220">
                           ↑
                    Change this value
```

### Adjust Max Entries
```csharp
// MainViewModel.cs
private const int MaxLogEntries = 1000;  // Change this
```

### Customize Colors
```csharp
// LogEntry.cs
public string LevelColor => Level switch
{
    LogLevel.Error => "#E81123",   // Red
    LogLevel.Warning => "#FFB900", // Orange
    // ... customize here
};
```

---

## Example Output

### Application Startup
```
12:00:00.000 ℹ️  System  SpriteAtlasForge initialized
12:00:00.001 🔍 System  .NET Version: 8.0.0
```

### Loading Project
```
12:01:00.000 ℹ️  OpenProject  Loading project: demo.safproj
12:01:00.100 ✅ OpenProject  Loaded 3 groups
12:01:00.200 ℹ️  OpenProject  Loading image: characters.png
12:01:00.500 🔍 OpenProject  Rebuilding UI tree
12:01:00.600 ✅ OpenProject  Project loaded successfully: demo.safproj
```

### Auto Detection
```
12:02:00.000 ℹ️  AutoDetect  Starting auto-detection
12:02:01.000 ℹ️  AutoDetect  Analyzing image: 2048×2048
12:02:02.000 ℹ️  AutoDetect  Found 5 potential groups
12:02:02.100 ✅ AutoDetect  Created Characters (24 frames)
12:02:02.200 ✅ AutoDetect  Created Enemies (16 frames)
12:02:02.300 ✅ AutoDetect  Created Items (8 frames)
12:02:02.400 ✅ AutoDetect  Auto-detection complete: 48 frames
```

### Export
```
12:03:00.000 ℹ️  Export  Starting export to: atlas.json
12:03:00.100 ℹ️  Export  Exporting 48 frames from 3 groups
12:03:00.500 ✅ Export  Export completed successfully
```

---

## Keyboard Shortcuts

```
None currently - all interactions via mouse

Future:
Ctrl+L - Focus log panel
Ctrl+Shift+L - Clear logs
Ctrl+F - Find in logs
```

---

## Future Enhancements

### Planned Features
- 🔍 Search/filter by text
- 💾 Export logs to file
- 📊 Statistics (errors/warnings count)
- 🎨 Custom color themes
- 📌 Pin important entries
- 🔔 Notification badges
- 📈 Performance metrics
- 🕐 Relative timestamps ("2 seconds ago")

---

## Summary

**Professional logging system integrated into UI!**

✅ Real-time log display
✅ Color-coded severity levels
✅ Filtering by level
✅ Auto-scroll to latest
✅ One-click clear
✅ Thread-safe
✅ Performance optimized
✅ Developer-friendly API

**See exactly what's happening in your application!** 📋✨
