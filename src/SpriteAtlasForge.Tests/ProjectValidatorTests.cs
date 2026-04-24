using SpriteAtlasForge.Core.Models;
using SpriteAtlasForge.Core.Validation;
using Xunit;

namespace SpriteAtlasForge.Tests;

public class ProjectValidatorTests
{
    private readonly ProjectValidator _validator;

    public ProjectValidatorTests()
    {
        _validator = new ProjectValidator();
    }

    [Fact]
    public void Validate_NoSourceImage_ReturnsError()
    {
        // Arrange
        var project = new AtlasProject();

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Messages, m => m.Severity == ValidationSeverity.Error);
    }

    [Fact]
    public void Validate_NoGroups_ReturnsWarning()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.True(result.IsValid); // Only warning, not error
        Assert.Contains(result.Messages, m => m.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_FrameOutsideBounds_ReturnsError()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Test", GridGroupType.Character);
        group.Frames.Add(new SpriteFrame("test", 2000, 2000, 64, 64)); // Outside 1024x1024
        project.Groups.Add(group);

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Messages, m => 
            m.Severity == ValidationSeverity.Error && 
            m.Message.Contains("outside image bounds"));
    }

    [Fact]
    public void Validate_EmptyFrame_ReturnsError()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Test", GridGroupType.Character);
        group.Frames.Add(new SpriteFrame("test", 0, 0, 0, 0)); // Empty frame
        project.Groups.Add(group);

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Messages, m => 
            m.Severity == ValidationSeverity.Error && 
            m.Message.Contains("invalid dimensions"));
    }

    [Fact]
    public void Validate_CharacterWithNoAnimations_ReturnsWarning()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Player", GridGroupType.Character);
        group.Frames.Add(new SpriteFrame("test", 0, 0, 64, 64));
        project.Groups.Add(group);

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.True(result.IsValid);
        Assert.Contains(result.Messages, m => 
            m.Severity == ValidationSeverity.Warning && 
            m.Message.Contains("no animations"));
    }

    [Fact]
    public void Validate_AnimationWithNoFrames_ReturnsWarning()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Player", GridGroupType.Character);
        var animation = new AnimationDefinition("idle", AnimationType.Idle);
        group.Animations.Add(animation);
        project.Groups.Add(group);

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.Contains(result.Messages, m => 
            m.Severity == ValidationSeverity.Warning && 
            m.Message.Contains("has no frames"));
    }

    [Fact]
    public void Validate_InvalidAnimationFps_ReturnsError()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Player", GridGroupType.Character);
        var animation = new AnimationDefinition("idle", AnimationType.Idle, -1.0); // Invalid FPS
        animation.Frames.Add(new SpriteFrame("test", 0, 0, 64, 64));
        group.Animations.Add(animation);
        project.Groups.Add(group);

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Messages, m => 
            m.Severity == ValidationSeverity.Error && 
            m.Message.Contains("invalid FPS"));
    }

    [Fact]
    public void Validate_TileWithoutKind_ReturnsWarning()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Tiles", GridGroupType.Tile);
        group.Frames.Add(new SpriteFrame("tile", 0, 0, 32, 32)); // No TileKind set
        project.Groups.Add(group);

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.Contains(result.Messages, m => 
            m.Severity == ValidationSeverity.Warning && 
            m.Message.Contains("no tile kind"));
    }

    [Fact]
    public void Validate_ParallaxWithNoLayers_ReturnsWarning()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Background", GridGroupType.Parallax);
        project.Groups.Add(group);

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.Contains(result.Messages, m => 
            m.Severity == ValidationSeverity.Warning && 
            m.Message.Contains("no layers"));
    }

    [Fact]
    public void Validate_ParallaxLayerInvalidOpacity_ReturnsError()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Background", GridGroupType.Parallax);
        var layer = new ParallaxLayerDefinition("test", 0, 0, 100, 100)
        {
            Opacity = 2.0 // Invalid opacity > 1.0
        };
        group.ParallaxLayers.Add(layer);
        project.Groups.Add(group);

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Messages, m => 
            m.Severity == ValidationSeverity.Error && 
            m.Message.Contains("invalid opacity"));
    }

    [Fact]
    public void Validate_DisabledGroup_SkipsValidation()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Disabled", GridGroupType.Character);
        group.ExportEnabled = false;
        group.Frames.Add(new SpriteFrame("test", 9999, 9999, 64, 64)); // Would be error if checked
        project.Groups.Add(group);

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.Contains(result.Messages, m => 
            m.Severity == ValidationSeverity.Info && 
            m.Message.Contains("export is disabled"));
    }

    [Fact]
    public void Validate_ValidProject_ReturnsNoErrors()
    {
        // Arrange
        var project = CreateTestProject();
        var group = new GridGroup("Player", GridGroupType.Character);
        var frame = new SpriteFrame("idle_001", 0, 0, 64, 64);
        group.Frames.Add(frame);
        
        var animation = new AnimationDefinition("idle", AnimationType.Idle, 8.0);
        animation.Frames.Add(frame);
        group.Animations.Add(animation);
        
        project.Groups.Add(group);

        // Act
        var result = _validator.Validate(project);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(0, result.ErrorCount);
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
