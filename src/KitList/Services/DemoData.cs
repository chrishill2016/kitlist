using KitList.Models;

namespace KitList.Services;

/// <summary>Sample data so the app can be tried before Firebase is set up.</summary>
public static class DemoData
{
    public static readonly AppUser User = new("demo@example.com", "Demo Admin", null);

    public static async Task EnsureSeededAsync(KitRepository repository)
    {
        if (await repository.GetMemberAsync(User.Email) is not null)
            return;

        await repository.SaveMemberAsync(new Member { Email = User.Email, Name = User.Name, Role = MemberRole.Admin });

        var today = ServiceRules.Today();
        var items = new List<KitItem>
        {
            Item(KitCategory.Bcd, "BCD 1", "Scubapro", "Hydros Pro", "SP-HP-10423", "M", KitCondition.Good, today.AddMonths(-3)),
            Item(KitCategory.Bcd, "BCD 2", "Mares", "Prestige", "MR-77810", "L", KitCondition.Fair, today.AddMonths(-11).AddDays(-10)),
            Item(KitCategory.Regulator, "Reg set A", "Apeks", "XTX50", "AP-55120", null, KitCondition.Good, today.AddMonths(-14)),
            Item(KitCategory.Regulator, "Reg set B", "Apeks", "XTX50", "AP-55121", null, KitCondition.NeedsRepair, null),
            Item(KitCategory.Cylinder, "12L steel #1", "Faber", "12L 232 bar", "FB-9921", "12L", KitCondition.Good, today.AddMonths(-6)),
            Item(KitCategory.Cylinder, "12L steel #2", "Faber", "12L 232 bar", "FB-9922", "12L", KitCondition.Good, today.AddMonths(-13)),
            Item(KitCategory.Fins, "Jet fins (pair 1)", "Scubapro", "Jet Fin", null, "L", KitCondition.Good, null),
        };

        foreach (var item in items)
            await repository.SaveItemAsync(item, User.Email);
    }

    private static KitItem Item(KitCategory category, string name, string make, string model, string? serial,
        string? size, KitCondition condition, DateOnly? lastServiced)
    {
        var item = new KitItem
        {
            Category = category, Name = name, Make = make, Model = model, SerialNumber = serial,
            Size = size, Condition = condition, Location = "Club store", Schedules = ServiceRules.DefaultSchedules(category),
        };
        if (lastServiced is { } date)
        {
            foreach (var schedule in item.Schedules)
                ServiceRules.ApplyServiceRecord(item, new ServiceRecord { Date = date, Type = schedule.Type, DoneBy = "Dive shop" });
        }
        return item;
    }
}
