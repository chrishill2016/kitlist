using Microsoft.JSInterop;

namespace KitList.Services;

/// <summary>Lazily loads wwwroot/js/firebase.js, the thin wrapper over the Firebase JS SDK.</summary>
public sealed class FirebaseJs(IJSRuntime js) : IAsyncDisposable
{
    private Task<IJSObjectReference>? _module;

    public Task<IJSObjectReference> GetModuleAsync() =>
        _module ??= js.InvokeAsync<IJSObjectReference>("import", "./js/firebase.js").AsTask();

    public async ValueTask DisposeAsync()
    {
        if (_module is { IsCompletedSuccessfully: true })
            await (await _module).DisposeAsync();
    }
}
