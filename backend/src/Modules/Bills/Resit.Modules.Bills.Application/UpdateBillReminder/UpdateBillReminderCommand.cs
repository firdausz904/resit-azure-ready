using Mediator;
using Resit.Modules.Bills.Domain;

namespace Resit.Modules.Bills.Application.UpdateBillReminder;

public sealed record UpdateBillReminderCommand(
    Guid HouseholdId,
    Guid BillId,
    string Name,
    decimal Amount,
    DateOnly DueDate,
    BillRecurrence Recurrence,
    string Category,
    int NotifyDaysBefore) : IRequest<bool>;
