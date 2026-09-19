using Microsoft.AspNetCore.SignalR;

namespace Resit.Modules.Notifications;

public sealed class SignalRRealtimeNotifier(IHubContext<ProgressHub> hubContext) : IRealtimeNotifier
{
    public Task NotifyBatchProgressAsync(Guid householdId, BatchProgressUpdate update, CancellationToken cancellationToken) =>
        hubContext.Clients
            .Group(ProgressHub.HouseholdGroup(householdId))
            .SendAsync("BatchProgressUpdated", update, cancellationToken);

    public Task NotifyReceiptProcessedAsync(Guid householdId, ReceiptProcessedUpdate update, CancellationToken cancellationToken) =>
        hubContext.Clients
            .Group(ProgressHub.HouseholdGroup(householdId))
            .SendAsync("ReceiptProcessed", update, cancellationToken);

    public Task NotifyReminderDueAsync(Guid householdId, ReminderDueUpdate update, CancellationToken cancellationToken) =>
        hubContext.Clients
            .Group(ProgressHub.HouseholdGroup(householdId))
            .SendAsync("ReminderDue", update, cancellationToken);
}
