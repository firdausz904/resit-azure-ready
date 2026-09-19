using Hangfire;
using Resit.Modules.Bills.Application.Abstractions;
using Resit.Modules.Notifications;

namespace Resit.Modules.Bills.Infrastructure.Jobs;

public sealed class BillReminderCheckJob(IBillRepository repository, IRealtimeNotifier notifier)
{
    public async Task RunAsync(IJobCancellationToken jobCancellationToken)
    {
        var cancellationToken = jobCancellationToken.ShutdownToken;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var dueBills = await repository.GetDueForNotificationAsync(today, cancellationToken);

        foreach (var bill in dueBills)
        {
            var daysUntilDue = bill.DueDate.DayNumber - today.DayNumber;

            await notifier.NotifyReminderDueAsync(
                bill.HouseholdId,
                new ReminderDueUpdate(bill.Id, bill.Name, bill.Amount, bill.DueDate, daysUntilDue),
                cancellationToken);

            bill.MarkNotified();
        }

        await repository.SaveChangesAsync(cancellationToken);
    }
}
