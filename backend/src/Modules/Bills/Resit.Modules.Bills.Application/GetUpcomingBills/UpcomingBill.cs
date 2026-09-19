namespace Resit.Modules.Bills.Application.GetUpcomingBills;

public sealed record UpcomingBill(
    Guid Id,
    string Name,
    decimal Amount,
    DateOnly DueDate,
    string Recurrence,
    string Category,
    bool IsPaid,
    int DaysUntilDue,
    int NotifyDaysBefore);
