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

    /// <summary>
    /// Gets icon for the severity level
    /// </summary>
    public string SeverityIcon => Severity switch
    {
        ValidationSeverity.Info => "ℹ️",
        ValidationSeverity.Warning => "⚠️",
        ValidationSeverity.Error => "❌",
        _ => "•"
    };

    /// <summary>
    /// Gets color for the severity level
    /// </summary>
    public string SeverityColor => Severity switch
    {
        ValidationSeverity.Info => "#3794FF",     // Blue
        ValidationSeverity.Warning => "#FFB900",  // Orange
        ValidationSeverity.Error => "#E81123",    // Red
        _ => "#FFFFFF"
    };

    /// <summary>
    /// Gets formatted source location (group and frame if available)
    /// </summary>
    public string Source
    {
        get
        {
            if (!string.IsNullOrEmpty(FrameName))
                return $"Group: {GroupName} → Frame: {FrameName}";
            if (!string.IsNullOrEmpty(GroupName))
                return $"Group: {GroupName}";
            return string.Empty;
        }
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
