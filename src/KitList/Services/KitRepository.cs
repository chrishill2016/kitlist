using System.Text.Json;
using System.Text.Json.Serialization;
using KitList.Models;

namespace KitList.Services;

/// <summary>
/// Reads and writes the club's data. Layout:
///   items/{itemId}                  KitItem (schedules and service log embedded)
///   items/{itemId}/photos/{photoId} KitPhoto (kept separate so lists stay small)
///   members/{lower-case email}      Member
/// </summary>
public sealed class KitRepository(IDocumentStore store)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<List<KitItem>> GetItemsAsync()
    {
        var docs = await store.ListAsync("items");
        return docs.Select(d => Read<KitItem>(d.Json, d.Id)).OrderBy(i => i.Category).ThenBy(i => i.Name).ToList();
    }

    public async Task<KitItem?> GetItemAsync(string id)
    {
        var json = await store.GetAsync($"items/{id}");
        return json is null ? null : Read<KitItem>(json, id);
    }

    public Task SaveItemAsync(KitItem item, string? updatedBy)
    {
        if (string.IsNullOrEmpty(item.Id))
            item.Id = Guid.NewGuid().ToString("N");
        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.UpdatedBy = updatedBy;
        return store.SetAsync($"items/{item.Id}", JsonSerializer.Serialize(item, Json));
    }

    public async Task DeleteItemAsync(string id)
    {
        foreach (var photo in await store.ListAsync($"items/{id}/photos"))
            await store.DeleteAsync($"items/{id}/photos/{photo.Id}");
        await store.DeleteAsync($"items/{id}");
    }

    public async Task<List<KitPhoto>> GetPhotosAsync(string itemId)
    {
        var docs = await store.ListAsync($"items/{itemId}/photos");
        return docs.Select(d => Read<KitPhoto>(d.Json, d.Id)).OrderBy(p => p.AddedAt).ToList();
    }

    public Task AddPhotoAsync(string itemId, KitPhoto photo) =>
        store.SetAsync($"items/{itemId}/photos/{photo.Id}", JsonSerializer.Serialize(photo, Json));

    public Task DeletePhotoAsync(string itemId, string photoId) =>
        store.DeleteAsync($"items/{itemId}/photos/{photoId}");

    public async Task<List<Member>> GetMembersAsync()
    {
        var docs = await store.ListAsync("members");
        return docs.Select(d => Read<Member>(d.Json, d.Id)).OrderBy(m => m.Name ?? m.Email).ToList();
    }

    public async Task<Member?> GetMemberAsync(string email)
    {
        var key = MemberKey(email);
        var json = await store.GetAsync($"members/{key}");
        return json is null ? null : Read<Member>(json, key);
    }

    public Task SaveMemberAsync(Member member)
    {
        member.Email = MemberKey(member.Email);
        return store.SetAsync($"members/{member.Email}", JsonSerializer.Serialize(member, Json));
    }

    public Task DeleteMemberAsync(string email) => store.DeleteAsync($"members/{MemberKey(email)}");

    public static string MemberKey(string email) => email.Trim().ToLowerInvariant();

    private static T Read<T>(string json, string id)
    {
        var value = JsonSerializer.Deserialize<T>(json, Json)!;
        switch (value)
        {
            case KitItem item: item.Id = id; break;
            case KitPhoto photo: photo.Id = id; break;
            case Member member: member.Email = id; break;
        }
        return value;
    }
}
