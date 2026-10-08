namespace KitList.Models;

public static class SessionRules
{
    public static string IdFor(DateOnly date) => date.ToString("yyyy-MM-dd");

    public static DiveSession Create(DateOnly date, IEnumerable<KitItem> cylinders, IEnumerable<DiveSession> sessions)
    {
        var session = new DiveSession { Id = IdFor(date), Date = date };
        AddMissingCylinders(session, cylinders, sessions);
        return session;
    }

    /// <summary>
    /// Adds a row for each in-service cylinder the session doesn't have yet, with its
    /// start pressure carried over from the last session that recorded one.
    /// </summary>
    public static void AddMissingCylinders(DiveSession session, IEnumerable<KitItem> cylinders, IEnumerable<DiveSession> sessions)
    {
        var earlier = sessions.Where(s => s.Date < session.Date).OrderByDescending(s => s.Date).ToList();
        foreach (var cylinder in cylinders)
        {
            if (cylinder.Category != KitCategory.Cylinder || cylinder.Condition == KitCondition.OutOfService)
                continue;
            if (session.Rows.Any(r => r.CylinderId == cylinder.Id))
                continue;
            session.Rows.Add(new SessionRow { CylinderId = cylinder.Id, StartPressure = LastKnownPressure(cylinder.Id, earlier) });
        }
    }

    /// <summary>The end pressure from the cylinder's latest session, or its start pressure if no end was noted.</summary>
    public static string? LastKnownPressure(string cylinderId, IEnumerable<DiveSession> earlierNewestFirst)
    {
        foreach (var session in earlierNewestFirst)
        {
            var row = session.Rows.FirstOrDefault(r => r.CylinderId == cylinderId);
            if (row is null)
                continue;
            if ((NullIfBlank(row.EndPressure) ?? NullIfBlank(row.StartPressure)) is { } pressure)
                return pressure;
        }
        return null;
    }

    /// <summary>IDs of kit given to more than one person in the session.</summary>
    public static HashSet<string> DoubleBooked(DiveSession session) =>
        session.Rows
            .SelectMany(r => new[] { r.BcdId, r.FinsId, r.RegId })
            .Where(id => !string.IsNullOrEmpty(id))
            .GroupBy(id => id!)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

    /// <summary>Trims text and turns empty values into nulls before saving.</summary>
    public static void Tidy(DiveSession session)
    {
        session.Notes = NullIfBlank(session.Notes);
        foreach (var row in session.Rows)
        {
            row.StartPressure = NullIfBlank(row.StartPressure);
            row.EndPressure = NullIfBlank(row.EndPressure);
            row.BcdId = NullIfBlank(row.BcdId);
            row.FinsId = NullIfBlank(row.FinsId);
            row.RegId = NullIfBlank(row.RegId);
            row.Diver = NullIfBlank(row.Diver);
        }
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
