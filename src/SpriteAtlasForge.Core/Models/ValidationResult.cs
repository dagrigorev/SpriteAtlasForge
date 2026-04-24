namespace SpriteAtlasForge.Core.Models;

public enum ValidationSeverity
{
    Info,
    Warning,
    Error
}

public class ValidationMessage
{
    public ValidationSeverity Severity { get; set; }
    public string GroupId { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? FrameName { get; set; }

    public ValidationMessage() { }

    public ValidationMessage(ValidationSeverity severity, string groupId, string groupName, string message, string? frameName = null)
    {
        Severity = severity;
        GroupId = groupId;
        GroupName = groupName;
        Message = message;
        FrameName = frameName;
    }
}

public class ValidationResult
{
    public List<ValidationMessage> Messages { get; set; } = new();
    public bool IsValid => !Messages.Any(m => m.Severity == ValidationSeverity.Error);
    public int ErrorCount => Messages.Count(m => m.Severity == ValidationSeverity.Error);
    public int WarningCount => Messages.Count(m => m.Severity == ValidationSeverity.Warning);
    public int InfoCount => Messages.Count(m => m.Severity == ValidationSeverity.Info);

    public void AddError(string groupId, string groupName, string message, string? frameName = null)
    {
        Messages.Add(new ValidationMessage(ValidationSeverity.Error, groupId, groupName, message, frameName));
    }

    public void AddWarning(string groupId, string groupName, string message, string? frameName = null)
    {
        Messages.Add(new ValidationMessage(ValidationSeverity.Warning, groupId, groupName, message, frameName));
    }

    public void AddInfo(string groupId, string groupName, string message, string? frameName = null)
    {
        Messages.Add(new ValidationMessage(ValidationSeverity.Info, groupId, groupName, message, frameName));
    }
}
