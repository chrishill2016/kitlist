using KitList.Models;
using KitList.Services;

namespace KitList.Tests;

public class KitRepositoryTests
{
    private sealed class InMemoryStore : IDocumentStore
    {
        public Dictionary<string, string> Docs { get; } = [];

        public Task<IReadOnlyList<StoredDocument>> ListAsync(string collectionPath)
        {
            var prefix = collectionPath + "/";
            IReadOnlyList<StoredDocument> result = Docs
                .Where(d => d.Key.StartsWith(prefix) && !d.Key[prefix.Length..].Contains('/'))
                .Select(d => new StoredDocument(d.Key[prefix.Length..], d.Value))
                .ToList();
            return Task.FromResult(result);
        }

        public Task<string?> GetAsync(string documentPath) => Task.FromResult(Docs.GetValueOrDefault(documentPath));

        public Task SetAsync(string documentPath, string json)
        {
            Docs[documentPath] = json;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string documentPath)
        {
            Docs.Remove(documentPath);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Items_round_trip_with_enums_as_strings()
    {
        var store = new InMemoryStore();
        var repository = new KitRepository(store);
        var item = new KitItem
        {
            Category = KitCategory.Cylinder,
            Name = "12L steel #1",
            Condition = KitCondition.NeedsRepair,
            PurchaseDate = new DateOnly(2024, 5, 1),
            PurchaseCost = 249.99m,
            Schedules = ServiceRules.DefaultSchedules(KitCategory.Cylinder),
        };

        await repository.SaveItemAsync(item, "kit@club.org");
        var loaded = await repository.GetItemAsync(item.Id);

        Assert.Contains("\"category\":\"Cylinder\"", store.Docs[$"items/{item.Id}"]);
        Assert.NotNull(loaded);
        Assert.Equal(item.Name, loaded.Name);
        Assert.Equal(KitCondition.NeedsRepair, loaded.Condition);
        Assert.Equal(new DateOnly(2024, 5, 1), loaded.PurchaseDate);
        Assert.Equal(2, loaded.Schedules.Count);
        Assert.Equal("kit@club.org", loaded.UpdatedBy);
    }

    [Fact]
    public async Task Deleting_an_item_removes_its_photos()
    {
        var store = new InMemoryStore();
        var repository = new KitRepository(store);
        var item = new KitItem { Name = "BCD 1" };
        await repository.SaveItemAsync(item, null);
        await repository.AddPhotoAsync(item.Id, new KitPhoto { DataUrl = "data:image/jpeg;base64,AA==" });

        await repository.DeleteItemAsync(item.Id);

        Assert.Empty(store.Docs);
    }

    [Fact]
    public async Task Members_are_keyed_by_lower_case_email()
    {
        var repository = new KitRepository(new InMemoryStore());

        await repository.SaveMemberAsync(new Member { Email = " Sam.Diver@Gmail.com ", Role = MemberRole.Editor });

        var member = await repository.GetMemberAsync("sam.diver@gmail.com");
        Assert.NotNull(member);
        Assert.Equal("sam.diver@gmail.com", member.Email);
        Assert.Equal(MemberRole.Editor, member.Role);
    }
}
