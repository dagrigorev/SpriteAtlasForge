using System.Text.Json;
using System.Text.Json.Serialization;
using SpriteAtlasForge.Core.Models;

namespace SpriteAtlasForge.Core.Export;

public class AtlasExporter
{
    private readonly JsonSerializerOptions _options;

    public AtlasExporter()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<string> ExportToJsonAsync(AtlasProject project, string outputPath)
    {
        var atlasData = BuildAtlasData(project);
        var json = JsonSerializer.Serialize(atlasData, _options);
        
        await File.WriteAllTextAsync(outputPath, json);
        return json;
    }

    public string ExportToJsonString(AtlasProject project)
    {
        var atlasData = BuildAtlasData(project);
        return JsonSerializer.Serialize(atlasData, _options);
    }

    private Dictionary<string, object> BuildAtlasData(AtlasProject project)
    {
        var result = new Dictionary<string, object>
        {
            ["version"] = 1,
            ["texture"] = project.SourceImage?.GetFileName() ?? "unknown.png",
            ["groups"] = BuildGroups(project)
        };

        return result;
    }

    private Dictionary<string, object> BuildGroups(AtlasProject project)
    {
        var groups = new Dictionary<string, object>();

        foreach (var group in project.Groups.Where(g => g.ExportEnabled))
        {
            var groupKey = SanitizeKey(group.Name);
            
            switch (group.Type)
            {
                case GridGroupType.Character:
                case GridGroupType.Enemy:
                case GridGroupType.Boss:
                    groups[groupKey] = BuildCharacterGroup(group);
                    break;
                case GridGroupType.Tile:
                    groups[groupKey] = BuildTileGroup(group);
                    break;
                case GridGroupType.Parallax:
                    groups[groupKey] = BuildParallaxGroup(group);
                    break;
                default:
                    groups[groupKey] = BuildGenericGroup(group);
                    break;
            }
        }

        return groups;
    }

    private Dictionary<string, object> BuildCharacterGroup(GridGroup group)
    {
        var result = new Dictionary<string, object>
        {
            ["type"] = group.Type.ToString().ToLowerInvariant()
        };

        var animations = new Dictionary<string, object>();

        if (group.Animations.Count > 0)
        {
            foreach (var animation in group.Animations.Where(a => a.HasFrames()))
            {
                animations[SanitizeKey(animation.Name)] = new Dictionary<string, object>
                {
                    ["fps"] = animation.Fps,
                    ["loop"] = animation.Loop,
                    ["frames"] = animation.Frames
                        .Where(f => f.Enabled)
                        .Select(f => BuildFrameData(f))
                        .ToList()
                };
            }
        }
        else
        {
            // If no animations defined, export all frames as individual frames
            var frames = group.Frames
                .Where(f => f.Enabled)
                .Select(f => BuildFrameData(f))
                .ToList();

            if (frames.Any())
            {
                animations["default"] = new Dictionary<string, object>
                {
                    ["fps"] = group.PreviewFps,
                    ["loop"] = group.PreviewLoop,
                    ["frames"] = frames
                };
            }
        }

        result["animations"] = animations;
        return result;
    }

    private Dictionary<string, object> BuildTileGroup(GridGroup group)
    {
        var result = new Dictionary<string, object>
        {
            ["type"] = "tileset",
            ["frames"] = new Dictionary<string, object>()
        };

        var framesDict = (Dictionary<string, object>)result["frames"];

        foreach (var frame in group.Frames.Where(f => f.Enabled))
        {
            var frameKey = SanitizeKey(frame.Name);
            var frameData = new Dictionary<string, object>
            {
                ["x"] = frame.X,
                ["y"] = frame.Y,
                ["w"] = frame.Width,
                ["h"] = frame.Height
            };

            if (frame.TileKind.HasValue)
            {
                frameData["kind"] = ConvertTileKind(frame.TileKind.Value);
                frameData["solid"] = IsSolidTile(frame.TileKind.Value);
            }

            if (frame.Tags.Any())
            {
                frameData["tags"] = frame.Tags;
            }

            framesDict[frameKey] = frameData;
        }

        return result;
    }

    private Dictionary<string, object> BuildParallaxGroup(GridGroup group)
    {
        var result = new Dictionary<string, object>
        {
            ["type"] = "parallax",
            ["layers"] = group.ParallaxLayers
                .OrderBy(l => l.RenderOrder)
                .Select(l => new Dictionary<string, object>
                {
                    ["name"] = l.Name,
                    ["x"] = l.X,
                    ["y"] = l.Y,
                    ["w"] = l.Width,
                    ["h"] = l.Height,
                    ["scrollFactor"] = l.ScrollFactor,
                    ["repeatX"] = l.RepeatX,
                    ["repeatY"] = l.RepeatY,
                    ["opacity"] = l.Opacity,
                    ["tint"] = l.Tint
                })
                .ToList()
        };

        return result;
    }

    private Dictionary<string, object> BuildGenericGroup(GridGroup group)
    {
        var result = new Dictionary<string, object>
        {
            ["type"] = group.Type.ToString().ToLowerInvariant(),
            ["frames"] = group.Frames
                .Where(f => f.Enabled)
                .Select(f => BuildFrameData(f))
                .ToList()
        };

        return result;
    }

    private Dictionary<string, object> BuildFrameData(SpriteFrame frame)
    {
        var data = new Dictionary<string, object>
        {
            ["name"] = frame.Name,
            ["x"] = frame.X,
            ["y"] = frame.Y,
            ["w"] = frame.Width,
            ["h"] = frame.Height
        };

        if (frame.Pivot != null)
        {
            var (pivotX, pivotY) = frame.Pivot.ToPixels(frame.Width, frame.Height);
            data["pivotX"] = pivotX;
            data["pivotY"] = pivotY;
        }

        if (frame.Duration != 0.125) // Only include if not default
        {
            data["duration"] = frame.Duration;
        }

        if (frame.Hitboxes.Any())
        {
            data["hitboxes"] = frame.Hitboxes.Select(h => new Dictionary<string, object>
            {
                ["x"] = h.X,
                ["y"] = h.Y,
                ["w"] = h.Width,
                ["h"] = h.Height,
                ["type"] = h.Type
            }).ToList();
        }

        if (frame.Tags.Any())
        {
            data["tags"] = frame.Tags;
        }

        return data;
    }

    private string SanitizeKey(string key)
    {
        return key.ToLowerInvariant()
            .Replace(" ", "_")
            .Replace("-", "_");
    }

    private string ConvertTileKind(TileKind kind)
    {
        return kind switch
        {
            TileKind.GroundLeft => "ground_left",
            TileKind.GroundCenter => "ground_center",
            TileKind.GroundRight => "ground_right",
            TileKind.GroundSingle => "ground_single",
            TileKind.CornerLeft => "corner_left",
            TileKind.CornerRight => "corner_right",
            TileKind.SlopeLeft => "slope_left",
            TileKind.SlopeRight => "slope_right",
            TileKind.Decoration => "decoration",
            TileKind.Hazard => "hazard",
            TileKind.Platform => "platform",
            _ => "unknown"
        };
    }

    private bool IsSolidTile(TileKind kind)
    {
        return kind switch
        {
            TileKind.GroundLeft => true,
            TileKind.GroundCenter => true,
            TileKind.GroundRight => true,
            TileKind.GroundSingle => true,
            TileKind.CornerLeft => true,
            TileKind.CornerRight => true,
            TileKind.SlopeLeft => true,
            TileKind.SlopeRight => true,
            TileKind.Platform => true,
            TileKind.Decoration => false,
            TileKind.Hazard => true,
            _ => false
        };
    }
}
