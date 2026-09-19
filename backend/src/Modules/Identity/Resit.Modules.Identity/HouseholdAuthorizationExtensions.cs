using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Resit.Modules.Identity;

public static class HouseholdAuthorizationExtensions
{
    public static RouteGroupBuilder RequireHouseholdMatch(this RouteGroupBuilder group)
    {
        group.AddEndpointFilter(async (context, next) =>
        {
            var routeHouseholdId = context.HttpContext.Request.RouteValues["householdId"]?.ToString();
            var claimHouseholdId = context.HttpContext.User.FindFirst("household_id")?.Value;

            if (routeHouseholdId is null || claimHouseholdId is null || routeHouseholdId != claimHouseholdId)
            {
                return Results.Forbid();
            }

            return await next(context);
        });

        return group.RequireAuthorization();
    }
}
