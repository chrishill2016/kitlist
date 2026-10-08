namespace KitList.Models;

public static class DisplayNames
{
    public static string Display(this KitCategory category) => category switch
    {
        KitCategory.Bcd => "BCD",
        KitCategory.Regulator => "Regulator",
        KitCategory.Cylinder => "Cylinder",
        KitCategory.Fins => "Fins",
        _ => category.ToString(),
    };

    public static string Display(this KitCondition condition) => condition switch
    {
        KitCondition.NeedsRepair => "Needs repair",
        KitCondition.OutOfService => "Out of service",
        _ => condition.ToString(),
    };

    public static string Display(this DueStatus status) => status switch
    {
        DueStatus.NotTracked => "Not tracked",
        DueStatus.Ok => "OK",
        DueStatus.DueSoon => "Due soon",
        DueStatus.NoRecord => "No record",
        DueStatus.Overdue => "Overdue",
        _ => status.ToString(),
    };

    public static string Display(this DateOnly? date) => date?.ToString("d MMM yyyy") ?? "—";

    public static string Display(this DateOnly date) => date.ToString("d MMM yyyy");
}

public static class KitLabels
{
    /// <summary>How an item appears on the session sheet: fins by size, everything else by name.</summary>
    public static string SheetLabel(this KitItem item) =>
        item.Category switch
        {
            KitCategory.Cylinder when !string.IsNullOrWhiteSpace(item.Size) => $"{item.Name} ({item.Size})",
            KitCategory.Fins when !string.IsNullOrWhiteSpace(item.Size) => item.Size!,
            _ => item.Name,
        };

    /// <summary>Label with the category in front, e.g. "BCD 2", unless it already mentions it ("Open fins").</summary>
    public static string PrefixedLabel(this KitItem item, string prefix)
    {
        var label = item.SheetLabel();
        return label.Contains(prefix, StringComparison.OrdinalIgnoreCase) ? label : $"{prefix} {label}";
    }

    /// <summary>Why this kit shouldn't be handed out, if there's a reason.</summary>
    public static string? Warning(this KitItem item, DateOnly today)
    {
        if (item.Condition is KitCondition.NeedsRepair or KitCondition.OutOfService)
            return item.Condition.Display();
        return ServiceRules.GetStatus(item, today) == DueStatus.Overdue ? "Service overdue" : null;
    }
}
