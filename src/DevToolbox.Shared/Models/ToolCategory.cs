namespace DevToolbox.Shared.Models;

/// <summary>
/// Tool category model for navigation (AI Accept)
/// </summary>
public class ToolCategory
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsExpanded { get; set; } = true;
    public List<ToolItem> Tools { get; set; } = new();
}

/// <summary>
/// Tool item model (AI Accept)
/// </summary>
public class ToolItem
{
    public string Name { get; set; }
    public string Route { get; set; }

    public ToolItem(string name, string route)
    {
        Name = name;
        Route = route;
    }
}
