using Mediator;
using Resit.Modules.Bills.Application.Abstractions;
using Resit.Modules.Bills.Domain;

namespace Resit.Modules.Bills.Application.CreateBillReminder;

public sealed class CreateBillReminderCommandHandler(IBillRepository repository)
    : IRequestHandler<CreateBillReminderCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateBillReminderCommand command, CancellationToken cancellationToken)
    {
        var bill = Bill.Create(
            command.HouseholdId,
            command.CreatedByUserId,
            command.Name,
            command.Amount,
            command.DueDate,
            command.Recurrence,
            command.Category,
            command.NotifyDaysBefore);

        await repository.AddAsync(bill, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return bill.Id;
    }
}
