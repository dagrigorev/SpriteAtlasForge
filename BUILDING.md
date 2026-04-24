# Building and Running SpriteAtlasForge

## Prerequisites

### Required
- **.NET 8.0 SDK** or later
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
  - Verify: `dotnet --version` (should show 8.0.x or later)

### Platform-Specific

#### Windows
- Windows 10 version 1809 or later (recommended: Windows 11)
- Visual Studio 2022 (optional, for IDE support)

#### Linux
- Modern Linux distribution (Ubuntu 20.04+, Fedora 36+, etc.)
- libicu, libfontconfig, libx11 installed
  ```bash
  # Ubuntu/Debian
  sudo apt-get install libicu-dev libfontconfig1 libx11-6
  
  # Fedora
  sudo dnf install libicu fontconfig libX11
  ```

#### macOS
- macOS 10.15 (Catalina) or later
- Xcode Command Line Tools (optional)

## Quick Start

```bash
# 1. Clone or download the project
cd SpriteAtlasForge

# 2. Restore NuGet packages
dotnet restore

# 3. Build the solution
dotnet build

# 4. Run the application
dotnet run --project src/SpriteAtlasForge.App
```

## Detailed Build Instructions

### Step 1: Restore Dependencies

This downloads all NuGet packages:

```bash
dotnet restore SpriteAtlasForge.sln
```

Expected output:
```
Determining projects to restore...
Restored SpriteAtlasForge.Core
Restored SpriteAtlasForge.Rendering
Restored SpriteAtlasForge.App
Restored SpriteAtlasForge.Tests
```

### Step 2: Build

#### Debug Build (default)
```bash
dotnet build
```

#### Release Build (optimized)
```bash
dotnet build -c Release
```

### Step 3: Run

#### From Project Directory
```bash
dotnet run --project src/SpriteAtlasForge.App
```

#### From Binary
```bash
# Debug
./src/SpriteAtlasForge.App/bin/Debug/net8.0/SpriteAtlasForge.App

# Release
./src/SpriteAtlasForge.App/bin/Release/net8.0/SpriteAtlasForge.App
```

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Tests with Verbose Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Tests with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Creating a Distributable Package

### Windows

#### Self-Contained Executable
```bash
dotnet publish src/SpriteAtlasForge.App -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

Output: `src/SpriteAtlasForge.App/bin/Release/net8.0/win-x64/publish/SpriteAtlasForge.App.exe`

#### Framework-Dependent
```bash
dotnet publish src/SpriteAtlasForge.App -c Release -r win-x64 --self-contained false
```

### Linux

```bash
dotnet publish src/SpriteAtlasForge.App -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
```

Output: `src/SpriteAtlasForge.App/bin/Release/net8.0/linux-x64/publish/SpriteAtlasForge.App`

### macOS

```bash
dotnet publish src/SpriteAtlasForge.App -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true
```

Output: `src/SpriteAtlasForge.App/bin/Release/net8.0/osx-x64/publish/SpriteAtlasForge.App`

## Common Issues

### Issue: "SDK not found"

**Solution**: Install .NET 8.0 SDK from https://dotnet.microsoft.com/download

```bash
# Verify installation
dotnet --list-sdks
```

### Issue: "NU1101: Unable to find package"

**Solution**: Restore packages with explicit source

```bash
dotnet restore --source https://api.nuget.org/v3/index.json
```

### Issue: Build fails on Linux with font errors

**Solution**: Install fontconfig

```bash
sudo apt-get install libfontconfig1
```

### Issue: "Could not load file or assembly Avalonia"

**Solution**: Clean and rebuild

```bash
dotnet clean
dotnet restore
dotnet build
```

### Issue: Application doesn't start on macOS

**Solution**: Grant execute permission

```bash
chmod +x ./src/SpriteAtlasForge.App/bin/Debug/net8.0/SpriteAtlasForge.App
```

## Development Setup

### Visual Studio 2022

1. Open `SpriteAtlasForge.sln`
2. Set `SpriteAtlasForge.App` as startup project
3. Press F5 to run

### Visual Studio Code

1. Install C# extension
2. Open folder in VS Code
3. Press F5 (or use `.vscode/launch.json` configuration)

### JetBrains Rider

1. Open `SpriteAtlasForge.sln`
2. Run configuration will be auto-detected
3. Press Shift+F10 to run

## Performance Optimization

### Release Build
Always use Release configuration for production:
```bash
dotnet build -c Release
dotnet publish -c Release
```

### Ahead-of-Time (AOT) Compilation
For maximum performance (experimental):
```bash
dotnet publish -c Release -r win-x64 /p:PublishAot=true
```

**Note**: AOT compilation is experimental for Avalonia apps.

## Troubleshooting

### Check .NET Version
```bash
dotnet --version
dotnet --list-sdks
dotnet --list-runtimes
```

### Verbose Build Output
```bash
dotnet build -v detailed
```

### Clear NuGet Cache
```bash
dotnet nuget locals all --clear
dotnet restore --force
```

## Continuous Integration

### GitHub Actions Example

```yaml
name: Build and Test

on: [push, pull_request]

jobs:
  build:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore -c Release
    
    - name: Test
      run: dotnet test --no-build -c Release
```

## Next Steps

After successful build:

1. **Try the Example**: Open `examples/war_and_purr_assets.png` in the app
2. **Read Documentation**: See `README.md` for usage guide
3. **Run Tests**: `dotnet test` to verify everything works
4. **Start Creating**: Begin your sprite atlas project!

## Support

If you encounter build issues:

1. Check this guide first
2. Verify .NET SDK installation
3. Clear caches and retry
4. Open an issue with build output

---

**Build Status**: ✅ Should build successfully on Windows, Linux, and macOS with .NET 8.0+
