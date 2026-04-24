using System.Text.Json;
using System.Text.Json.Serialization;
using SpriteAtlasForge.Core.Models;

namespace SpriteAtlasForge.Core.Services;

public class ProjectSerializer
{
    private readonly JsonSerializerOptions _options;

    public ProjectSerializer()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }

    public async Task SaveProjectAsync(AtlasProject project, string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(project, _options);
            await File.WriteAllTextAsync(filePath, json);
            project.FilePath = filePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save project: {ex.Message}", ex);
        }
    }

    public async Task<AtlasProject> LoadProjectAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Project file not found: {filePath}");

            var json = await File.ReadAllTextAsync(filePath);
            var project = JsonSerializer.Deserialize<AtlasProject>(json, _options);

            if (project == null)
                throw new InvalidOperationException("Failed to deserialize project");

            project.FilePath = filePath;
            return project;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load project: {ex.Message}", ex);
        }
    }

    public bool CanLoadProject(string filePath)
    {
        try
        {
            return File.Exists(filePath) && 
                   Path.GetExtension(filePath).Equals(".safproj", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
