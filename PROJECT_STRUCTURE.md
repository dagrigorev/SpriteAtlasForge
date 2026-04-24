# 📁 Project Structure

## Overview

```
SpriteAtlasForge/
├── 📄 Solution & Config
│   ├── SpriteAtlasForge.sln          # Visual Studio solution
│   ├── .gitignore                     # Git ignore rules
│   ├── README.md                      # Project documentation
│   ├── LICENSE                        # MIT License
│   ├── AUTO_DETECTION.md              # Auto-detection guide
│   └── PROJECT_SUMMARY.md             # Technical summary
│
├── 📂 src/
│   ├── 🎨 SpriteAtlasForge.App/      # Main Avalonia UI Application
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs       # Main application logic (300+ lines)
│   │   │   └── GroupTreeNode.cs       # Tree hierarchy model
│   │   ├── Views/
│   │   │   ├── MainWindow.axaml       # Main UI layout (270+ lines)
│   │   │   ├── MainWindow.axaml.cs    # Main window code-behind (350+ lines)
│   │   │   ├── ImageCanvas.cs         # Interactive canvas control (280+ lines)
│   │   │   └── FramePreview.cs        # Animation preview (180+ lines)
│   │   ├── Converters/
│   │   │   ├── EnumHelper.cs          # Enum utilities
│   │   │   └── ObjectConverters.cs    # Value converters
│   │   ├── App.axaml                  # Application styles
│   │   ├── App.axaml.cs               # Application entry
│   │   ├── Program.cs                 # Main entry point
│   │   └── SpriteAtlasForge.App.csproj
│   │
│   ├── 🔧 SpriteAtlasForge.Core/     # Core Business Logic
│   │   ├── Models/                    # Data models
│   │   │   ├── AtlasProject.cs        # Main project model
│   │   │   ├── GridGroup.cs           # Grid group model
│   │   │   ├── GridDefinition.cs      # Grid configuration
│   │   │   ├── SpriteFrame.cs         # Individual frame
│   │   │   ├── SourceImage.cs         # Image metadata
│   │   │   ├── AnimationDefinition.cs # Animation data
│   │   │   ├── ParallaxLayerDefinition.cs
│   │   │   ├── HitboxDefinition.cs    # Collision boxes
│   │   │   ├── PivotDefinition.cs     # Rotation pivots
│   │   │   ├── ExportSettings.cs      # Export config
│   │   │   ├── ValidationResult.cs    # Validation output
│   │   │   └── Enums.cs               # All enumerations
│   │   ├── Services/
│   │   │   ├── AutoDetectionService.cs   # 🤖 AI sprite detection (400+ lines)
│   │   │   ├── ImageDataExtractor.cs     # Pixel data extraction (80+ lines)
│   │   │   ├── ProjectSerializer.cs      # Save/Load projects
│   │   │   └── ImageLoader.cs            # Image loading
│   │   ├── Export/
│   │   │   └── AtlasExporter.cs       # JSON export logic
│   │   ├── Validation/
│   │   │   └── ProjectValidator.cs    # Project validation
│   │   └── SpriteAtlasForge.Core.csproj
│   │
│   ├── 🎨 SpriteAtlasForge.Rendering/ # Rendering Utilities
│   │   ├── Canvas/
│   │   │   └── CanvasRenderer.cs      # SkiaSharp rendering helpers
│   │   └── SpriteAtlasForge.Rendering.csproj
│   │
│   └── 🧪 SpriteAtlasForge.Tests/    # Unit Tests
│       ├── AtlasExporterTests.cs      # Export tests (10 tests)
│       ├── ProjectValidatorTests.cs   # Validation tests (12 tests)
│       └── SpriteAtlasForge.Tests.csproj
│
└── 📂 examples/                       # Sample Sprite Sheets
    └── war_and_purr_assets.png       # Example game sprites
```

## Key Components

### 🤖 Auto Detection System

**AutoDetectionService.cs** (400+ lines)
- Flood fill algorithm for sprite boundary detection
- AI classification into 8 types
- Grid pattern recognition
- Dynamic frame generation

