using SpriteAtlasForge.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteAtlasForge.Core.Services;

/// <summary>
/// AI-powered sprite detection and classification service
/// Uses computer vision algorithms to automatically detect sprites in sprite sheets
/// </summary>
public class AutoDetectionService
{
    private const int MinSpriteSize = 8;
    private const int MaxSpriteSize = 1024;
    private const byte AlphaThreshold = 10;

    public class DetectionResult
    {
        public List<DetectedSprite> Sprites { get; set; } = new();
        public List<GridGroup> SuggestedGroups { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }

    public class DetectedSprite
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public SpriteType Type { get; set; }
        public double Confidence { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public enum SpriteType
    {
        Character,
        Enemy,
        Boss,
        Tile,
        ParallaxBackground,
        Item,
        Effect,
        UI,
        Unknown
    }

    /// <summary>
    /// Main detection method - analyzes image and returns detected sprites
    /// </summary>
    public DetectionResult DetectSprites(byte[] imageData, int width, int height)
    {
        var result = new DetectionResult();

        // Step 1: Find connected opaque regions using flood fill
        var sprites = FindOpaqueRegions(imageData, width, height);
        
        // Step 2: Filter by size constraints
        sprites = sprites.Where(s => 
            s.Width >= MinSpriteSize && s.Height >= MinSpriteSize &&
            s.Width <= MaxSpriteSize && s.Height <= MaxSpriteSize
        ).ToList();

        // Step 3: AI Classification - determine sprite type
        foreach (var sprite in sprites)
        {
            ClassifySprite(sprite, width, height, imageData);
        }

        result.Sprites = sprites;

        // Step 4: Group sprites into grid patterns
        result.SuggestedGroups = GroupSpritesIntoGrids(sprites, width, height);

        // Step 5: Generate summary
        result.Summary = GenerateSummary(result);

        return result;
    }

    /// <summary>
    /// Flood fill algorithm to find connected opaque regions
    /// </summary>
    private List<DetectedSprite> FindOpaqueRegions(byte[] imageData, int width, int height)
    {
        var visited = new bool[width * height];
        var sprites = new List<DetectedSprite>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 4;
                
                if (visited[y * width + x])
                    continue;

                byte alpha = imageData[index + 3];
                if (alpha <= AlphaThreshold)
                    continue;

                // Found opaque pixel - flood fill to find entire sprite
                var sprite = FloodFill(imageData, width, height, x, y, visited);
                
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }
        }

        return sprites;
    }

    /// <summary>
    /// Flood fill implementation for finding sprite boundaries
    /// </summary>
    private DetectedSprite? FloodFill(byte[] imageData, int width, int height, int startX, int startY, bool[] visited)
    {
        var queue = new Queue<(int x, int y)>();
        queue.Enqueue((startX, startY));

        int minX = startX, maxX = startX;
        int minY = startY, maxY = startY;
        int pixelCount = 0;

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            if (x < 0 || x >= width || y < 0 || y >= height)
                continue;

            int index = y * width + x;
            if (visited[index])
                continue;

            int pixelIndex = index * 4;
            byte alpha = imageData[pixelIndex + 3];

            if (alpha <= AlphaThreshold)
                continue;

            visited[index] = true;
            pixelCount++;

            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);

