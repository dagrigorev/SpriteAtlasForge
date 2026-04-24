# 🎮 SpriteAtlasForge

**Professional sprite atlas generator for 2D game development**

A powerful desktop application for creating optimized sprite atlases with **AI-powered automatic detection**, dynamic grid management, and real-time animation preview.

## ✨ Key Features

### 🤖 AI-Powered Auto Detection
- **Automatic sprite detection** using computer vision
- **Smart classification** into 8 types (Character, Enemy, Boss, Tile, etc.)
- **Grid pattern recognition** with auto-parameters
- **Dynamic sizing** for variable sprite dimensions

### 🎯 Professional Tools
- **Multi-grid system** - multiple grids per image
- **TreeView organization** - hierarchical display by type
- **Real-time animation preview** (1-60 FPS)
- **Interactive canvas** with zoom, pan, drag grids
- **Frame management** - rename, tag, enable/disable
- **JSON export** for game engines

## 🚀 Quick Start

### Installation
```bash
# Clone and build
git clone https://github.com/yourusername/SpriteAtlasForge.git
cd SpriteAtlasForge
dotnet build
dotnet run --project src/SpriteAtlasForge.App
```

### 3-Click Workflow
1. **Open Image** (Ctrl+O)
2. **Auto Detect** (🤖 button)
3. **Export** (Ctrl+E)

## 📖 Usage

### Auto Detection

AI automatically detects and classifies sprites:

| Type | Criteria | Confidence |
|------|----------|------------|
| 💀 Boss | Large (>5% image) | 85% |
| 🌄 Parallax | Wide (>3:1), top | 90% |
| 🧱 Tile | Small, square | 75% |
| 🎮 Character | Medium, left | 80% |
| 👾 Enemy | Medium, right | 75% |

### Controls

**Mouse:**
- Click - Select group
- Ctrl+Drag - Move grid
- Shift+Drag - Pan
- Ctrl+Scroll - Zoom

**Keyboard:**
- Ctrl+O - Open
- Ctrl+S - Save  
- Ctrl+E - Export
- G - Toggle grid
- F - Fit screen

## 🏗️ Architecture

```
SpriteAtlasForge/
├── src/
│   ├── SpriteAtlasForge.App/      # Avalonia UI
│   ├── SpriteAtlasForge.Core/     # Business logic
│   ├── SpriteAtlasForge.Rendering/# Graphics
│   └── SpriteAtlasForge.Tests/    # Tests
└── examples/                       # Sample sprites
```

**Tech Stack:**
- .NET 8 + C# 12
- Avalonia UI 11
- SkiaSharp
- MVVM Pattern

## 📊 Performance

| Image | Sprites | Time |
|-------|---------|------|
| 1024² | ~50 | <1s |
| 2048² | ~100 | 1-2s |
| 4096² | ~200 | 3-5s |

## 📝 License

MIT License - see [LICENSE](LICENSE)

---

**Made with ❤️ for game developers**
