using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SpriteAtlasForge.Core.Models;

namespace SpriteAtlasForge.App.ViewModels;

/// <summary>
/// Tree node for hierarchical group display
/// </summary>
public partial class GroupTreeNode : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private bool _isExpanded = true;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private GridGroup? _group;

    [ObservableProperty]
    private GridGroupType? _groupType;

    public ObservableCollection<GroupTreeNode> Children { get; } = new();

    public bool IsCategory => Group == null && GroupType.HasValue;
    public bool IsGroup => Group != null;

    public GroupTreeNode(string name, string icon)
    {
        Name = name;
        Icon = icon;
    }

    public static GroupTreeNode CreateCategory(GridGroupType type)
    {
        var (name, icon) = type switch
        {
            GridGroupType.Character => ("Characters", "🎮"),
            GridGroupType.Enemy => ("Enemies", "👾"),
            GridGroupType.Boss => ("Bosses", "💀"),
            GridGroupType.Tile => ("Tiles", "🧱"),
            GridGroupType.Parallax => ("Parallax", "🌄"),
            GridGroupType.Item => ("Items", "💎"),
            GridGroupType.Effect => ("Effects", "✨"),
            GridGroupType.UI => ("UI Elements", "🖼️"),
            _ => ("Other", "📦")
        };

        return new GroupTreeNode(name, icon)
        {
            GroupType = type
        };
    }

    public static GroupTreeNode CreateGroupNode(GridGroup group)
    {
        return new GroupTreeNode(group.Name, "")
        {
            Group = group
        };
    }
}
