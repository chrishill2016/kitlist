namespace KitList.Models;

public enum MemberRole { Viewer, Editor, Admin }

/// <summary>A club member allowed into the app. Stored under members/{lower-case email}.</summary>
public sealed class Member
{
    public string Email { get; set; } = "";
    public string? Name { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Viewer;
}

public sealed record AppUser(string Email, string? Name, string? PhotoUrl);
