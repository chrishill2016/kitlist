using KitList.Models;

namespace KitList.Tests;

public class SessionRulesTests
{
    private static readonly DateOnly Today = new(2026, 10, 8);

    private static KitItem Cylinder(string id, KitCondition condition = KitCondition.Good) =>
        new() { Id = id, Name = id, Category = KitCategory.Cylinder, Condition = condition };

    private static DiveSession Session(DateOnly date, params SessionRow[] rows) =>
        new() { Id = SessionRules.IdFor(date), Date = date, Rows = [.. rows] };

    [Fact]
    public void New_session_has_a_row_per_in_service_cylinder()
    {
        var cylinders = new[] { Cylinder("1"), Cylinder("2", KitCondition.OutOfService), Cylinder("3", KitCondition.NeedsRepair) };

        var session = SessionRules.Create(Today, cylinders, []);

        Assert.Equal("2026-10-08", session.Id);
        Assert.Equal(["1", "3"], session.Rows.Select(r => r.CylinderId));
    }

    [Fact]
    public void Start_pressure_carries_over_from_the_latest_earlier_end_pressure()
    {
        var sessions = new[]
        {
            Session(Today.AddDays(-14), new SessionRow { CylinderId = "1", StartPressure = "full", EndPressure = "150" }),
            Session(Today.AddDays(-7), new SessionRow { CylinderId = "1", StartPressure = "150", EndPressure = "80" }),
            Session(Today.AddDays(7), new SessionRow { CylinderId = "1", StartPressure = "full", EndPressure = "20" }),
        };

        var session = SessionRules.Create(Today, [Cylinder("1")], sessions);

        Assert.Equal("80", session.Rows.Single().StartPressure);
    }

    [Fact]
    public void Unused_cylinder_keeps_its_start_pressure()
    {
        var sessions = new[]
        {
            Session(Today.AddDays(-14), new SessionRow { CylinderId = "1", EndPressure = "120" }),
            Session(Today.AddDays(-7), new SessionRow { CylinderId = "1", StartPressure = "full" }),
            Session(Today.AddDays(-3)),
        };

        Assert.Equal("full", SessionRules.Create(Today, [Cylinder("1")], sessions).Rows.Single().StartPressure);
        Assert.Null(SessionRules.Create(Today, [Cylinder("2")], sessions).Rows.Single().StartPressure);
    }

    [Fact]
    public void Adding_missing_cylinders_keeps_existing_rows()
    {
        var session = Session(Today, new SessionRow { CylinderId = "1", Diver = "Thomas" });

        SessionRules.AddMissingCylinders(session, [Cylinder("1"), Cylinder("2")], []);

        Assert.Equal(["1", "2"], session.Rows.Select(r => r.CylinderId));
        Assert.Equal("Thomas", session.Rows[0].Diver);
    }

    [Fact]
    public void Kit_given_to_two_people_is_double_booked()
    {
        var session = Session(Today,
            new SessionRow { CylinderId = "1", BcdId = "bcd2", RegId = "reg1" },
            new SessionRow { CylinderId = "2", BcdId = "bcd2", RegId = "reg4", FinsId = "" },
            new SessionRow { CylinderId = "3", RegId = "reg5", FinsId = "" });

        Assert.Equal(["bcd2"], SessionRules.DoubleBooked(session));
    }

    [Fact]
    public void Tidy_turns_blank_values_into_nulls()
    {
        var session = Session(Today, new SessionRow { CylinderId = "1", StartPressure = " 200 ", BcdId = "", Diver = "  " });

        SessionRules.Tidy(session);

        var row = session.Rows.Single();
        Assert.Equal("200", row.StartPressure);
        Assert.Null(row.BcdId);
        Assert.Null(row.Diver);
        Assert.False(row.IsAssigned);
    }

    [Fact]
    public void Natural_sort_puts_numbers_in_order()
    {
        string[] names = ["10", "2", "BCD 11", "1", "BCD 9", "2a", "02"];

        var sorted = names.Order(NaturalComparer.Instance);

        Assert.Equal(["1", "2", "02", "2a", "10", "BCD 9", "BCD 11"], sorted);
    }

    [Fact]
    public void Sheet_labels_show_fins_by_size_and_avoid_doubled_prefixes()
    {
        Assert.Equal("6/7", new KitItem { Category = KitCategory.Fins, Name = "Fins A", Size = "6/7" }.SheetLabel());
        Assert.Equal("2 (10L)", new KitItem { Category = KitCategory.Cylinder, Name = "2", Size = "10L" }.SheetLabel());
        Assert.Equal("BCD 2", new KitItem { Category = KitCategory.Bcd, Name = "2" }.PrefixedLabel("BCD"));
        Assert.Equal("BCD 2", new KitItem { Category = KitCategory.Bcd, Name = "BCD 2" }.PrefixedLabel("BCD"));
        Assert.Equal("Open fins", new KitItem { Category = KitCategory.Fins, Name = "C", Size = "Open fins" }.PrefixedLabel("Fins"));
    }
}
