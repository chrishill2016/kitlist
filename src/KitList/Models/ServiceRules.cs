namespace KitList.Models;

/// <summary>Ordered by severity so the worst status of several schedules is simply the max.</summary>
public enum DueStatus { NotTracked, Ok, DueSoon, NoRecord, Overdue }

public static class ServiceRules
{
    public const int DueSoonDays = 30;

    /// <summary>Starting schedules for a new item. They can be changed per item.</summary>
    public static List<ServiceSchedule> DefaultSchedules(KitCategory category) => category switch
    {
        KitCategory.Bcd => [new() { Type = "Annual service", IntervalMonths = 12 }],
        KitCategory.Regulator => [new() { Type = "Annual service", IntervalMonths = 12 }],
        KitCategory.Cylinder =>
        [
            new() { Type = "Visual inspection", IntervalMonths = 12 },
            new() { Type = "Hydrostatic test", IntervalMonths = 60 },
        ],
        _ => [],
    };

    public static DueStatus GetStatus(ServiceSchedule schedule, DateOnly today)
    {
        if (schedule.NextDue() is not { } due)
            return DueStatus.NoRecord;
        if (due < today)
            return DueStatus.Overdue;
        if (due <= today.AddDays(DueSoonDays))
            return DueStatus.DueSoon;
        return DueStatus.Ok;
    }

    public static DueStatus GetStatus(KitItem item, DateOnly today) =>
        item.Schedules.Count == 0
            ? DueStatus.NotTracked
            : item.Schedules.Max(s => GetStatus(s, today));

    /// <summary>Earliest upcoming due date across the item's schedules, if any are known.</summary>
    public static DateOnly? NextDue(KitItem item) =>
        item.Schedules.Select(s => s.NextDue()).Where(d => d.HasValue).Min();

    /// <summary>Adds the record to the log and moves the matching schedule's "last done" forward.</summary>
    public static void ApplyServiceRecord(KitItem item, ServiceRecord record)
    {
        item.ServiceLog.Add(record);
        item.ServiceLog.Sort((a, b) => b.Date.CompareTo(a.Date));

        var schedule = item.Schedules.FirstOrDefault(s =>
            string.Equals(s.Type.Trim(), record.Type.Trim(), StringComparison.OrdinalIgnoreCase));
        if (schedule is not null && (schedule.LastDone is null || record.Date > schedule.LastDone))
            schedule.LastDone = record.Date;
    }

    public static DateOnly Today() => DateOnly.FromDateTime(DateTime.Now);
}
