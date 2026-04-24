using System.Text.Json.Serialization;

namespace SpriteAtlasForge.Core.Models;

public class PivotDefinition
{
    [JsonPropertyName("x")]
    public double X { get; set; } = 0.5;

    [JsonPropertyName("y")]
    public double Y { get; set; } = 0.5;

    public PivotDefinition() { }

    public PivotDefinition(double x, double y)
    {
        X = x;
        Y = y;
    }

    public (int pixelX, int pixelY) ToPixels(int frameWidth, int frameHeight)
    {
        return ((int)(X * frameWidth), (int)(Y * frameHeight));
    }
}
