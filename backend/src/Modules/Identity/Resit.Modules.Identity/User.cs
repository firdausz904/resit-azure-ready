namespace Resit.Modules.Identity;

public sealed class User
{
    public Guid Id { get; private init; }
    public Guid HouseholdId { get; private init; }
    public string Email { get; private init; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string DisplayName { get; private init; } = string.Empty;
    public string Initials { get; private init; } = string.Empty;

    private User() { }

    public static User Create(Guid householdId, string email, string displayName, string initials) => new()
    {
        Id = Guid.NewGuid(),
        HouseholdId = householdId,
        Email = email.ToLowerInvariant(),
        DisplayName = displayName,
        Initials = initials
    };

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;
}

public sealed class Household
{
    public Guid Id { get; private init; }
    public string Name { get; private init; } = string.Empty;

    private Household() { }

    public static Household Create(string name) => new() { Id = Guid.NewGuid(), Name = name };
}
