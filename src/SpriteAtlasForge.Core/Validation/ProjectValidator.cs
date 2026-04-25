using SpriteAtlasForge.Core.Models;

namespace SpriteAtlasForge.Core.Validation;

public class ProjectValidator
{
    public ValidationResult Validate(AtlasProject project)
    {
        var result = new ValidationResult();

        if (!project.HasSourceImage())
        {
            result.AddError("project", "Project", "No source image loaded");
            return result;
        }

        if (!project.HasGroups())
        {
            result.AddWarning("project", "Project", "No grid groups defined");
            return result;
        }

        var sourceImage = project.SourceImage!;

        foreach (var group in project.Groups)
        {
            ValidateGroup(group, sourceImage, result);
        }

        return result;
    }

    private void ValidateGroup(GridGroup group, SourceImage sourceImage, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(group.Name))
        {
            result.AddError(group.Id, "Unknown", "Group name is empty");
        }

        if (!group.ExportEnabled)
        {
            result.AddInfo(group.Id, group.Name, "Group export is disabled");
            return;
        }

        if (group.Frames.Count == 0)
        {
            result.AddWarning(group.Id, group.Name, "Group has no frames");
            return;
        }

        // Validate frames
        var enabledFrames = group.Frames.Where(f => f.Enabled).ToList();

        if (enabledFrames.Count == 0)
        {
            result.AddWarning(group.Id, group.Name, "All frames are disabled");
        }

        foreach (var frame in enabledFrames)
        {
            ValidateFrame(frame, group, sourceImage, result);
        }

        // Check for overlapping frames if not allowed
        if (!group.AllowOverlap)
        {
            for (int i = 0; i < enabledFrames.Count; i++)
            {
                for (int j = i + 1; j < enabledFrames.Count; j++)
                {
                    if (enabledFrames[i].IntersectsWith(enabledFrames[j]))
                    {
                        result.AddWarning(
                            group.Id, 
                            group.Name, 
                            $"Frames '{enabledFrames[i].Name}' and '{enabledFrames[j].Name}' overlap");
                    }
                }
            }
        }

        // Type-specific validation
        switch (group.Type)
        {
            case GridGroupType.Character:
            case GridGroupType.Enemy:
            case GridGroupType.Boss:
                ValidateCharacterGroup(group, result);
                break;
            case GridGroupType.Tile:
                ValidateTileGroup(group, result);
                break;
            case GridGroupType.Parallax:
                ValidateParallaxGroup(group, result);
                break;
        }
    }

    private void ValidateFrame(SpriteFrame frame, GridGroup group, SourceImage sourceImage, ValidationResult result)
    {
        if (frame.IsEmpty())
        {
            result.AddError(group.Id, group.Name, $"Frame '{frame.Name}' has invalid dimensions", frame.Name);
        }

        // Не нужно валидировать Out Of Bounds в grid
        //if (!frame.IsWithinBounds(sourceImage.Width, sourceImage.Height))
        //{
        //    result.AddError(
        //        group.Id, 
        //        group.Name, 
        //        $"Frame '{frame.Name}' is outside image bounds ({frame.X},{frame.Y},{frame.Width},{frame.Height})",
        //        frame.Name);
        //}
    }

    private void ValidateCharacterGroup(GridGroup group, ValidationResult result)
    {
        if (group.Animations.Count == 0)
        {
            result.AddWarning(group.Id, group.Name, "Character group has no animations defined");
            return;
        }

        foreach (var animation in group.Animations)
        {
            if (!animation.HasFrames())
            {
                result.AddWarning(
                    group.Id, 
                    group.Name, 
                    $"Animation '{animation.Name}' has no frames");
            }

            if (animation.Fps <= 0)
            {
                result.AddError(
                    group.Id, 
                    group.Name, 
                    $"Animation '{animation.Name}' has invalid FPS: {animation.Fps}");
            }
        }
    }

    private void ValidateTileGroup(GridGroup group, ValidationResult result)
    {
        var framesWithoutKind = group.Frames
            .Where(f => f.Enabled && !f.TileKind.HasValue)
            .ToList();

        if (framesWithoutKind.Any())
        {
            result.AddWarning(
                group.Id, 
                group.Name, 
                $"{framesWithoutKind.Count} tile frame(s) have no tile kind assigned");
        }
    }

    private void ValidateParallaxGroup(GridGroup group, ValidationResult result)
    {
        if (group.ParallaxLayers.Count == 0)
        {
            result.AddWarning(group.Id, group.Name, "Parallax group has no layers defined");
            return;
        }

        foreach (var layer in group.ParallaxLayers)
        {
            if (string.IsNullOrWhiteSpace(layer.Name))
            {
                result.AddError(group.Id, group.Name, "Parallax layer has no name");
            }

            if (layer.ScrollFactor < 0 || layer.ScrollFactor > 10)
            {
                result.AddWarning(
                    group.Id, 
                    group.Name, 
                    $"Parallax layer '{layer.Name}' has unusual scroll factor: {layer.ScrollFactor}");
            }

            if (layer.Opacity < 0 || layer.Opacity > 1)
            {
                result.AddError(
                    group.Id, 
                    group.Name, 
                    $"Parallax layer '{layer.Name}' has invalid opacity: {layer.Opacity}");
            }
        }
    }
}
