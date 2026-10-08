using KitList.Models;
using Microsoft.JSInterop;

namespace KitList.Services;

/// <summary>Sample data so the app can be tried before Firebase is set up.</summary>
public static class DemoData
{
    public static readonly AppUser User = new("demo@example.com", "Demo Admin", null);

    public static async Task ResetAsync(IJSRuntime js)
    {
        await js.InvokeVoidAsync("localStorage.removeItem", LocalDocumentStore.StorageKey);
        await js.InvokeVoidAsync("location.reload");
    }

    public static async Task EnsureSeededAsync(KitRepository repository)
    {
        if (await repository.GetMemberAsync(User.Email) is not null)
            return;

        await repository.SaveMemberAsync(new Member { Email = User.Email, Name = User.Name, Role = MemberRole.Admin });

        var today = ServiceRules.Today();
        var items = new List<KitItem>();
        for (var n = 1; n <= 10; n++)
        {
            var size = n == 2 ? "10L" : null;
            var condition = n == 3 ? KitCondition.NeedsRepair : KitCondition.Good;
            items.Add(Item(KitCategory.Cylinder, $"{n}", "Faber", $"{size ?? "12L"} steel 232 bar", $"FB-99{n:00}", size, condition,
                today.AddMonths(n == 7 ? -13 : -n)));
        }
        foreach (var (name, size) in new[] { ("2", "M"), ("6", "S"), ("11", "L"), ("15", "XL") })
            items.Add(Item(KitCategory.Bcd, name, "Mares", "Prestige", $"MR-{name}0{name}", size, KitCondition.Good, today.AddMonths(-int.Parse(name) / 2)));
        foreach (var name in new[] { "1", "2", "4", "5" })
            items.Add(Item(KitCategory.Regulator, name, "Apeks", "XTX50", $"AP-55{name}", null, KitCondition.Good,
                name == "5" ? today.AddMonths(-14) : today.AddMonths(-3)));
        items.Add(Item(KitCategory.Fins, "Fins A", "Mares", "Avanti", null, "6/7", KitCondition.Good, null));
        items.Add(Item(KitCategory.Fins, "Fins B", "Mares", "Avanti", null, "8/9", KitCondition.Good, null));
        items.Add(Item(KitCategory.Fins, "Fins C", "Scubapro", "Jet Fin", null, "Open fins", KitCondition.Good, null));

        foreach (var item in items)
            await repository.SaveItemAsync(item, User.Email);

        // Last week's sheet, so a new one has pressures to carry over.
        string Id(KitCategory category, string name) => items.First(i => i.Category == category && i.Name == name).Id;
        var cylinders = items.Where(i => i.Category == KitCategory.Cylinder).ToList();
        var lastWeek = SessionRules.Create(today.AddDays(-7), cylinders, []);
        string[] start = ["200", "180", "50", "full", "full", "full", "150", "120", "full", "190"];
        string?[] end = ["30", "100", null, "110", null, null, "80", "80", null, null];
        for (var n = 0; n < lastWeek.Rows.Count; n++)
        {
            lastWeek.Rows[n].StartPressure = start[n];
            lastWeek.Rows[n].EndPressure = end[n];
        }
        Assign(lastWeek.Rows[1], "Thomas", Id(KitCategory.Bcd, "2"), Id(KitCategory.Fins, "Fins A"), Id(KitCategory.Regulator, "1"));
        Assign(lastWeek.Rows[3], "Ian", Id(KitCategory.Bcd, "11"), null, Id(KitCategory.Regulator, "4"));
        Assign(lastWeek.Rows[6], "Chris", Id(KitCategory.Bcd, "6"), Id(KitCategory.Fins, "Fins C"), Id(KitCategory.Regulator, "2"));
        await repository.SaveSessionAsync(lastWeek, User.Email);
    }

    private static void Assign(SessionRow row, string diver, string? bcd, string? fins, string? reg)
    {
        row.Diver = diver;
        row.BcdId = bcd;
        row.FinsId = fins;
        row.RegId = reg;
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
