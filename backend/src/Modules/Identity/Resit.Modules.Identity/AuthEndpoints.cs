using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Resit.Modules.Identity;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            IdentityDbContext dbContext,
            JwtTokenService tokenService,
            CancellationToken cancellationToken) =>
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

            if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            var householdName = await dbContext.Households
                .Where(h => h.Id == user.HouseholdId)
                .Select(h => h.Name)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

            var token = tokenService.IssueToken(user);
            return Results.Ok(new LoginResponse(token, user.HouseholdId, user.Id, user.DisplayName, user.Initials, householdName));
        });

        return app;
    }

    private sealed record LoginRequest(string Email, string Password);

    private sealed record LoginResponse(string Token, Guid HouseholdId, Guid UserId, string DisplayName, string Initials, string HouseholdName);
}