            // Check 4 neighbors
            queue.Enqueue((x + 1, y));
            queue.Enqueue((x - 1, y));
            queue.Enqueue((x, y + 1));
            queue.Enqueue((x, y - 1));
        }

        if (pixelCount < 10)
            return null;

        return new DetectedSprite
        {
            X = minX,
            Y = minY,
            Width = maxX - minX + 1,
            Height = maxY - minY + 1
        };
    }

    /// <summary>
    /// AI Classification based on sprite characteristics
    /// </summary>
    private void ClassifySprite(DetectedSprite sprite, int imageWidth, int imageHeight, byte[] imageData)
    {
        var area = sprite.Width * sprite.Height;
        var aspectRatio = (double)sprite.Width / sprite.Height;
        var relativeY = (double)sprite.Y / imageHeight;
        var relativeX = (double)sprite.X / imageWidth;
        var relativeSize = (double)area / (imageWidth * imageHeight);

        // Calculate pixel density (opaque pixel ratio)
        var density = CalculatePixelDensity(sprite, imageData, imageWidth);

        // BOSS: Very large sprites (>5% of image area)
        if (relativeSize > 0.05)
        {
            sprite.Type = SpriteType.Boss;
            sprite.Confidence = 0.85;
            sprite.Name = "Boss";
            return;
        }

        // PARALLAX BACKGROUND: Very wide, thin, at top
        if (aspectRatio > 3.0 && relativeY < 0.3 && sprite.Height < 300)
        {
            sprite.Type = SpriteType.ParallaxBackground;
            sprite.Confidence = 0.90;
            sprite.Name = "Parallax Layer";
            return;
        }

        // TILE: Small, square-ish, high density
        if (area < 4096 && aspectRatio > 0.7 && aspectRatio < 1.3 && density > 0.6)
        {
            sprite.Type = SpriteType.Tile;
            sprite.Confidence = 0.75;
            sprite.Name = "Tile";
            return;
        }

        // UI: Small, at edges
        if (area < 1600 && (relativeX < 0.1 || relativeX > 0.9 || relativeY < 0.1 || relativeY > 0.9))
        {
            sprite.Type = SpriteType.UI;
            sprite.Confidence = 0.70;
            sprite.Name = "UI Element";
            return;
        }

        // CHARACTER: Medium size, humanoid proportions, left side
        if (area >= 4096 && area <= 40000 && aspectRatio > 0.4 && aspectRatio < 2.5)
        {
            if (relativeX < 0.4)
            {
                sprite.Type = SpriteType.Character;
                sprite.Confidence = 0.80;
                sprite.Name = "Player";
                return;
            }
            else if (relativeX > 0.6)
            {
                sprite.Type = SpriteType.Enemy;
                sprite.Confidence = 0.75;
                sprite.Name = "Enemy";
                return;
            }
        }

        // EFFECT: Small, low density (particles, explosions)
        if (area < 4096 && density < 0.5)
        {
            sprite.Type = SpriteType.Effect;
            sprite.Confidence = 0.60;
            sprite.Name = "Effect";
            return;
        }

        // ITEM: Small to medium, medium density
        if (area < 8192 && density > 0.5)
        {
            sprite.Type = SpriteType.Item;
            sprite.Confidence = 0.65;
            sprite.Name = "Item";
            return;
        }

        sprite.Type = SpriteType.Unknown;
        sprite.Confidence = 0.3;
        sprite.Name = "Unknown";
    }

    /// <summary>
    /// Calculate ratio of opaque pixels in sprite bounding box
    /// </summary>
    private double CalculatePixelDensity(DetectedSprite sprite, byte[] imageData, int imageWidth)
    {
        int opaqueCount = 0;
        int totalPixels = sprite.Width * sprite.Height;

        for (int y = sprite.Y; y < sprite.Y + sprite.Height; y++)
        {
            for (int x = sprite.X; x < sprite.X + sprite.Width; x++)
            {
                int index = (y * imageWidth + x) * 4;
                if (imageData[index + 3] > AlphaThreshold)
                {
                    opaqueCount++;
                }
            }
        }

        return (double)opaqueCount / totalPixels;
    }

    /// <summary>
    /// Group sprites into grid patterns
    /// </summary>
    private List<GridGroup> GroupSpritesIntoGrids(List<DetectedSprite> sprites, int imageWidth, int imageHeight)
    {
        var groups = new List<GridGroup>();

        // Group by type first
        var spritesByType = sprites.GroupBy(s => s.Type);

        foreach (var typeGroup in spritesByType)
        {
            if (typeGroup.Key == SpriteType.Unknown)
                continue;

            var typeSprites = typeGroup.ToList();

            // Detect grid patterns for this type
            var gridPatterns = DetectGridPatterns(typeSprites);

            foreach (var pattern in gridPatterns)
            {
                var group = CreateGridGroupFromSprites(pattern, typeGroup.Key);
                if (group != null)
                {
                    groups.Add(group);
                }
            }
        }

        return groups;
    }

    /// <summary>
    /// Detect grid patterns in sprite list
    /// </summary>
    private List<List<DetectedSprite>> DetectGridPatterns(List<DetectedSprite> sprites)
    {
        var patterns = new List<List<DetectedSprite>>();
        var remaining = new List<DetectedSprite>(sprites);

        while (remaining.Count >= 3)
        {
            var pattern = FindLargestPattern(remaining);
            if (pattern.Count >= 3)
            {
                patterns.Add(pattern);
                remaining.RemoveAll(s => pattern.Contains(s));
            }
            else
            {
                break;
            }
        }

        return patterns;
    }

    /// <summary>
    /// Find largest consistent grid pattern
    /// </summary>
    private List<DetectedSprite> FindLargestPattern(List<DetectedSprite> sprites)
    {
        if (sprites.Count == 0)
            return new List<DetectedSprite>();

        // Sort by position
        var sorted = sprites.OrderBy(s => s.Y).ThenBy(s => s.X).ToList();

        var bestPattern = new List<DetectedSprite>();

        for (int i = 0; i < sorted.Count - 2; i++)
        {
            var pattern = new List<DetectedSprite> { sorted[i] };
            int refWidth = sorted[i].Width;
            int refHeight = sorted[i].Height;

            for (int j = i + 1; j < sorted.Count; j++)
            {
                bool matches = Math.Abs(sorted[j].Width - refWidth) <= 2 &&
                              Math.Abs(sorted[j].Height - refHeight) <= 2;

                if (matches)
                {
                    pattern.Add(sorted[j]);
                }
            }

            if (pattern.Count > bestPattern.Count)
            {
                bestPattern = pattern;
            }
        }

        return bestPattern;
    }

    /// <summary>
    /// Create GridGroup from detected sprites
    /// </summary>
    private GridGroup? CreateGridGroupFromSprites(List<DetectedSprite> sprites, SpriteType type)
    {
        if (sprites.Count == 0)
            return null;

        var minX = sprites.Min(s => s.X);
        var minY = sprites.Min(s => s.Y);
        var avgWidth = (int)sprites.Average(s => s.Width);
        var avgHeight = (int)sprites.Average(s => s.Height);

        // Detect grid structure
        var uniqueX = sprites.Select(s => s.X).Distinct().OrderBy(x => x).ToList();
        var uniqueY = sprites.Select(s => s.Y).Distinct().OrderBy(y => y).ToList();

        // Calculate spacing
        int spacing = 0;
        if (uniqueX.Count > 1)
        {
            var spacings = new List<int>();
            for (int i = 1; i < uniqueX.Count; i++)
            {
                int gap = uniqueX[i] - uniqueX[i - 1] - avgWidth;
                spacings.Add(gap);
            }
            spacing = spacings.Any() ? (int)spacings.Average() : 0;
            spacing = Math.Max(0, spacing);
        }

        var groupType = ConvertToGridGroupType(type);
        var groupName = GetGroupName(type, sprites.Count);
        
        var group = new GridGroup(groupName, groupType);
        group.GridDefinition = new GridDefinition
        {
            OriginX = minX,
            OriginY = minY,
            CellWidth = avgWidth,
            CellHeight = avgHeight,
            Columns = Math.Max(1, uniqueX.Count),
            Rows = Math.Max(1, uniqueY.Count),
            Spacing = spacing,
            Padding = 0
        };

        // Add frames
        for (int i = 0; i < sprites.Count; i++)
        {
            var sprite = sprites[i];
            var frame = new SpriteFrame(
                $"{groupName}_{i:D3}",
                sprite.X,
                sprite.Y,
                sprite.Width,
                sprite.Height
            );
            group.Frames.Add(frame);
        }

        return group;
    }

    private GridGroupType ConvertToGridGroupType(SpriteType type)
    {
        return type switch
        {
            SpriteType.Character => GridGroupType.Character,
            SpriteType.Enemy => GridGroupType.Enemy,
            SpriteType.Boss => GridGroupType.Boss,
            SpriteType.Tile => GridGroupType.Tile,
            SpriteType.ParallaxBackground => GridGroupType.Parallax,
            SpriteType.Item => GridGroupType.Item,
            SpriteType.Effect => GridGroupType.Effect,
            SpriteType.UI => GridGroupType.UI,
            _ => GridGroupType.Item
        };
    }

    private string GetGroupName(SpriteType type, int count)
    {
        return type switch
        {
            SpriteType.Character => "Player",
            SpriteType.Enemy => "Enemies",
            SpriteType.Boss => "Bosses",
            SpriteType.Tile => "Tiles",
            SpriteType.ParallaxBackground => "Parallax",
            SpriteType.Item => "Items",
            SpriteType.Effect => "Effects",
            SpriteType.UI => "UI",
            _ => "Unknown"
        };
    }

    private string GenerateSummary(DetectionResult result)
    {
        var summary = $"Detected {result.Sprites.Count} sprites\n";
        summary += $"Created {result.SuggestedGroups.Count} groups:\n";
        
        foreach (var group in result.SuggestedGroups)
        {
            summary += $"  • {group.Name}: {group.Frames.Count} frames\n";
        }

        return summary;
    }
}
