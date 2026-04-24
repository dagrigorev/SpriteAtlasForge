# SpriteAtlasForge

**Production-ready desktop application for creating sprite atlas JSON files from sprite sheets for 2D games.**

![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Avalonia](https://img.shields.io/badge/Avalonia-11.0-C679E0)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

### Core Functionality
- **Multiple Grid Groups**: Define multiple independent grids on a single sprite sheet
- **Manual Grid Configuration**: Precise control over origin, cell size, spacing, and padding
- **Auto-Generate Frames**: Automatically create sprite frames from grid definitions
- **Frame Management**: Enable/disable, rename, and tag individual frames
- **Animation Support**: Define character/enemy animations with FPS and loop settings
- **Tile Metadata**: Assign tile types (ground, platform, hazard, decoration, etc.)
- **Parallax Layers**: Configure scrolling backgrounds with scroll factors
- **Hitbox/Hurtbox**: Define collision boxes for combat mechanics
- **Pivot Points**: Set custom pivot points for proper sprite rotation

### UI Features
- **Dark Professional Theme**: Game dev tool aesthetic
- **Zoomable Canvas**: Pan and zoom sprite sheets with mouse/keyboard
- **Grid Overlay**: Toggle grid visualization
- **Live Preview**: See frame selections in real-time
- **Validation Panel**: Real-time validation with warnings and errors
- **Status Bar**: Mouse coordinates, zoom level, selected frame info
- **Keyboard Shortcuts**: Fast workflow with hotkeys

### Export & Validation
- **JSON Export**: Export to runtime-ready atlas JSON format
- **Validation**: Comprehensive validation before export
  - Frame bounds checking
  - Empty frame detection
  - Overlap detection
  - Animation completeness
  - Tile metadata verification
  - Parallax layer validation
- **Project Save/Load**: Save work as `.safproj` files

## Installation

### Prerequisites
- .NET 8.0 SDK or later
- Windows 10/11, Linux (x64), or macOS 10.15+

### Build from Source

```bash
# Clone repository
git clone https://github.com/yourusername/SpriteAtlasForge.git
cd SpriteAtlasForge

# Restore dependencies
dotnet restore

# Build
dotnet build -c Release

# Run
dotnet run --project src/SpriteAtlasForge.App
```

### Quick Start

```bash
dotnet restore
dotnet build
dotnet run --project src/SpriteAtlasForge.App
```

## Usage Guide

### Basic Workflow

1. **Open Image** (Ctrl+O)
   - Load your sprite sheet (PNG, JPG, WEBP)
   - Supported formats: PNG, JPEG, WEBP

2. **Create Grid Group** (Add button)
   - Click "Add" in the Groups panel
   - Set group name and type (Character, Enemy, Tile, Parallax, etc.)

3. **Configure Grid**
   - Set **Origin X/Y**: Starting point of the grid
   - Set **Cell Width/Height**: Size of each sprite frame
   - Set **Columns/Rows**: Number of cells in grid
   - Set **Spacing**: Gap between cells
   - Set **Padding**: Offset from origin

4. **Generate Frames**
   - Click "Generate Frames" to create frames from grid
   - Frames appear in the canvas with colored borders

5. **Customize Frames** (optional)
   - Select individual frames
   - Rename, tag, or disable frames
   - Add hitboxes/hurtboxes
   - Set pivot points

6. **Create Animations** (for Character/Enemy types)
   - Define animation sets (idle, walk, attack, etc.)
   - Assign frames to animations
   - Set FPS and loop behavior

7. **Validate** (Ctrl+Shift+V)
   - Check for errors before export
   - Fix any validation issues

8. **Export Atlas** (Ctrl+E)
   - Export to JSON file
   - Use in your game engine

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | New Project |
| `Ctrl+O` | Open Image |
| `Ctrl+S` | Save Project |
| `Ctrl+E` | Export Atlas JSON |
| `G` | Toggle Grid |
| `F` | Fit to Screen |
| `Ctrl+=` | Zoom In |
| `Ctrl+-` | Zoom Out |
| `Delete` | Remove Selected Group/Frame |
| `Ctrl+Z` | Undo *(coming soon)* |
| `Ctrl+Y` | Redo *(coming soon)* |

## Project Structure

```
SpriteAtlasForge/
├── src/
│   ├── SpriteAtlasForge.App/         # Main application
│   │   ├── ViewModels/               # MVVM ViewModels
│   │   ├── Views/                    # Avalonia UI views
│   │   ├── Converters/               # Value converters
│   │   └── Assets/                   # Application resources
│   ├── SpriteAtlasForge.Core/        # Core domain logic
│   │   ├── Models/                   # Data models
│   │   ├── Services/                 # Business logic services
│   │   ├── Export/                   # Atlas export engine
│   │   └── Validation/               # Validation logic
│   ├── SpriteAtlasForge.Rendering/   # Canvas rendering
│   │   ├── Canvas/                   # Canvas renderer
│   │   ├── Tools/                    # Drawing tools
│   │   └── Overlays/                 # Grid overlays
│   └── SpriteAtlasForge.Tests/       # Unit tests
├── examples/                         # Example projects
│   ├── war_and_purr.safproj         # Sample project
│   └── atlas_output.json            # Sample export
└── README.md
```

## File Formats

### Project File (.safproj)

JSON format storing:
- Source image path
- Grid groups with configurations
- Frame definitions
- Animations
- Parallax layers
- Export settings

Example: `examples/war_and_purr.safproj`

### Atlas Export (JSON)

Runtime-ready JSON format for game engines:

```json
{
  "version": 1,
  "texture": "sprites.png",
  "groups": {
    "player": {
      "type": "character",
      "animations": {
        "idle": {
          "fps": 8,
          "loop": true,
          "frames": [
            {
              "name": "idle_000",
              "x": 0,
              "y": 0,
              "w": 64,
              "h": 64,
              "pivotX": 32,
              "pivotY": 56,
              "duration": 0.125
            }
          ]
        }
      }
    }
  }
}
```

## Grid Group Types

### Character / Enemy / Boss
- Animation-based
- Supports: idle, walk, run, jump, fall, attack, hurt, death
- Frame-by-frame animation sequences
- Hitbox/hurtbox definitions
- Pivot points for rotation

### Tile
- Static tileset
- Tile types: ground, corner, slope, platform, decoration, hazard
- Solid/non-solid flags
- Perfect for platformer level design

### Parallax
- Scrolling background layers
- Scroll factor (0.0 - 1.0+)
- Repeat X/Y
- Render order
- Opacity and tint

### Item / Effect / UI / Decoration / Projectile
- Generic sprite groups
- Frame-based collections
- Flexible tagging system

## Validation Rules

### Errors (Block Export)
- ❌ Frame outside image bounds
- ❌ Empty frames (0 width/height)
- ❌ Invalid animation FPS (<= 0)
- ❌ Invalid parallax opacity (< 0 or > 1)
- ❌ No source image loaded

### Warnings (Allow Export)
- ⚠️ No frames in group
- ⚠️ No animations in character group
- ⚠️ Animation has no frames
- ⚠️ Tiles without tile kind
- ⚠️ Overlapping frames (if overlap disabled)

### Info
- ℹ️ Group export disabled
- ℹ️ Unusual scroll factors

## Testing

Run unit tests:

```bash
dotnet test
```

Test coverage includes:
- ✅ Atlas export validation
- ✅ Project validation rules
- ✅ Frame bounds checking
- ✅ Animation validation
- ✅ Tile metadata validation
- ✅ Parallax layer validation
- ✅ JSON serialization

## Technology Stack

- **Framework**: .NET 8.0
- **UI**: Avalonia UI 11.0
- **MVVM**: CommunityToolkit.Mvvm
- **Rendering**: SkiaSharp
- **Serialization**: System.Text.Json
- **Testing**: xUnit

## Architecture

**Clean Architecture** with separation of concerns:

1. **Core Layer**: Domain models, business logic, validation
2. **Rendering Layer**: Canvas rendering, visual tools
3. **App Layer**: UI, ViewModels, user interaction
4. **Tests Layer**: Comprehensive unit tests

**MVVM Pattern**: Reactive UI with data binding

## Roadmap

### v1.0 (Current)
- ✅ Multiple grid groups
- ✅ Manual grid configuration
- ✅ Frame generation
- ✅ Animation support
- ✅ Validation engine
- ✅ JSON export
- ✅ Project save/load

### Future Enhancements
- 🔜 Undo/Redo system
- 🔜 Auto-detect sprite boundaries
- 🔜 Animation preview playback
- 🔜 Batch processing
- 🔜 Template library
- 🔜 Multi-texture atlas support
- 🔜 Drag-and-drop frame selection
- 🔜 Color picker for borders

## Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Add tests for new features
4. Submit a pull request

## License

MIT License - See LICENSE file for details

## Support

- 📧 Issues: [GitHub Issues](https://github.com/yourusername/SpriteAtlasForge/issues)
- 📖 Documentation: This README
- 💬 Discussions: [GitHub Discussions](https://github.com/yourusername/SpriteAtlasForge/discussions)

## Credits

Developed with ❤️ for the 2D game development community.

Example sprite sheet "War & Purr" used for demonstration purposes.

---

**Happy Atlas Forging! 🔨✨**
