namespace KitList.Services;

public sealed record StoredDocument(string Id, string Json);

/// <summary>Minimal document database: Firestore in production, browser localStorage in demo mode.</summary>
public interface IDocumentStore
{
    Task<IReadOnlyList<StoredDocument>> ListAsync(string collectionPath);
    Task<string?> GetAsync(string documentPath);
    Task SetAsync(string documentPath, string json);
    Task DeleteAsync(string documentPath);
}
