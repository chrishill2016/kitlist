using System.Text.Json.Serialization;

namespace KitList.Services;

/// <summary>The web app config from the Firebase console (Project settings → Your apps). These values are not secret.</summary>
public sealed class FirebaseOptions
{
    public string ApiKey { get; set; } = "";
    public string AuthDomain { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string AppId { get; set; } = "";

    [JsonIgnore]
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(ProjectId);
}
