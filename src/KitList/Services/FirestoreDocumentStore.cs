using System.Text.Json;
using Microsoft.JSInterop;

namespace KitList.Services;

public sealed class FirestoreDocumentStore(FirebaseJs firebase) : IDocumentStore
{
    public async Task<IReadOnlyList<StoredDocument>> ListAsync(string collectionPath)
    {
        var module = await firebase.GetModuleAsync();
        var json = await module.InvokeAsync<string>("list", collectionPath);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray()
            .Select(e => new StoredDocument(e.GetProperty("id").GetString()!, e.GetProperty("data").GetRawText()))
            .ToList();
    }

    public async Task<string?> GetAsync(string documentPath)
    {
        var module = await firebase.GetModuleAsync();
        return await module.InvokeAsync<string?>("get", documentPath);
    }

    public async Task SetAsync(string documentPath, string json)
    {
        var module = await firebase.GetModuleAsync();
        await module.InvokeVoidAsync("set", documentPath, json);
    }

    public async Task DeleteAsync(string documentPath)
    {
        var module = await firebase.GetModuleAsync();
        await module.InvokeVoidAsync("remove", documentPath);
    }
}
