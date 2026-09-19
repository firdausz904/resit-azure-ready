namespace Resit.Modules.Notifications;

public interface IRealtimeNotifier
{
    Task NotifyBatchProgressAsync(Guid householdId, BatchProgressUpdate update, CancellationToken cancellationToken);

    Task NotifyReceiptProcessedAsync(Guid householdId, ReceiptProcessedUpdate update, CancellationToken cancellationToken);

    Task NotifyReminderDueAsync(Guid householdId, ReminderDueUpdate update, CancellationToken cancellationToken);
}

public readonly record struct BatchProgressUpdate(
    Guid BatchId,
    int TotalCount,
    int ProcessedCount,
    int FailedCount);

public readonly record struct ReceiptProcessedUpdate(
    Guid BatchId,
    Guid ReceiptId,
    string FileName,
    string Status,
    string? Merchant,
    decimal? Total,
    string? FailureReason);

public readonly record struct ReminderDueUpdate(
    Guid BillId,
    string Name,
    decimal Amount,
    DateOnly DueDate,
    int DaysUntilDue);
