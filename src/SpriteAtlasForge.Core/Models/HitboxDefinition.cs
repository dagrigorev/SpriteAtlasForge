using System.Text.Json.Serialization;

namespace SpriteAtlasForge.Core.Models;

public class HitboxDefinition
{
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "default"; // "hitbox", "hurtbox", "collision"

    public HitboxDefinition() { }

    public HitboxDefinition(int x, int y, int width, int height, string type = "default")
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Type = type;
    }
}
