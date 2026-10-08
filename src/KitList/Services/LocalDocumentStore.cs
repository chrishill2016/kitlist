using System.Text.Json;
using Microsoft.JSInterop;

namespace KitList.Services;

/// <summary>Demo-mode store that keeps every document in one localStorage entry, keyed by path.</summary>
public sealed class LocalDocumentStore(IJSRuntime js) : IDocumentStore
{
    private const string StorageKey = "kitlist-demo";
    private Dictionary<string, string>? _docs;

    public async Task<IReadOnlyList<StoredDocument>> ListAsync(string collectionPath)
    {
        var docs = await LoadAsync();
        var prefix = collectionPath + "/";
        return docs
            .Where(d => d.Key.StartsWith(prefix, StringComparison.Ordinal) && !d.Key[prefix.Length..].Contains('/'))
            .Select(d => new StoredDocument(d.Key[prefix.Length..], d.Value))
            .ToList();
    }

    public async Task<string?> GetAsync(string documentPath) =>
        (await LoadAsync()).GetValueOrDefault(documentPath);

    public async Task SetAsync(string documentPath, string json)
    {
        (await LoadAsync())[documentPath] = json;
        await SaveAsync();
    }

    public async Task DeleteAsync(string documentPath)
    {
        (await LoadAsync()).Remove(documentPath);
        await SaveAsync();
    }

    private async Task<Dictionary<string, string>> LoadAsync()
    {
        if (_docs is null)
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            _docs = json is null ? [] : JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        }
        return _docs;
    }

    private async Task SaveAsync() =>
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(_docs));
}
