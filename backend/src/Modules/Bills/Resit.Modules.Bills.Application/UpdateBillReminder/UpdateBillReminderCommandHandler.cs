using Mediator;
using Resit.Modules.Bills.Application.Abstractions;

namespace Resit.Modules.Bills.Application.UpdateBillReminder;

public sealed class UpdateBillReminderCommandHandler(IBillRepository repository)
    : IRequestHandler<UpdateBillReminderCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateBillReminderCommand command, CancellationToken cancellationToken)
    {
        var bill = await repository.GetAsync(command.BillId, command.HouseholdId, cancellationToken);
        if (bill is null)
        {
            return false;
        }

        bill.Update(command.Name, command.Amount, command.DueDate, command.Recurrence, command.Category, command.NotifyDaysBefore);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
