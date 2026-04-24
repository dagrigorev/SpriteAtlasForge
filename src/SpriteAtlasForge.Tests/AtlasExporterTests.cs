using SpriteAtlasForge.Core.Export;
using SpriteAtlasForge.Core.Models;
using System.Text.Json;
using Xunit;

namespace SpriteAtlasForge.Tests;

public class AtlasExporterTests
{
    private readonly AtlasExporter _exporter;

    public AtlasExporterTests()
    {
        _exporter = new AtlasExporter();
    }

    [Fact]
    public void ExportToJsonString_WithCharacterGroup_GeneratesValidJson()
    {
        // Arrange
        var project = CreateTestProject();
        var characterGroup = new GridGroup("Player", GridGroupType.Character);
        characterGroup.Frames.Add(new SpriteFrame("idle_001", 0, 0, 64, 64));
        characterGroup.Frames.Add(new SpriteFrame("idle_002", 64, 0, 64, 64));
        
        var animation = new AnimationDefinition("idle", AnimationType.Idle, 8.0, true);
        animation.Frames.Add(characterGroup.Frames[0]);
        animation.Frames.Add(characterGroup.Frames[1]);
        characterGroup.Animations.Add(animation);
        
        project.Groups.Add(characterGroup);

        // Act
        var json = _exporter.ExportToJsonString(project);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"version\": 1", json);
        Assert.Contains("\"player\"", json);
        Assert.Contains("\"animations\"", json);
        Assert.Contains("\"idle\"", json);
        Assert.Contains("\"fps\": 8", json);
    }

    [Fact]
    public void ExportToJsonString_WithTileGroup_GeneratesValidJson()
    {
        // Arrange
        var project = CreateTestProject();
        var tileGroup = new GridGroup("Tiles", GridGroupType.Tile);
        var frame = new SpriteFrame("ground_left", 0, 0, 32, 32);
        frame.TileKind = TileKind.GroundLeft;
        tileGroup.Frames.Add(frame);
        project.Groups.Add(tileGroup);

        // Act
        var json = _exporter.ExportToJsonString(project);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"tiles\"", json);
        Assert.Contains("\"type\": \"tileset\"", json);
        Assert.Contains("\"ground_left\"", json);
        Assert.Contains("\"solid\": true", json);
    }

    [Fact]
    public void ExportToJsonString_WithParallaxGroup_GeneratesValidJson()
    {
        // Arrange
        var project = CreateTestProject();
        var parallaxGroup = new GridGroup("Backgrounds", GridGroupType.Parallax);
        var layer = new ParallaxLayerDefinition("far_mountains", 0, 0, 1024, 256)
        {
            ScrollFactor = 0.1,
            RepeatX = true,
            Opacity = 1.0
        };
        parallaxGroup.ParallaxLayers.Add(layer);
        project.Groups.Add(parallaxGroup);

        // Act
        var json = _exporter.ExportToJsonString(project);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"backgrounds\"", json);
        Assert.Contains("\"type\": \"parallax\"", json);
        Assert.Contains("\"far_mountains\"", json);
        Assert.Contains("\"scrollFactor\": 0.1", json);
        Assert.Contains("\"repeatX\": true", json);
    }

    [Fact]
    public void ExportToJsonString_DisabledGroup_NotIncluded()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Disabled", GridGroupType.Item);
        group.ExportEnabled = false;
        group.Frames.Add(new SpriteFrame("item_001", 0, 0, 32, 32));
        project.Groups.Add(group);

        // Act
        var json = _exporter.ExportToJsonString(project);

        // Assert
        Assert.NotNull(json);
        Assert.DoesNotContain("\"disabled\"", json);
    }

    [Fact]
    public void ExportToJsonString_DisabledFrames_NotIncluded()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Items", GridGroupType.Item);
        group.Frames.Add(new SpriteFrame("item_001", 0, 0, 32, 32) { Enabled = true });
        group.Frames.Add(new SpriteFrame("item_002", 32, 0, 32, 32) { Enabled = false });
        project.Groups.Add(group);

        // Act
        var json = _exporter.ExportToJsonString(project);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"item_001\"", json);
        Assert.DoesNotContain("\"item_002\"", json);
    }

    [Fact]
    public void ExportToJsonString_GeneratesValidJsonStructure()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Test", GridGroupType.Character);
        group.Frames.Add(new SpriteFrame("test_001", 0, 0, 64, 64));
        project.Groups.Add(group);

        // Act
        var json = _exporter.ExportToJsonString(project);
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.NotNull(doc.RootElement.GetProperty("version"));
        Assert.NotNull(doc.RootElement.GetProperty("texture"));
        Assert.NotNull(doc.RootElement.GetProperty("groups"));
    }

    private AtlasProject CreateTestProject()
    {
        return new AtlasProject
        {
            ProjectName = "Test Project",
            SourceImage = new SourceImage("test.png", 1024, 1024, "PNG", 1024000)
        };
    }
}
