using System.ComponentModel.DataAnnotations;

namespace KitList.Models;

public enum KitCategory { Bcd, Regulator, Cylinder, Fins }

public enum KitCondition { Good, Fair, NeedsRepair, OutOfService }

public sealed class KitItem
{
    public string Id { get; set; } = "";

    public KitCategory Category { get; set; }

    [Required(ErrorMessage = "Give the item a name, e.g. \"BCD 3\" or \"12L steel #4\".")]
    public string Name { get; set; } = "";

    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Size { get; set; }
    public KitCondition Condition { get; set; } = KitCondition.Good;
    public string? Location { get; set; }
    public DateOnly? PurchaseDate { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Purchase cost can't be negative.")]
    public decimal? PurchaseCost { get; set; }

    public string? Notes { get; set; }

    public List<ServiceSchedule> Schedules { get; set; } = [];
    public List<ServiceRecord> ServiceLog { get; set; } = [];

    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>A recurring service or test, e.g. "Hydrostatic test" every 60 months.</summary>
public sealed class ServiceSchedule
{
    public string Type { get; set; } = "";

    [Range(1, 240)]
    public int IntervalMonths { get; set; } = 12;

    public DateOnly? LastDone { get; set; }

    public DateOnly? NextDue() => LastDone?.AddMonths(IntervalMonths);
}

public sealed class ServiceRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateOnly Date { get; set; }
    public string Type { get; set; } = "";
    public string? DoneBy { get; set; }
    public string? Notes { get; set; }
    public string? RecordedBy { get; set; }
}

public sealed class KitPhoto
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string DataUrl { get; set; } = "";
    public DateTimeOffset AddedAt { get; set; }
    public string? AddedBy { get; set; }
}
