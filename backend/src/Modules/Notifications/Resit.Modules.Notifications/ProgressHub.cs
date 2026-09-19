using Microsoft.AspNetCore.SignalR;

namespace Resit.Modules.Notifications;

public sealed class ProgressHub : Hub
{
    public static string HouseholdGroup(Guid householdId) => $"household:{householdId}";

    public override async Task OnConnectedAsync()
    {
        var householdId = Context.User?.FindFirst("household_id")?.Value;

        if (Guid.TryParse(householdId, out var parsed))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, HouseholdGroup(parsed));
        }

        await base.OnConnectedAsync();
    }
}
