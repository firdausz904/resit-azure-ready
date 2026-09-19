using Mediator;
using Resit.Modules.Bills.Domain;

namespace Resit.Modules.Bills.Application.CreateBillReminder;

public sealed record CreateBillReminderCommand(
    Guid HouseholdId,
    Guid CreatedByUserId,
    string Name,
    decimal Amount,
    DateOnly DueDate,
    BillRecurrence Recurrence,
    string Category,
    int NotifyDaysBefore) : IRequest<Guid>;
