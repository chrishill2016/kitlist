using KitList.Models;
using Microsoft.JSInterop;

namespace KitList.Services;

/// <summary>
/// Tracks who is signed in (Google via Firebase Auth) and their club role.
/// In demo mode there is no Firebase: signing in just creates a local admin user.
/// </summary>
public sealed class AuthService(FirebaseOptions options, FirebaseJs firebase, KitRepository repository, IJSRuntime js) : IDisposable
{
    private const string DemoSignedInKey = "kitlist-demo-signed-in";

    private DotNetObjectReference<AuthService>? _selfRef;

    public bool IsDemo => !options.IsConfigured;
    public bool IsReady { get; private set; }
    public AppUser? User { get; private set; }
    public Member? Member { get; private set; }
    public string? Error { get; private set; }

    public bool CanEdit => Member?.Role is MemberRole.Editor or MemberRole.Admin;
    public bool IsAdmin => Member?.Role is MemberRole.Admin;

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        if (IsDemo)
        {
            // Remember the demo sign-in across reloads, like Firebase does for real accounts.
            if (await js.InvokeAsync<string?>("localStorage.getItem", DemoSignedInKey) is not null)
                await SignInAsync();
            IsReady = true;
            Changed?.Invoke();
            return;
        }

        try
        {
            _selfRef = DotNetObjectReference.Create(this);
            var module = await firebase.GetModuleAsync();
            // Calls OnAuthChanged once the saved sign-in (if any) has been restored.
            await module.InvokeVoidAsync("init", options, _selfRef);
        }
        catch (JSException ex)
        {
            Error = $"Couldn't start Firebase: {ex.Message}";
            IsReady = true;
            Changed?.Invoke();
        }
    }

    public async Task SignInAsync()
    {
        Error = null;
        if (IsDemo)
        {
            await DemoData.EnsureSeededAsync(repository);
            await js.InvokeVoidAsync("localStorage.setItem", DemoSignedInKey, "1");
            await OnAuthChanged(DemoData.User);
            return;
        }

        try
        {
            var module = await firebase.GetModuleAsync();
            await module.InvokeVoidAsync("signIn");
        }
        catch (JSException ex)
        {
            Error = $"Sign-in failed: {ex.Message}";
            Changed?.Invoke();
        }
    }

    public async Task SignOutAsync()
    {
        if (IsDemo)
        {
            await js.InvokeVoidAsync("localStorage.removeItem", DemoSignedInKey);
            await OnAuthChanged(null);
            return;
        }

        var module = await firebase.GetModuleAsync();
        await module.InvokeVoidAsync("signOut");
    }

    /// <summary>Re-reads the signed-in user's member record, e.g. after an admin changes roles.</summary>
    public async Task RefreshMemberAsync()
    {
        Member = User is null ? null : await repository.GetMemberAsync(User.Email);
        Changed?.Invoke();
    }

    [JSInvokable]
    public async Task OnAuthChanged(AppUser? user)
    {
        User = user;
        Member = null;
        if (user is not null)
        {
            try
            {
                Member = await repository.GetMemberAsync(user.Email);
            }
            catch (JSException ex)
            {
                Error = $"Couldn't check your membership: {ex.Message}";
            }
        }
        IsReady = true;
        Changed?.Invoke();
    }

    public void Dispose() => _selfRef?.Dispose();
}
