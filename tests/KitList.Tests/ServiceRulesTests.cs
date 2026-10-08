using KitList.Models;

namespace KitList.Tests;

public class ServiceRulesTests
{
    private static readonly DateOnly Today = new(2026, 10, 8);

    [Fact]
    public void Schedule_without_last_done_has_no_record()
    {
        var schedule = new ServiceSchedule { Type = "Annual service", IntervalMonths = 12 };

        Assert.Equal(DueStatus.NoRecord, ServiceRules.GetStatus(schedule, Today));
    }

    [Theory]
    [InlineData("2025-10-07", DueStatus.Overdue)]  // due yesterday
    [InlineData("2025-10-08", DueStatus.DueSoon)]  // due today
    [InlineData("2025-11-07", DueStatus.DueSoon)]  // due in 30 days
    [InlineData("2025-11-08", DueStatus.Ok)]       // due in 31 days
    public void Status_depends_on_next_due_date(string lastDone, DueStatus expected)
    {
        var schedule = new ServiceSchedule { Type = "Annual service", IntervalMonths = 12, LastDone = DateOnly.Parse(lastDone) };

        Assert.Equal(expected, ServiceRules.GetStatus(schedule, Today));
    }

    [Fact]
    public void Item_status_is_the_worst_of_its_schedules()
    {
        var item = new KitItem
        {
            Schedules =
            [
                new() { Type = "Visual inspection", IntervalMonths = 12, LastDone = Today.AddMonths(-1) },
                new() { Type = "Hydrostatic test", IntervalMonths = 60, LastDone = Today.AddMonths(-61) },
            ],
        };

        Assert.Equal(DueStatus.Overdue, ServiceRules.GetStatus(item, Today));
    }

    [Fact]
    public void Item_without_schedules_is_not_tracked()
    {
        Assert.Equal(DueStatus.NotTracked, ServiceRules.GetStatus(new KitItem(), Today));
    }

    [Fact]
    public void Cylinders_default_to_visual_and_hydro_schedules()
    {
        var schedules = ServiceRules.DefaultSchedules(KitCategory.Cylinder);

        Assert.Equal(["Visual inspection", "Hydrostatic test"], schedules.Select(s => s.Type));
        Assert.Equal([12, 60], schedules.Select(s => s.IntervalMonths));
        Assert.Empty(ServiceRules.DefaultSchedules(KitCategory.Fins));
    }

    [Fact]
    public void Recording_a_service_moves_the_matching_schedule_forward()
    {
        var item = new KitItem { Schedules = ServiceRules.DefaultSchedules(KitCategory.Cylinder) };

        ServiceRules.ApplyServiceRecord(item, new ServiceRecord { Date = Today, Type = "visual inspection " });

        Assert.Equal(Today, item.Schedules[0].LastDone);
        Assert.Null(item.Schedules[1].LastDone);
        Assert.Single(item.ServiceLog);
    }

    [Fact]
    public void Recording_an_older_service_keeps_the_later_last_done_date()
    {
        var item = new KitItem { Schedules = [new() { Type = "Annual service", IntervalMonths = 12, LastDone = Today }] };

        ServiceRules.ApplyServiceRecord(item, new ServiceRecord { Date = Today.AddYears(-1), Type = "Annual service" });
        ServiceRules.ApplyServiceRecord(item, new ServiceRecord { Date = Today.AddDays(-10), Type = "Annual service" });

        Assert.Equal(Today, item.Schedules[0].LastDone);
        Assert.Equal([Today.AddDays(-10), Today.AddYears(-1)], item.ServiceLog.Select(r => r.Date));
    }
}
