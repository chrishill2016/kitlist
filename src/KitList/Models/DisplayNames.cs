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