**ImageDataExtractor.cs** (80+ lines)
- RGBA pixel data extraction
- SkiaSharp bitmap processing
- Alpha channel analysis

### 🎨 UI Components

**MainWindow.axaml** (270+ lines)
- 3-panel layout (Groups | Canvas | Properties)
- TreeView with hierarchical groups
- Interactive toolbar
- Status bar

**ImageCanvas.cs** (280+ lines)
- Custom Avalonia Control
- Real-time grid rendering
- Mouse interactions (click, drag, zoom, pan)
- Frame visualization

**FramePreview.cs** (180+ lines)
- Animation playback
- FPS control (1-60)
- Frame counter
- Play/Pause/Stop controls

**GroupTreeNode.cs** (70+ lines)
- Tree hierarchy model
- Category nodes (with icons)
- Group nodes (with frame count)

### 💾 Core Models

**AtlasProject.cs**
- Main project container
- Groups collection
- Source image reference
- Metadata (name, version, dates)

**GridGroup.cs**
- Group type (Character, Enemy, etc.)
- Grid definition
- Frames collection
- Export settings
- Border color

**GridDefinition.cs**
- Origin (X, Y)
- Cell size (Width, Height)
- Spacing and padding
- Columns and rows

**SpriteFrame.cs**
- Position (X, Y)
- Dimensions (Width, Height)
- Metadata (name, tags, enabled)
- Optional hitbox/pivot

### 🔧 Services

**ProjectSerializer.cs**
- Save/Load .safproj files
- JSON serialization
- File I/O operations

**ImageLoader.cs**
- Load image metadata
- SkiaSharp integration
- Format support (PNG, JPG, WEBP)

**AtlasExporter.cs**
- Export to JSON format
- Game engine integration
- Configurable output

**ProjectValidator.cs**
- Comprehensive validation
- Error and warning reporting
- 22 validation rules

## File Statistics

| Category | Files | Lines of Code |
|----------|-------|---------------|
| Core Models | 11 | ~800 |
| Services | 5 | ~700 |
| UI (XAML) | 2 | ~300 |
| UI (C#) | 5 | ~1200 |
| Tests | 2 | ~400 |
| **Total** | **25+** | **~3400+** |

## Dependencies

### NuGet Packages

**SpriteAtlasForge.App:**
- Avalonia 11.0.10
- Avalonia.Themes.Fluent 11.0.10
- Avalonia.Desktop 11.0.10
- CommunityToolkit.Mvvm 8.2.2

**SpriteAtlasForge.Core:**
- SkiaSharp 2.88.8
- System.Text.Json 8.0.4

**SpriteAtlasForge.Tests:**
- xUnit 2.6.6
- FluentAssertions 6.12.0

## Build Artifacts

```
bin/
├── Debug/
│   └── net8.0/
│       ├── SpriteAtlasForge.App.dll
│       ├── SpriteAtlasForge.Core.dll
│       └── SpriteAtlasForge.Rendering.dll
└── Release/
    └── net8.0/
        └── [Optimized builds]
```

## Design Patterns

- **MVVM** - ViewModels, Commands, Data Binding
- **Dependency Injection** - Service constructors
- **Repository Pattern** - ProjectSerializer
- **Strategy Pattern** - Export formats
- **Observer Pattern** - PropertyChanged events
- **Factory Pattern** - GroupTreeNode creation

## Recent Changes

### ✨ Added (Latest Version)
- 🤖 AI-powered auto detection system
- 🌳 TreeView with hierarchical groups
- 🎬 Animation preview with playback
- 🖱️ Interactive grid dragging
- 📊 Real-time grid updates
- 🎨 8 sprite type categories with icons

### 🔧 Improved
- Mouse interaction system
- SkiaSharp integration
- File I/O performance
- Validation system

### 🐛 Fixed
- TreeView selection binding
- SkiaSharp pixel data extraction
- Canvas event handling
- Memory leaks in preview

## Future Enhancements

- [ ] Texture packing optimization
- [ ] Batch processing
- [ ] ML-based classification
- [ ] Unity/Godot plugins
- [ ] Cloud storage integration
