using System.Text.Json.Serialization;

namespace KitList.Models;

/// <summary>One club dive/pool session: who is using which cylinder, BCD, fins and reg. Stored under sessions/{yyyy-MM-dd}.</summary>
public sealed class DiveSession
{
    public string Id { get; set; } = "";
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    public List<SessionRow> Rows { get; set; } = [];
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>A line of the kit sheet. There's one per cylinder; the other kit is optional.</summary>
public sealed class SessionRow
{
    public string CylinderId { get; set; } = "";

    // Free text, like the paper sheet: "200", "full", "30?".
    public string? StartPressure { get; set; }
    public string? EndPressure { get; set; }

    public string? BcdId { get; set; }
    public string? FinsId { get; set; }
    public string? RegId { get; set; }
    public string? Diver { get; set; }

    [JsonIgnore]
    public bool IsAssigned =>
        !string.IsNullOrWhiteSpace(Diver) || BcdId is { Length: > 0 } || FinsId is { Length: > 0 } || RegId is { Length: > 0 };
}
